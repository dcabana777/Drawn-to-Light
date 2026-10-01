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

        float targetAspect = TargetAspect;

        float windowAspect = (float)Screen.width / Screen.height;

        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // Window is relatively taller
            // Adds bars to top and bottom

            Rect rect = cam.rect;

            rect.width = 1f;
            rect.height = scaleHeight;

            rect.x = 0f;
            rect.y = (1f - scaleHeight) * 0.5f;

            cam.rect = rect;
        }
        else
        {
            // Window is relatively wider
            // Add bars to left and right

            float scaleWidth = 1f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1f;

            rect.x = (1f - scaleWidth) * 0.5f;
            rect.y = 0f;

            cam.rect = rect;
        }
    }
}
