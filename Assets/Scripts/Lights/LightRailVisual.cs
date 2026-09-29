using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LightRail))]
[RequireComponent(typeof(LineRenderer))]
public class LightRailVisual : MonoBehaviour
{
    [Header("Visual Settings")]
    [Min(2)]
    [SerializeField] private int visualResolution = 50;

    [SerializeField] private float railWidth = 0.15f;

    private LightRail rail;
    private LineRenderer lineRenderer;

    private void OnEnable()
    {
        GetReferences();
        RefreshVisual();
    }

    private void OnValidate()
    {
        GetReferences();
        RefreshVisual();
    }

    private void Update()
    {
        // Keeps the visual updated while editing the rail
        // in the Scene/Inspector.
        if (!Application.isPlaying)
        {
            RefreshVisual();
        }
    }

    private void GetReferences()
    {
        if (rail == null)
            rail = GetComponent<LightRail>();

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }

    private void RefreshVisual()
    {
        if (rail == null || lineRenderer == null)
            return;

        rail.RebuildRail();

        if (rail.Length <= 0f)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = railWidth;
        lineRenderer.endWidth = railWidth;

        lineRenderer.positionCount = visualResolution;

        for (int i = 0; i < visualResolution; i++)
        {
            float t =
                i / (float)(visualResolution - 1);

            Vector2 point =
                rail.GetPointAtNormalizedPosition(t);

            lineRenderer.SetPosition(
                i,
                new Vector3(point.x, point.y, transform.position.z)
            );
        }
    }
}