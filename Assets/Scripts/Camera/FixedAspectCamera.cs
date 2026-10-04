using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    [Header("Reference Aspect Ratio")]
    [SerializeField] private int referenceWidth = 16;
    [SerializeField] private int referenceHeight = 9;

    private Camera cam;

    private int lastScreenWidth;
    private int lastScreenHeight;

    public float TargetAspect => (float)referenceWidth / referenceHeight;

    private void Start()
    {
        cam = GetComponent<Camera>();

        ApplyAspectRatio();
    }

    private void Update()
    {
        if (Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight)
        {
            ApplyAspectRatio();
        }
    }

    private void ApplyAspectRatio()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        lastScreenHeight = Screen.height;
        lastScreenWidth = Screen.width;

        float windowAspect = (float)Screen.width / Screen.height;

        float scaleHeight = windowAspect / TargetAspect;

        if (scaleHeight < 1f)
        {
            // Window is relatively taller
            // Adds bars to top and bottom

            cam.rect = new Rect(0f, (1f - scaleHeight) * 0.5f, 1f, scaleHeight);
        }
        else
        {
            // Window is relatively wider
            // Add bars to left and right

            float scaleWidth = 1f / scaleHeight;

            cam.rect = new Rect((1f - scaleWidth) * 0.5f, 0f, scaleWidth, 1f);
        }
    }
}