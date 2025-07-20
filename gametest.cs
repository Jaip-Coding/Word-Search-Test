using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Grid Setup")]
    public int gridSize = 10;
    public GameObject letterTilePrefab;
    public Transform gridParent;

    [Header("Words")]
    public List<string> wordList;
    public TextMeshProUGUI wordsDisplay;

    public TextMeshProUGUI timerText;

    private float elapsedTime = 0f;

    private LetterTile[,] board;
    private List<LetterTile> selectedTiles = new List<LetterTile>();
    private HashSet<string> foundWords = new HashSet<string>();

    public static GameManager Instance { get; private set; }

    private List<WordPlacement> placedWords = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        GenerateBoard();
        DisplayWords();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void GenerateBoard()
    {
        board = new LetterTile[gridSize, gridSize];
        char[,] tempBoard = new char[gridSize, gridSize];

        // Initialize board with empty chars
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                tempBoard[x, y] = '\0';
            }
        }

        // Place each word
        foreach (string word in wordList)
        {
            string upperWord = word.ToUpper();
            if (TryPlaceWord(tempBoard, upperWord, out Vector2Int start, out Vector2Int end))
            {
                StartCoroutine(SaveWordScreenPositionsNextFrame(upperWord, start, end));
            }
        }

        // Instantiate tiles
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                GameObject tileGO = Instantiate(letterTilePrefab, gridParent);
                LetterTile tile = tileGO.GetComponent<LetterTile>();
                tile.gridPosition = new Vector2Int(x, y);

                char letter = tempBoard[x, y];
                if (letter == '\0')
                    letter = (char)('A' + Random.Range(0, 26));

                tile.SetLetter(letter);
                board[x, y] = tile;
            }
        }
    }

    void DisplayWords()
    {
        wordsDisplay.text = "Find:\n" + string.Join("\n", wordList);
    }

    bool TryPlaceWord(char[,] tempBoard, string word, out Vector2Int startPos, out Vector2Int endPos)
    {
        Vector2Int[] directions = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1),
        };

        startPos = Vector2Int.zero;
        endPos = Vector2Int.zero;

        for (int attempt = 0; attempt < 100; attempt++)
        {
            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            int startX = Random.Range(0, gridSize);
            int startY = Random.Range(0, gridSize);

            int endX = startX + dir.x * (word.Length - 1);
            int endY = startY + dir.y * (word.Length - 1);

            if (endX < 0 || endX >= gridSize || endY < 0 || endY >= gridSize)
                continue;

            bool canPlace = true;
            for (int i = 0; i < word.Length; i++)
            {
                int x = startX + dir.x * i;
                int y = startY + dir.y * i;
                char existing = tempBoard[x, y];

                if (existing != '\0' && existing != word[i])
                {
                    canPlace = false;
                    break;
                }
            }

            if (!canPlace)
                continue;

            // Place the word
            for (int i = 0; i < word.Length; i++)
            {
                int x = startX + dir.x * i;
                int y = startY + dir.y * i;
                tempBoard[x, y] = word[i];
            }

            startPos = new Vector2Int(startX, startY);
            endPos = new Vector2Int(endX, endY);
            return true;
        }

        Debug.LogWarning("Failed to place word: " + word);
        return false;
    }

    // Coroutine to ensure screen coords are calculated AFTER UI is placed
    private IEnumerator SaveWordScreenPositionsNextFrame(string word, Vector2Int start, Vector2Int end)
    {
        yield return null; // wait 1 frame

        LetterTile startTile = board[start.x, start.y];
        LetterTile endTile = board[end.x, end.y];

        Vector3 screenStart = Camera.main.ScreenToWorldPoint(startTile.transform.position);
        Vector3 screenEnd = Camera.main.ScreenToWorldPoint(endTile.transform.position);
        //Vector2 screenStart = RectTransformUtility.WorldToScreenPoint(null, startTile.transform.position);
        //Vector2 screenEnd = RectTransformUtility.WorldToScreenPoint(null, endTile.transform.position);

        placedWords.Add(new WordPlacement
        {
            word = word,
            screenStart = screenStart,
            screenEnd = screenEnd
        });

        Debug.Log($"Saved screen-space coords for '{word}': Start={screenStart}, End={screenEnd}");
    }

    public List<WordPlacement> GetPlacedWords()
    {
        return placedWords;
    }
}

public class WordPlacement
{
    public string word;
    public Vector2 screenStart;
    public Vector2 screenEnd;
}
