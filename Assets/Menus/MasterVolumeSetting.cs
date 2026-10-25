using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeSetting : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";

    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        AudioListener.volume = volume;

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(volume);
        }
    }

    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        AudioListener.volume = volume;

        PlayerPrefs.SetFloat(VolumeKey, volume);

        PlayerPrefs.Save();
    }
}
