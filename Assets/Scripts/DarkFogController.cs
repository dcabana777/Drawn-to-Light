using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DarkFogController : MonoBehaviour
{
    [Header("Fog Overlay")]
    [SerializeField] private SpriteRenderer fogOverlay;
    [SerializeField] private Material fogMaterial;

    [SerializeField]
    private Color fogColor =
        new Color(0f, 0f, 0f, 0.95f);

    [Header("Reveal Mask")]
    [SerializeField] private LayerMask fogRevealLayer;

    [SerializeField, Min(64)]
    private int maskWidth = 640;

    [SerializeField, Min(64)]
    private int maskHeight = 360;

    [Header("Overlay Position")]
    [SerializeField]
    private float overlayDistanceFromCamera = 1f;

    private Camera mainCamera;
    private Camera maskCamera;

    private RenderTexture revealTexture;
    private Material runtimeFogMaterial;

    private FixedAspectCamera fixedAspectCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();

        fixedAspectCamera =
            GetComponent<FixedAspectCamera>();

        CreateRevealTexture();
        CreateMaskCamera();
        SetupFogOverlay();
    }

    private void LateUpdate()
    {
        SyncMaskCamera();
        ResizeFogOverlay();
    }

    private void CreateRevealTexture()
    {
        revealTexture =
            new RenderTexture(
                maskWidth,
                maskHeight,
                0,
                RenderTextureFormat.ARGB32
            );

        revealTexture.name =
            "Fog Reveal Mask";

        revealTexture.filterMode =
            FilterMode.Bilinear;

        revealTexture.wrapMode =
            TextureWrapMode.Clamp;

        revealTexture.Create();
    }

    private void CreateMaskCamera()
    {
        GameObject maskCameraObject =
            new GameObject("Fog Mask Camera");

        maskCameraObject.transform.SetParent(
            transform,
            false
        );

        maskCamera =
            maskCameraObject.AddComponent<Camera>();

        maskCamera.orthographic = true;

        maskCamera.clearFlags =
            CameraClearFlags.SolidColor;

        maskCamera.backgroundColor =
            Color.black;

        maskCamera.cullingMask =
            fogRevealLayer;

        maskCamera.targetTexture =
            revealTexture;

        maskCamera.allowHDR = false;
        maskCamera.allowMSAA = false;

        // Render the mask before the gameplay camera.
        maskCamera.depth =
            mainCamera.depth - 10f;

        // The gameplay camera should never render
        // FogReveal objects themselves.
        mainCamera.cullingMask &=
            ~fogRevealLayer.value;
    }

    private void SetupFogOverlay()
    {
        if (fogOverlay == null ||
            fogMaterial == null)
        {
            Debug.LogError(
                "DarkFogController is missing its " +
                "Fog Overlay or Fog Material."
            );

            return;
        }

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
                overlayDistanceFromCamera
            );
    }

    private void SyncMaskCamera()
    {
        if (maskCamera == null)
            return;

        maskCamera.orthographicSize =
            mainCamera.orthographicSize;

        maskCamera.aspect =
            GetGameplayAspect();
    }

    private void ResizeFogOverlay()
    {
        if (fogOverlay == null ||
            fogOverlay.sprite == null)
        {
            return;
        }

        float height =
            mainCamera.orthographicSize * 2f;

        float width =
            height * GetGameplayAspect();

        Vector2 spriteSize =
            fogOverlay.sprite.bounds.size;

        fogOverlay.transform.localScale =
            new Vector3(
                width / spriteSize.x,
                height / spriteSize.y,
                1f
            );
    }

    private float GetGameplayAspect()
    {
        if (fixedAspectCamera != null)
        {
            return fixedAspectCamera.TargetAspect;
        }

        return (float)maskWidth / maskHeight;
    }

    private void OnDestroy()
    {
        if (revealTexture != null)
        {
            revealTexture.Release();
        }

        if (runtimeFogMaterial != null)
        {
            Destroy(runtimeFogMaterial);
        }
    }
}