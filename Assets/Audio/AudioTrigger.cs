using System.Collections;
using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;

    [SerializeField, Min(0f)] private float volume = 1f;

    private int previousClip = -1;

    private AudioSource audioSource;
    private Coroutine durationRoutine;

    private void Awake()
    {
        audioSource = GameAudioManager.Instance.OneShotSource;
    }

    private AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return null;

        int index;

        if (clips.Length == 1)
            index = 0;
        else
        {
            do
            {
                index = Random.Range(0, clips.Length);
            }
            while (index == previousClip);
        }

        previousClip = index;

        return clips[index];
    }

    public void Play()
    {
        if (GameAudioManager.Instance == null)
            return;

        AudioClip clip = GetRandomClip();

        if (clip == null)
            return;

        GameAudioManager.Instance.PlayOneShot(clip, volume);
    }

    public void PlayForDuration(float duration)
    {
        if (durationRoutine != null)
        {
            StopCoroutine(durationRoutine);
            durationRoutine = null;
        }

        audioSource.Stop();

        if (duration <= 0f)
            return;

        AudioClip clip = GetRandomClip();

        if (clip == null)
            return;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        durationRoutine = StartCoroutine(StopAfterDuration(duration));
    }

    private IEnumerator StopAfterDuration(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);

        if (audioSource.isPlaying)
            audioSource.Stop();

        durationRoutine = null;
    }

    private void OnDisable()
    {
        if (durationRoutine != null)
        {
            StopCoroutine(durationRoutine);
            durationRoutine = null;
        }

        if (audioSource != null)
            audioSource.Stop();
    }
}
