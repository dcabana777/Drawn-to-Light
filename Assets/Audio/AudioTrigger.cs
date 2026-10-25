using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;

    [SerializeField, Min(0f)] private float volume = 1f;

    private int previousClip = -1;

    public void Play()
    {
        if (GameAudioManager.Instance == null || clips == null || clips.Length == 0)
            return;

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

        GameAudioManager.Instance.PlayOneShot(clips[index], volume);
    }
}
