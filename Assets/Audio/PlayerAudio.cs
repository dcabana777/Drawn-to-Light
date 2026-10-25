using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField, Min(0f)] private float footstepVolume = 1f;

    [Header("Jump")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField, Min(0f)] private float jumpVolume = 1f;

    [Header("Landing")]
    [SerializeField] private AudioClip landingClip;
    [SerializeField, Min(0f)] private float landingVolume = 1f;
    [SerializeField, Min(0f)] private float minLandingInterval = 0.25f;
    private float landingIntervalTimer;

    private int previousFootstep = -1;

    private void FixedUpdate()
    {
        if (landingIntervalTimer > 0f)
            landingIntervalTimer -= Time.fixedDeltaTime;
    }

    public void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0 || GameAudioManager.Instance == null)
            return;

        int index;

        if (footstepClips.Length == 1)
            index = 0;
        else
        {
            do
            {
                index = Random.Range(0, footstepClips.Length);
            }
            while (index == previousFootstep);
        }

        previousFootstep = index;

        Debug.Log("Playing footstep: " + index, this);

        GameAudioManager.Instance.PlayOneShot(footstepClips[index], footstepVolume);
    }

    public void PlayJump()
    {
        if (jumpClip == null || GameAudioManager.Instance == null)
            return;

        Debug.Log("Playing jump.", this);

        GameAudioManager.Instance.PlayOneShot(jumpClip, jumpVolume);
    }

    public void PlayLanding()
    {
        if (landingClip == null || GameAudioManager.Instance == null)
            return;

        Debug.Log("Playing landing.", this);

        // avoid playing the sound if it has played too recently
        if (landingIntervalTimer > 0f)
            return;

        GameAudioManager.Instance.PlayOneShot(landingClip, landingVolume);

        landingIntervalTimer = minLandingInterval;
    }
}
