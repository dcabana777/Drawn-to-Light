using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class WorldStateLight2D : MonoBehaviour
{
    public enum QueryMode
    {
        Automatic,
        Collider
    }

    [Header("Query")]
    [SerializeField] public QueryMode queryMode = QueryMode.Automatic;

    [Tooltip("Used when QueryMode is Collider or as a fallback " +
        "for Light2D types that cannot easily be reproduced on the CPU.")]
    [SerializeField] private Collider2D queryCollider;

    [SerializeField, Min(0f)] private float colliderFalloffDistance = 0f;

    [Header("Falloff Approximation")]
    [Tooltip("1 is linear. Values above 1 make the CPU-side light " +
        "more concentrated. Values below 1 broaden it.")]
    [SerializeField] private float falloffPower = 1f;

    [SerializeField] private bool useLightIntensity;

    private Light2D light;

    private void Awake()
    {
        light = GetComponent<Light2D>();
    }

    private void OnEnable()
    {
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.RegisterLight(this);
        }
    }

    private void OnDisable()
    {
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.UnregisterLight(this);
        }
    }

    private void Start()
    {
        // Handles cases where this light's onEnable ran before WorldStateManager.Awake
        if (WorldStateManager.Instance != null)
        {
            WorldStateManager.Instance.RegisterLight(this);
        }
    }

    public float Evaluate(Vector2 worldPosition)
    {
        if (!isActiveAndEnabled || light == null || !light.enabled)
            return 0f;

        float shapeAmount;

        if (queryMode == QueryMode.Collider)
        {
            shapeAmount = EvaluateCollider(worldPosition);
        }
        else
        {
            shapeAmount = EvaluateAutomatic(worldPosition);
        }

        float intensityMultiplier = useLightIntensity ? Mathf.Max(0f, light.intensity) : 1f;

        return Mathf.Clamp01(shapeAmount * intensityMultiplier);
    }

    private float EvaluateAutomatic(Vector2 worldPosition)
    {
        switch (light.lightType)
        {
            case Light2D.LightType.Global:
                return 1f;

            case Light2D.LightType.Point:
                return EvaluatePointLight(worldPosition);

            case Light2D.LightType.Freeform:
                return EvaluateFreeformLight(worldPosition);

            case Light2D.LightType.Sprite:
                return EvaluateCollider(worldPosition);

            default:
                return EvaluateCollider(worldPosition);
                
        }
    }

    private float EvaluatePointLight(Vector2 worldPosition)
    {
        Vector2 localPosition = transform.InverseTransformPoint(worldPosition);

        float distance = localPosition.magnitude;

        float innerRadius = Mathf.Max(0f, light.pointLightInnerRadius);

        float outerRadius = Mathf.Max(innerRadius + 0.0001f, light.pointLightOuterRadius);

        if (distance <= innerRadius)
            return 1f;

        if (distance >= outerRadius)
            return 0f;

        float amount = 1f - Mathf.InverseLerp(innerRadius, outerRadius, distance);

        return Mathf.Pow(Mathf.Clamp01(amount), falloffPower);
    }


    private float EvaluateFreeformLight(Vector2 worldPosition)
    {
        Vector2 localPosition = transform.InverseTransformPoint(worldPosition);

        Vector3[] path = light.shapePath;

        if (path == null || path.Length < 3)
            return 0f;

        if (PointInsidePolygon(localPosition, path))
        {
            return 1f;
        }

        float falloffDistance = Mathf.Max(0f, light.shapeLightFalloffSize);

        if (falloffDistance <= 0f)
            return 0f;

        float distance = DistanceToPolygon(localPosition, path);

        if (distance >= falloffDistance)
            return 0f;

        float amount = 1f - distance / falloffDistance;

        return Mathf.Pow(Mathf.Clamp01(amount), falloffPower);
    }


    private float EvaluateCollider(Vector2 worldPosition)
    {
        if (queryCollider == null || !queryCollider.enabled)
        {
            return 0f;
        }

        if (queryCollider.OverlapPoint(worldPosition))
        {
            return 1f;
        }

        if (colliderFalloffDistance <= 0f)
        {
            return 0f;
        }

        Vector2 closestPoint = queryCollider.ClosestPoint(worldPosition);

        float distance = Vector2.Distance(worldPosition, closestPoint);

        if (distance >= colliderFalloffDistance)
            return 0f;

        float amount = 1f - distance / colliderFalloffDistance;

        return Mathf.Pow(Mathf.Clamp01(amount), falloffPower);
    }


    private bool PointInsidePolygon(Vector2 point, Vector3[] polygon)
    {
        bool inside = false;

        int j = polygon.Length - 1;

        for (int i = 0; i < polygon.Length; j = i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[j];

            bool crosses = ((a.y > point.y) != (b.y > point.y)) &&
                (point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x);

            if (crosses)
            {
                inside = !inside;
            }
        }

        return inside;
    }


    private float DistanceToPolygon(Vector2 point, Vector3[] polygon)
    {
        float nearest = float.PositiveInfinity;

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 a = polygon[i];

            Vector2 b = polygon[(i + 1) % polygon.Length];

            float distance = DistanceToSegment(point, a, b);

            nearest = Mathf.Min(nearest, distance);
        }

        return nearest;
    }


    private float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;

        float lengthSquared = ab.sqrMagnitude;

        if (lengthSquared <= Mathf.Epsilon)
        {
            return Vector2.Distance(point, a);
        }

        float t = Vector2.Dot(point - a, ab) / lengthSquared;

        t = Mathf.Clamp01(t);

        Vector2 closest = a + ab * t;

        return Vector2.Distance(point, closest);
    }
}
