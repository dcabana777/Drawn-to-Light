using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class DarkFogController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera fogMaskCamera;

    [SerializeField] private SpriteRenderer fogMaskSurface;

    [SerializeField] private SpriteRenderer fogOverlay;

    [SerializeField] private Material fogMaterial;

    [Header("Fog")]
    [SerializeField] private Color fogColor = new Color(0f, 0f, 0f, 0.97f);

    [SerializeField, Min(0f)] private float revealMultiplier = 1f;

    [SerializeField, Min(0.01f)] private float revealPower = 1f;

    [Header("Fog Light Sources")]
    [Tooltip("Normal GameObject layers containing Light2D components " +
        "that should affect the fog mask.")]
    [SerializeField] private LayerMask fogLightSourceLayers;

    [Header("Mask Texture")]
    [SerializeField, Min(64)] private int maskHeight = 360;

    [Header("Object Placement")]
    [Tooltip("Distance in front of the camera where the " +
        "hidden mask sprite is placed.")]
    [SerializeField] private float maskSurfaceDistance = 10f;

    [Tooltip("Distance in front of the camera where the " +
        "visible fog overlay is placed.")]
    [SerializeField] private float fogOverlayDistance = 1f;
    [SerializeField, Min(1f)] private float overscan = 1f;

    private Camera mainCamera;

    private RenderTexture revealTexture;
    private Material runtimeFogMaterial;

    private int previousScreenWidth;
    private int previousScreenHeight;

    private int fogRevealLayer;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();

        fogRevealLayer =
            LayerMask.NameToLayer("FogReveal");

        if (fogRevealLayer == -1)
        {
            Debug.LogError(
                "DarkFogController requires a normal " +
                "Unity Layer named FogReveal."
            );

            enabled = false;
            return;
        }

        if (fogMaskCamera == null ||
            fogMaskSurface == null ||
            fogOverlay == null ||
            fogMaterial == null)
        {
            Debug.LogError(
                "DarkFogController is missing one " +
                "or more references."
            );

            enabled = false;
            return;
        }

        CreateRevealTexture();
        ConfigureMaskCamera();
        ConfigureMaskSurface();
        ConfigureFogOverlay();
    }

    private void LateUpdate()
    {
        if (Screen.width != previousScreenWidth ||
            Screen.height != previousScreenHeight)
        {
            CreateRevealTexture();
        }

        SyncCameraSettings();
        ResizeSurfaces();
    }

    private void CreateRevealTexture()
    {
        if (revealTexture != null)
        {
            fogMaskCamera.targetTexture = null;

            revealTexture.Release();
            Destroy(revealTexture);
        }

        int height = maskHeight;

        int width = Mathf.RoundToInt(
            height * mainCamera.aspect
        );

        revealTexture =
            new RenderTexture(
                width,
                height,
                24,
                RenderTextureFormat.ARGBHalf
            );

        revealTexture.name =
            "Fog Reveal Texture";

        revealTexture.filterMode =
            FilterMode.Bilinear;

        revealTexture.wrapMode =
            TextureWrapMode.Clamp;

        revealTexture.Create();

        fogMaskCamera.targetTexture =
            revealTexture;

        if (runtimeFogMaterial != null)
        {
            runtimeFogMaterial.SetTexture(
                "_RevealMask",
                revealTexture
            );
        }

        previousScreenWidth =
            Screen.width;

        previousScreenHeight =
            Screen.height;
    }

    private void ConfigureMaskCamera()
    {
        fogMaskCamera.transform.SetParent(
            transform,
            false
        );

        fogMaskCamera.transform.localPosition =
            Vector3.zero;

        fogMaskCamera.transform.localRotation =
            Quaternion.identity;

        fogMaskCamera.orthographic = true;

        fogMaskCamera.clearFlags =
            CameraClearFlags.SolidColor;

        fogMaskCamera.backgroundColor =
            Color.black;

        UniversalAdditionalCameraData cameraData = fogMaskCamera.GetUniversalAdditionalCameraData();

        cameraData.renderType = CameraRenderType.Base;

        int fogRevealMask =
            1 << fogRevealLayer;

        // Render:
        // 1. the hidden FogReveal surface
        // 2. the layers containing the actual Light2Ds
        fogMaskCamera.cullingMask =
            fogRevealMask |
            fogLightSourceLayers.value;

        fogMaskCamera.targetTexture =
            revealTexture;

        fogMaskCamera.allowHDR = true;
        fogMaskCamera.allowMSAA = false;

        fogMaskCamera.depth =
            mainCamera.depth - 10f;

        // Main camera must NOT draw the hidden mask surface.
        mainCamera.cullingMask &=
            ~fogRevealMask;
    }

    private void ConfigureMaskSurface()
    {
        fogMaskSurface.gameObject.layer =
            fogRevealLayer;

        fogMaskSurface.sortingLayerName =
            "FogReveal";

        fogMaskSurface.transform.SetParent(
            transform,
            false
        );

        fogMaskSurface.transform.localPosition =
            new Vector3(
                0f,
                0f,
                maskSurfaceDistance
            );
    }

    private void ConfigureFogOverlay()
    {
        runtimeFogMaterial =
            new Material(fogMaterial);

        runtimeFogMaterial.SetTexture(
            "_RevealMask",
            revealTexture
        );

        runtimeFogMaterial.SetColor(
            "_FogColor",
            fogColor
        );

        runtimeFogMaterial.SetFloat(
            "_RevealMultiplier",
            revealMultiplier
        );

        runtimeFogMaterial.SetFloat(
            "_RevealPower",
            revealPower
        );

        fogOverlay.material =
            runtimeFogMaterial;

        fogOverlay.transform.SetParent(
            transform,
            false
        );

        fogOverlay.transform.localPosition =
            new Vector3(
                0f,
                0f,
                fogOverlayDistance
            );
    }

    private void SyncCameraSettings()
    {
        fogMaskCamera.orthographic =
            mainCamera.orthographic;
        
        fogMaskCamera.orthographicSize =
            mainCamera.orthographicSize;

        fogMaskCamera.aspect =
            mainCamera.aspect;

        fogMaskCamera.rect = new Rect(0f, 0f, 1f, 1f);
    }

    private void ResizeSurfaces()
    {        
        float halfHeight = mainCamera.orthographicSize;

        float fullHeight = halfHeight * 2f;

        float fullWidth = fullHeight * mainCamera.aspect;

        ResizeSprite(fogMaskSurface, fullWidth, fullHeight);

        ResizeSprite(fogOverlay, fullWidth * overscan, fullHeight * overscan);
    }

    private void ResizeSprite(
        SpriteRenderer renderer,
        float width,
        float height)
    {
        if (renderer == null ||
            renderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize =
            renderer.sprite.bounds.size;

        renderer.transform.localScale =
            new Vector3(
                width / spriteSize.x,
                height / spriteSize.y,
                1f
            );
    }

    private void OnDestroy()
    {
        if (revealTexture != null)
        {
            revealTexture.Release();
            Destroy(revealTexture);
        }

        if (runtimeFogMaterial != null)
        {
            Destroy(runtimeFogMaterial);
        }
    }
}