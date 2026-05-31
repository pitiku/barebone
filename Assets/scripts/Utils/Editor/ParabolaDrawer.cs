using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ParabolaDrawer : MonoBehaviour
{
    public Transform startPoint; // Drag the start point GameObject here
    public Transform endPoint;   // Drag the end point GameObject here
    public float height = 5f;    // Control the height of the parabola
    public int resolution = 50;  // Number of points in the parabola

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        DrawParabola();
    }

    void DrawParabola()
    {
        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;

        // Ensure LineRenderer has enough points
        lineRenderer.positionCount = resolution;

        // Calculate the parabola points
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1); // Normalized time [0, 1]
            Vector3 point = CalculateParabolaPoint(start, end, height, t);
            lineRenderer.SetPosition(i, point);
        }
    }

    Vector3 CalculateParabolaPoint(Vector3 start, Vector3 end, float height, float t)
    {
        // Linear interpolation between start and end
        Vector3 mid = Vector3.Lerp(start, end, t);

        // Calculate height using a quadratic equation
        float parabolaHeight = 4 * height * t * (1 - t);

        // Add height to the y-component
        mid.y += parabolaHeight;

        return mid;
    }

    
    void Update()
    {
        if (lineRenderer != null)
        {
            DrawParabola(); // Redraw when values change in the inspector
        }
    }

    void OnDrawGizmos()
    {
        if (lineRenderer != null)
        {
            DrawParabola(); // Redraw when values change in the inspector
        }
    }
}