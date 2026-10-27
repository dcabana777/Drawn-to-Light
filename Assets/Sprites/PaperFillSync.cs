using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class PaperFillSync : MonoBehaviour
{
    [Header("Sync Settings")]
    [SerializeField] private bool autoSync = true;

    [Header("Optional")]
    [SerializeField] private bool forceTiledMode = true;

    private SpriteRenderer spriteRenderer;

    private Transform cachedParent;
    private Vector3 lastParentScale;

    private void OnEnable()
    {
        CacheReferences();
        SyncToParent();
    }

    private void OnValidate()
    {
        CacheReferences();

        if (autoSync)
            SyncToParent();
    }

    private void Update()
    {
        if (!autoSync)
            return;

        if (transform.parent == null)
            return;

        // Parent changed.
        if (cachedParent != transform.parent)
        {
            CacheReferences();
            SyncToParent();
            return;
        }

        // Parent scale changed.
        if (transform.parent.localScale != lastParentScale)
        {
            SyncToParent();
        }
    }

    private void CacheReferences()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        cachedParent = transform.parent;

        if (cachedParent != null)
            lastParentScale = cachedParent.localScale;
    }

    public void SyncToParent()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        Transform parent = transform.parent;

        if (parent == null)
        {
            Debug.LogWarning(
                "PaperFillSync requires this object to have a parent.",
                this
            );

            return;
        }

        Vector3 parentScale = parent.localScale;

        // Prevent division by zero.
        float parentScaleX =
            Mathf.Approximately(parentScale.x, 0f)
                ? 0.0001f
                : parentScale.x;

        float parentScaleY =
            Mathf.Approximately(parentScale.y, 0f)
                ? 0.0001f
                : parentScale.y;

        // -------------------------
        // X
        // -------------------------

        float childScaleX =
            1f / parentScaleX;

        float spriteSizeX =
            Mathf.Abs(parentScale.x);

        // -------------------------
        // Y
        // -------------------------

        float childScaleY =
                1f / parentScaleY;

        float spriteSizeY =
                Mathf.Abs(parentScale.y);

        // Preserve the child's Z scale.
        transform.localScale =
            new Vector3(
                childScaleX,
                childScaleY,
                transform.localScale.z
            );

        spriteRenderer.size =
            new Vector2(
                spriteSizeX,
                spriteSizeY
            );

        if (forceTiledMode)
        {
            spriteRenderer.drawMode =
                SpriteDrawMode.Tiled;

            spriteRenderer.tileMode =
                SpriteTileMode.Continuous;
        }

        cachedParent = parent;
        lastParentScale = parent.localScale;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(PaperFillSync))]
public class PaperFillSyncEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        PaperFillSync sync =
            (PaperFillSync)target;

        if (GUILayout.Button("Sync To Parent"))
        {
            Undo.RecordObject(
                sync.transform,
                "Sync Paper Fill"
            );

            SpriteRenderer renderer =
                sync.GetComponent<SpriteRenderer>();

            if (renderer != null)
            {
                Undo.RecordObject(
                    renderer,
                    "Sync Paper Fill"
                );
            }

            sync.SyncToParent();

            EditorUtility.SetDirty(sync);
        }
    }
}

#endif