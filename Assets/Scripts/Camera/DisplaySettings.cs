using UnityEngine;

public class DisplaySettings : MonoBehaviour
{
    public void SetWindowed()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    public void SetFullScreen()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }
}
