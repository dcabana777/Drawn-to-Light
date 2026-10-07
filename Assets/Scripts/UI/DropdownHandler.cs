using UnityEngine;
using TMPro;
public class DropdownHandler : MonoBehaviour
{
    public void HandleInput(int val)
    {
        switch (val)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Debug.Log("Enter fullscreen mode.");
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Debug.Log("Enter windowed mode.");
                break;
        }
    }
}
