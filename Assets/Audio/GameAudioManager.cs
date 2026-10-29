using System.Collections.Generic;
using UnityEngine;

public enum SharedLoopType
{
    LightFlicker,
    Rotation,
    Movement
}

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource oneShotSource;
    [SerializeField] private AudioSource lightFlickerSource;
    [SerializeField] private AudioSource rotationSource;
    [SerializeField] private AudioSource movementSource;

    public AudioSource OneShotSource => oneShotSource;

    private readonly Dictionary<Object, float> flickerRequests = new();
    private readonly Dictionary<Object, float> rotationRequests = new();
    private readonly Dictionary<Object, float> movementRequests = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        UpdateLoop(lightFlickerSource, flickerRequests);

        UpdateLoop(rotationSource, rotationRequests);

        UpdateLoop(movementSource, movementRequests);
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null || oneShotSource == null)
            return;

        oneShotSource.PlayOneShot(clip, Mathf.Max(0f, volume));
    }

    public void SetLoopRequest(SharedLoopType type, Object owner, bool active, float volume)
    {
        if (owner == null)
            return;

        Dictionary<Object, float> requests = GetRequests(type);

        if (active)
        {
            requests[owner] = Mathf.Max(0f, volume);
        }
        else
        {
            requests.Remove(owner);
        }
    }

    public void RemoveAllRequests(Object owner)
    {
        flickerRequests.Remove(owner);
        rotationRequests.Remove(owner);
        movementRequests.Remove(owner);
    }

    private Dictionary<Object, float> GetRequests(SharedLoopType type)
    {
        switch (type)
        {
            case SharedLoopType.LightFlicker:
                return flickerRequests;

            case SharedLoopType.Rotation:
                return rotationRequests;

            case SharedLoopType.Movement:
                return movementRequests;

            default:
                return flickerRequests;
        }
    }

    private void UpdateLoop(AudioSource source, Dictionary<Object, float> requests)
    {
        if (source == null)
            return;

        float highestVolume = 0f;

        List<Object> deadOwners = null;

        foreach (var request in requests)
        {
            if (request.Key == null)
            {
                deadOwners ??= new List<Object>();
                deadOwners.Add(request.Key);
                continue;
            }

            highestVolume = Mathf.Max(highestVolume, request.Value);
        }

        if (deadOwners != null)
        {
            foreach (Object owner in deadOwners)
                requests.Remove(owner);
        }

        if (highestVolume > 0f)
        {
            source.volume = highestVolume;

            if (!source.isPlaying)
                source.Play();
        }
        else
        {
            if (source.isPlaying)
                source.Stop();
        }
    }
}
