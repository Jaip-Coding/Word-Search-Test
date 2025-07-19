using UnityEngine;
using UnityEngine.UI;

public class UILineConnector : MonoBehaviour
{
    public RectTransform uiLineImage;
    public Transform targetA;
    public Transform targetB;
    public Canvas canvas;

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

        // Handle input: left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }

        if (isDrawing)
        {
            // targetB stays in place, targetA follows the mouse
            targetA.position = SnapToGrid(mouseWorldPos);
            targetB.position = SnapToGrid(targetB.position); // ensure it's snapped

            DrawLine(targetA.position, targetB.position);
        }
        else
        {
            // Only targetB follows mouse
            targetB.position = SnapToGrid(mouseWorldPos);

            // Hide the line
            uiLineImage.sizeDelta = new Vector2(0f, uiLineImage.sizeDelta.y);
        }
    }

    // Snap to a grid based on grid origin and step size
    private Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round((position.x - gridOrigin.x) / gridSize) * gridSize + gridOrigin.x;
        float y = Mathf.Round((position.y - gridOrigin.y) / gridSize) * gridSize + gridOrigin.y;
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
}
