using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class WorldCompositeRig : MonoBehaviour
{
    [Header("Source Cameras")]
    [SerializeField] private Camera darkCamera;
    [SerializeField] private Camera lightCamera;
    [SerializeField] private Camera existenceMaskCamera;
    [SerializeField] private Camera fogMaskCamera;

    [Header("Mask Surfaces")]
    [Tooltip("White Sprite-Lit surface targeted only by existence Light2Ds.")]
    [SerializeField] private SpriteRenderer existenceMaskSurface;

    [Tooltip("White Sprite-Lit surface targeted by every Light2D that should clear fog.")]
    [SerializeField] private SpriteRenderer fogMaskSurface;

    [SerializeField] private float maskSurfaceDistance = 10f;

    [Header("Camera Layer Masks")]
    [Tooltip("Permanent / DarkWorld-equivalent render layers.")]
    [SerializeField] private LayerMask darkRenderLayers;

    [Tooltip("Must include both permanent and light-dependent render layers.")]
    [SerializeField] private LayerMask lightRenderLayers;

    [Tooltip("Must include the existence mask surface layer and the GameObject layers containing relevant Light2Ds.")]
    [SerializeField] private LayerMask existenceMaskRenderLayers;

    [Tooltip("Must include the fog mask surface layer and the GameObject layers containing every Light2D that should clear fog.")]
    [SerializeField] private LayerMask fogMaskRenderLayers;

    [Header("Composite Output")]
    [SerializeField] private RawImage compositeImage;
    [SerializeField] private Material compositeMaterialAsset;

    [Header("Render Texture")]
    [SerializeField, Range(0.25f, 1f)] private float renderScale = 1f;

    [Header("Existence Mask Remapping")]
    [SerializeField, Range(0f, 1f)] private float maskBlackPoint = 0f;
    [SerializeField, Range(0f, 1f)] private float maskWhitePoint = 1f;
    [SerializeField, Range(0.1f, 4f)] private float maskGamma = 1f;

    [Header("World Transition")]
    [SerializeField, Range(0f, 1f)] private float darkThreshold = 0.4f;
    [SerializeField, Range(0f, 1f)] private float lightThreshold = 0.6f;

    [Header("Dark Fog")]
    [SerializeField] private Color fogColor =
        new Color(0f, 0f, 0f, 0.97f);

    [SerializeField, Min(0f)] private float fogRevealMultiplier = 1f;
    [SerializeField, Min(0.01f)] private float fogRevealPower = 1f;

    [Header("Texture Orientation")]
    [SerializeField] private bool flipY = false;

    private Camera displayCamera;

    private RenderTexture darkRT;
    private RenderTexture lightRT;
    private RenderTexture existenceMaskRT;
    private RenderTexture fogMaskRT;

    private Material compositeMaterialInstance;

    private int currentWidth;
    private int currentHeight;

    private static readonly int DarkTexID =
        Shader.PropertyToID("_DarkTex");

    private static readonly int LightTexID =
        Shader.PropertyToID("_LightTex");

    private static readonly int ExistenceMaskTexID =
        Shader.PropertyToID("_ExistenceMaskTex");

    private static readonly int FogMaskTexID =
        Shader.PropertyToID("_FogMaskTex");

    private static readonly int MaskBlackPointID =
        Shader.PropertyToID("_MaskBlackPoint");

    private static readonly int MaskWhitePointID =
        Shader.PropertyToID("_MaskWhitePoint");

    private static readonly int MaskGammaID =
        Shader.PropertyToID("_MaskGamma");

    private static readonly int DarkThresholdID =
        Shader.PropertyToID("_DarkThreshold");

    private static readonly int LightThresholdID =
        Shader.PropertyToID("_LightThreshold");

    private static readonly int FogColorID =
        Shader.PropertyToID("_FogColor");

    private static readonly int FogRevealMultiplierID =
        Shader.PropertyToID("_FogRevealMultiplier");

    private static readonly int FogRevealPowerID =
        Shader.PropertyToID("_FogRevealPower");

    private static readonly int FlipYID =
        Shader.PropertyToID("_FlipY");

    private void Awake()
    {
        displayCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        compositeMaterialInstance =
            new Material(compositeMaterialAsset);

        compositeImage.material =
            compositeMaterialInstance;

        compositeImage.raycastTarget = false;

        ConfigureCameras();
        ConfigureMaskSurfaces();
        RebuildRenderTextures();
        SyncSourceCameras();
        ResizeMaskSurfaces();
        UpdateCompositeMaterial();
    }

    private void LateUpdate()
    {
        GetDesiredRenderSize(
            out int desiredWidth,
            out int desiredHeight
        );

        if (desiredWidth != currentWidth ||
            desiredHeight != currentHeight)
        {
            RebuildRenderTextures();
        }

        SyncSourceCameras();
        ResizeMaskSurfaces();
        UpdateCompositeMaterial();
    }

    private bool ValidateReferences()
    {
        if (displayCamera == null ||
            darkCamera == null ||
            lightCamera == null ||
            existenceMaskCamera == null ||
            fogMaskCamera == null ||
            existenceMaskSurface == null ||
            fogMaskSurface == null ||
            compositeImage == null ||
            compositeMaterialAsset == null)
        {
            Debug.LogError(
                "WorldCompositeRig is missing one or more required references.",
                this
            );

            return false;
        }

        return true;
    }

    private void ConfigureCameras()
    {
        darkCamera.cullingMask =
            darkRenderLayers;

        lightCamera.cullingMask =
            lightRenderLayers;

        existenceMaskCamera.cullingMask =
            existenceMaskRenderLayers;

        fogMaskCamera.cullingMask =
            fogMaskRenderLayers;

        ConfigureSourceCamera(darkCamera);
        ConfigureSourceCamera(lightCamera);
        ConfigureMaskCamera(existenceMaskCamera);
        ConfigureMaskCamera(fogMaskCamera);
    }

    private void ConfigureSourceCamera(Camera cameraToConfigure)
    {
        UniversalAdditionalCameraData cameraData =
            cameraToConfigure.GetUniversalAdditionalCameraData();

        cameraData.renderType =
            CameraRenderType.Base;

        cameraToConfigure.rect =
            new Rect(0f, 0f, 1f, 1f);
    }

    private void ConfigureMaskCamera(Camera cameraToConfigure)
    {
        ConfigureSourceCamera(cameraToConfigure);

        cameraToConfigure.clearFlags =
            CameraClearFlags.SolidColor;

        cameraToConfigure.backgroundColor =
            Color.black;

        cameraToConfigure.allowHDR = true;
        cameraToConfigure.allowMSAA = false;
    }

    private void ConfigureMaskSurfaces()
    {
        ConfigureMaskSurface(
            existenceMaskSurface
        );

        ConfigureMaskSurface(
            fogMaskSurface
        );
    }

    private void ConfigureMaskSurface(
        SpriteRenderer surface)
    {
        surface.transform.SetParent(
            displayCamera.transform,
            false
        );

        surface.transform.localPosition =
            new Vector3(
                0f,
                0f,
                maskSurfaceDistance
            );

        surface.transform.localRotation =
            Quaternion.identity;
    }

    private void SyncSourceCameras()
    {
        SyncCameraView(darkCamera);
        SyncCameraView(lightCamera);
        SyncCameraView(existenceMaskCamera);
        SyncCameraView(fogMaskCamera);

        // Mask cameras must always clear to black regardless
        // of the display camera's clear settings.
        existenceMaskCamera.clearFlags =
            CameraClearFlags.SolidColor;

        existenceMaskCamera.backgroundColor =
            Color.black;

        existenceMaskCamera.allowHDR = true;
        existenceMaskCamera.allowMSAA = false;

        fogMaskCamera.clearFlags =
            CameraClearFlags.SolidColor;

        fogMaskCamera.backgroundColor =
            Color.black;

        fogMaskCamera.allowHDR = true;
        fogMaskCamera.allowMSAA = false;
    }

    private void SyncCameraView(
        Camera target)
    {
        target.transform.SetPositionAndRotation(
            displayCamera.transform.position,
            displayCamera.transform.rotation
        );

        target.orthographic =
            displayCamera.orthographic;

        target.orthographicSize =
            displayCamera.orthographicSize;

        target.fieldOfView =
            displayCamera.fieldOfView;

        target.nearClipPlane =
            displayCamera.nearClipPlane;

        target.farClipPlane =
            displayCamera.farClipPlane;

        target.lensShift =
            displayCamera.lensShift;

        target.allowHDR =
            displayCamera.allowHDR;

        target.allowMSAA =
            displayCamera.allowMSAA;

        target.clearFlags =
            displayCamera.clearFlags;

        target.backgroundColor =
            displayCamera.backgroundColor;

        target.transparencySortMode =
            displayCamera.transparencySortMode;

        target.transparencySortAxis =
            displayCamera.transparencySortAxis;

        target.rect =
            new Rect(0f, 0f, 1f, 1f);

        if (currentHeight > 0)
        {
            target.aspect =
                (float)currentWidth / currentHeight;
        }
    }

    private void GetDesiredRenderSize(
        out int width,
        out int height)
    {
        // Respect the display camera's viewport. This means a
        // FixedAspectCamera can pillarbox/letterbox the display
        // while the source RTs stay at the actual gameplay aspect.
        float viewportWidth =
            Mathf.Max(
                1f,
                Screen.width * displayCamera.rect.width
            );

        float viewportHeight =
            Mathf.Max(
                1f,
                Screen.height * displayCamera.rect.height
            );

        width = Mathf.Max(
            1,
            Mathf.RoundToInt(
                viewportWidth * renderScale
            )
        );

        height = Mathf.Max(
            1,
            Mathf.RoundToInt(
                viewportHeight * renderScale
            )
        );
    }

    private void RebuildRenderTextures()
    {
        GetDesiredRenderSize(
            out currentWidth,
            out currentHeight
        );

        ReleaseRenderTextures();

        darkRT = CreateRenderTexture(
            "DarkWorldRT",
            currentWidth,
            currentHeight
        );

        lightRT = CreateRenderTexture(
            "LightWorldRT",
            currentWidth,
            currentHeight
        );

        existenceMaskRT = CreateRenderTexture(
            "ExistenceMaskRT",
            currentWidth,
            currentHeight
        );

        fogMaskRT = CreateRenderTexture(
            "FogRevealMaskRT",
            currentWidth,
            currentHeight
        );

        darkCamera.targetTexture = darkRT;
        lightCamera.targetTexture = lightRT;
        existenceMaskCamera.targetTexture =
            existenceMaskRT;
        fogMaskCamera.targetTexture = fogMaskRT;

        // RawImage supplies _MainTex automatically. The actual
        // image comes entirely from the custom composite material.
        compositeImage.texture =
            Texture2D.whiteTexture;

        UpdateCompositeMaterial();
    }

    private RenderTexture CreateRenderTexture(
        string textureName,
        int width,
        int height)
    {
        // Unity 6 RenderGraph expects an output RT to have a
        // depth/stencil buffer, so keep a 24-bit depth buffer.
        RenderTexture rt =
            new RenderTexture(
                width,
                height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default
            );

        rt.name = textureName;
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.useMipMap = false;
        rt.autoGenerateMips = false;
        rt.antiAliasing = 1;

        rt.Create();

        return rt;
    }

    private void ResizeMaskSurfaces()
    {
        if (!displayCamera.orthographic ||
            currentHeight <= 0)
        {
            return;
        }

        float fullHeight =
            displayCamera.orthographicSize * 2f;

        float aspect =
            (float)currentWidth / currentHeight;

        float fullWidth =
            fullHeight * aspect;

        ResizeSprite(
            existenceMaskSurface,
            fullWidth,
            fullHeight
        );

        ResizeSprite(
            fogMaskSurface,
            fullWidth,
            fullHeight
        );
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

    private void UpdateCompositeMaterial()
    {
        if (compositeMaterialInstance == null)
            return;

        compositeMaterialInstance.SetTexture(
            DarkTexID,
            darkRT
        );

        compositeMaterialInstance.SetTexture(
            LightTexID,
            lightRT
        );

        compositeMaterialInstance.SetTexture(
            ExistenceMaskTexID,
            existenceMaskRT
        );

        compositeMaterialInstance.SetTexture(
            FogMaskTexID,
            fogMaskRT
        );

        compositeMaterialInstance.SetFloat(
            MaskBlackPointID,
            maskBlackPoint
        );

        compositeMaterialInstance.SetFloat(
            MaskWhitePointID,
            maskWhitePoint
        );

        compositeMaterialInstance.SetFloat(
            MaskGammaID,
            maskGamma
        );

        compositeMaterialInstance.SetFloat(
            DarkThresholdID,
            darkThreshold
        );

        compositeMaterialInstance.SetFloat(
            LightThresholdID,
            lightThreshold
        );

        compositeMaterialInstance.SetColor(
            FogColorID,
            fogColor
        );

        compositeMaterialInstance.SetFloat(
            FogRevealMultiplierID,
            fogRevealMultiplier
        );

        compositeMaterialInstance.SetFloat(
            FogRevealPowerID,
            fogRevealPower
        );

        compositeMaterialInstance.SetFloat(
            FlipYID,
            flipY ? 1f : 0f
        );
    }

    private void ReleaseRenderTextures()
    {
        ClearTargetIfMatching(
            darkCamera,
            darkRT
        );

        ClearTargetIfMatching(
            lightCamera,
            lightRT
        );

        ClearTargetIfMatching(
            existenceMaskCamera,
            existenceMaskRT
        );

        ClearTargetIfMatching(
            fogMaskCamera,
            fogMaskRT
        );

        ReleaseRenderTexture(ref darkRT);
        ReleaseRenderTexture(ref lightRT);
        ReleaseRenderTexture(ref existenceMaskRT);
        ReleaseRenderTexture(ref fogMaskRT);
    }

    private void ClearTargetIfMatching(
        Camera cameraToClear,
        RenderTexture texture)
    {
        if (cameraToClear != null &&
            cameraToClear.targetTexture == texture)
        {
            cameraToClear.targetTexture = null;
        }
    }

    private void ReleaseRenderTexture(
        ref RenderTexture renderTexture)
    {
        if (renderTexture == null)
            return;

        renderTexture.Release();
        Destroy(renderTexture);
        renderTexture = null;
    }

    private void OnValidate()
    {
        if (lightThreshold < darkThreshold)
        {
            lightThreshold = darkThreshold;
        }

        if (maskWhitePoint < maskBlackPoint)
        {
            maskWhitePoint = maskBlackPoint;
        }
    }

    private void OnDestroy()
    {
        ReleaseRenderTextures();

        if (compositeMaterialInstance != null)
        {
            Destroy(compositeMaterialInstance);
        }
    }
}
