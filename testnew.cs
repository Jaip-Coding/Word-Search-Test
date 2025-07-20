using UnityEngine;
using UnityEngine.UI;

public class UILineConnector : MonoBehaviour
{
    public RectTransform uiLineImage;
    public Transform targetA;
    public Transform targetB;
    public Canvas canvas;
    public GameManager gameManager;

    private Camera cam;
    private float gridSize = 0.95f;
    private Vector2 gridOrigin = new Vector2(-2.685f, 4.25f);
    private float overshoot = 25f;

    private bool isDrawing = false;

    void Start()
    {
        cam = Camera.main;

        if (targetA != null) targetA.position = gridOrigin;
        if (targetB != null) targetB.position = gridOrigin;
    }

    void Update()
    {
        if (targetA == null || targetB == null || uiLineImage == null || canvas == null)
            return;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        if (Input.GetMouseButtonDown(0))
            isDrawing = true;
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;

            Vector2 bPos = new Vector2(targetB.position.x, targetB.position.y);
            Vector2 aPos = new Vector2(targetA.position.x, targetA.position.y);

            foreach (var word in GameManager.Instance.GetPlacedWords())
            {
                if (ApproximatelyEqual(word.screenStart, bPos) && ApproximatelyEqual(word.screenEnd, aPos))
                {
                    Debug.Log($"Word match found: {word.word}");
                }
            }
        }


        if (isDrawing)
        {
            var candidatePos = SnapToGrid(mouseWorldPos);
            if (candidatePos.HasValue && IsValidAngle(candidatePos.Value, targetB.position))
            {
                targetA.position = candidatePos.Value;
            }

            var snappedB = SnapToGrid(targetB.position);
            if (snappedB.HasValue)
                targetB.position = snappedB.Value;

            DrawLine(targetA.position, targetB.position);
        }
        else
        {
            var snappedB = SnapToGrid(mouseWorldPos);
            if (snappedB.HasValue)
                targetB.position = snappedB.Value;

            uiLineImage.sizeDelta = new Vector2(0f, uiLineImage.sizeDelta.y);
        }
    }

    private Vector3? SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round((position.x - gridOrigin.x) / gridSize) * gridSize + gridOrigin.x;
        float y = Mathf.Round((position.y - gridOrigin.y) / gridSize) * gridSize + gridOrigin.y;

        // If the snapped result is outside the valid bounds, return null
        if (x < -3f || x > 6f || y < -5f || y > 5f)
            return null;

        return new Vector3(x, y, 0f);
    }

    // Draw the line from worldPosA to worldPosB
    private void DrawLine(Vector3 worldPosA, Vector3 worldPosB)
    {
        Vector3 screenPosA = cam.WorldToScreenPoint(worldPosA);
        Vector3 screenPosB = cam.WorldToScreenPoint(worldPosB);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, screenPosA, canvas.worldCamera, out Vector2 localPointA);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, screenPosB, canvas.worldCamera, out Vector2 localPointB);

        Vector2 direction = localPointB - localPointA;
        Vector2 directionNormalized = direction.normalized;

        Vector2 extendedStart = localPointA - directionNormalized * overshoot;
        float extendedDistance = (localPointB - extendedStart).magnitude;

        uiLineImage.sizeDelta = new Vector2(extendedDistance + 25, uiLineImage.sizeDelta.y);
        uiLineImage.anchoredPosition = extendedStart;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        uiLineImage.rotation = Quaternion.Euler(0, 0, angle);
    }

    private bool IsValidAngle(Vector3 from, Vector3 to)
    {
        Vector2 dir = (to - from).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float[] validAngles = { 0, 90, -90, 180, -180, 135, -135, 45, -45 };

        foreach (float valid in validAngles)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(angle, valid)) <= 1f)
                return true;
        }

        return false;
    }

    private bool ApproximatelyEqual(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b) < 0.1f;
    }
}
