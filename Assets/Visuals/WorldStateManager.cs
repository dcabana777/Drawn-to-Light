using System.Collections.Generic;
using UnityEngine;

public enum WorldState
{
    Dark,
    Light
}

public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    public enum LightCombineMode
    {
        Additive,
        Maximum
    }

    [Header("World State")]
    [SerializeField, Range(0f, 1f)] private float defaultLightThreshold = 0.5f;
    [SerializeField] private LightCombineMode combineMode = LightCombineMode.Additive;

    private readonly HashSet<WorldStateLight2D> lights = new HashSet<WorldStateLight2D>();

    public float DefaultLightThreshold => defaultLightThreshold;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("More than one WorldStateManager exists.", this);

            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterLight(WorldStateLight2D light)
    {
        if (light != null)
        {
            lights.Add(light);
        }
    }

    public void UnregisterLight(WorldStateLight2D light)
    {
        if (light != null)
        {
            lights.Remove(light);
        }
    }

    public float GetLightAmount(Vector2 worldPosition)
    {
        float amount = 0f;

        foreach (WorldStateLight2D light in lights)
        {
            if (light == null)
                continue;

            float contribution = light.Evaluate(worldPosition);

            if (combineMode == LightCombineMode.Additive)
            {
                amount += contribution;

                if (amount >= 1f)
                    return 1f;
            }
            else
            {
                amount = Mathf.Max(amount, contribution);
            }
        }

        return Mathf.Clamp01(amount);
    }

    public bool IsLightWorld(Vector2 worldPosition)
    {
        return GetLightAmount(worldPosition) >= defaultLightThreshold;
    }

    public WorldState GetWorldState(Vector2 worldPosition)
    {
        return IsLightWorld(worldPosition) ? WorldState.Light : WorldState.Dark;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}