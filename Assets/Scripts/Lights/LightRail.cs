using System.Collections.Generic;
using UnityEngine;

public class LightRail : MonoBehaviour
{
    public enum RailEndBehavior
    {
        Bounce,
        Loop
    }

    [Header("Automatic Light Behavior")]
    [SerializeField]
    private RailEndBehavior endBehavior = RailEndBehavior.Bounce;
    
    [Header("Rail Shape")]
    [SerializeField]
    private Vector2[] points =
    {
        new Vector2(-5f, 0f),
        new Vector2(5f, 0f)
    };

    [SerializeField] private bool smooth = true;

    [Tooltip("Higher values make curved rails more accurate.")]
    [Min(1)]
    [SerializeField] private int samplesPerSegment = 12;

    [Header("Gizmos")]
    [SerializeField] private float pointGizmoRadius = 0.15f;

    private readonly List<Vector2> sampledPoints = new();
    private readonly List<float> cumulativeDistances = new();

    public RailEndBehavior EndBehavior => endBehavior;
    public float Length { get; private set; }

    private void Awake()
    {
        RebuildRail();
    }

    private void OnValidate()
    {
        RebuildRail();
    }

    public void RebuildRail()
    {
        sampledPoints.Clear();
        cumulativeDistances.Clear();
        Length = 0f;

        if (points == null || points.Length < 2)
            return;

        // Straight / segmented rail.
        if (!smooth || points.Length == 2)
        {
            foreach (Vector2 point in points)
            {
                sampledPoints.Add(point);
            }
        }
        // Smooth Catmull-Rom rail.
        else
        {
            sampledPoints.Add(points[0]);

            for (int segment = 0; segment < points.Length - 1; segment++)
            {
                for (int sample = 1; sample <= samplesPerSegment; sample++)
                {
                    float t = sample / (float)samplesPerSegment;

                    sampledPoints.Add(
                        EvaluateCatmullRom(segment, t)
                    );
                }
            }
        }

        // Calculate approximate length of the sampled path.
        cumulativeDistances.Add(0f);

        for (int i = 1; i < sampledPoints.Count; i++)
        {
            Length += Vector2.Distance(
                sampledPoints[i - 1],
                sampledPoints[i]
            );

            cumulativeDistances.Add(Length);
        }
    }

    public Vector3 GetPointAtDistance(float distance)
    {
        if (sampledPoints.Count < 2)
            return transform.position;

        distance = Mathf.Clamp(distance, 0f, Length);

        for (int i = 1; i < sampledPoints.Count; i++)
        {
            if (distance <= cumulativeDistances[i])
            {
                float segmentStartDistance =
                    cumulativeDistances[i - 1];

                float segmentEndDistance =
                    cumulativeDistances[i];

                float t = Mathf.InverseLerp(
                    segmentStartDistance,
                    segmentEndDistance,
                    distance
                );

                Vector2 localPoint = Vector2.Lerp(
                    sampledPoints[i - 1],
                    sampledPoints[i],
                    t
                );

                return transform.TransformPoint(localPoint);
            }
        }

        return transform.TransformPoint(
            sampledPoints[sampledPoints.Count - 1]
        );
    }

    public Vector3 GetPointAtNormalizedPosition(float position)
    {
        return GetPointAtDistance(
            Mathf.Clamp01(position) * Length
        );
    }

    private Vector2 EvaluateCatmullRom(int segment, float t)
    {
        Vector2 p1 = points[segment];
        Vector2 p2 = points[segment + 1];

        Vector2 p0 =
            segment > 0
                ? points[segment - 1]
                : p1;

        Vector2 p3 =
            segment + 2 < points.Length
                ? points[segment + 2]
                : p2;

        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f *
        (
            (2f * p1)
            + (-p0 + p2) * t
            + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
            + (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    private void OnDrawGizmos()
    {
        if (points == null || points.Length == 0)
            return;

        // Draw the actual control points.
        Gizmos.matrix = transform.localToWorldMatrix;

        for (int i = 0; i < points.Length; i++)
        {
            Gizmos.DrawWireSphere(points[i], pointGizmoRadius);
        }

        Gizmos.matrix = Matrix4x4.identity;

        if (points.Length < 2)
            return;

        // Generate the path for visualization.
        List<Vector3> previewPoints = new();

        if (!smooth || points.Length == 2)
        {
            foreach (Vector2 point in points)
            {
                previewPoints.Add(
                    transform.TransformPoint(point)
                );
            }
        }
        else
        {
            previewPoints.Add(
                transform.TransformPoint(points[0])
            );

            for (int segment = 0; segment < points.Length - 1; segment++)
            {
                for (int sample = 1; sample <= samplesPerSegment; sample++)
                {
                    float t =
                        sample / (float)samplesPerSegment;

                    Vector2 point =
                        EvaluateCatmullRom(segment, t);

                    previewPoints.Add(
                        transform.TransformPoint(point)
                    );
                }
            }
        }

        for (int i = 1; i < previewPoints.Count; i++)
        {
            Gizmos.DrawLine(
                previewPoints[i - 1],
                previewPoints[i]
            );
        }
    }
}