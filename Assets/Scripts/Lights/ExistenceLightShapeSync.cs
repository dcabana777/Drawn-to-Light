using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
[RequireComponent(typeof(PolygonCollider2D))]
public class ExistenceLightShapeSync : MonoBehaviour
{
    [ContextMenu("Sync Collider From Light")]
    private void SyncColliderFromLight()
    {
        Light2D light2D = GetComponent<Light2D>();
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();

        Vector3[] lightPath = light2D.shapePath;
        Vector2[] colliderPath = new Vector2[lightPath.Length];

        for (int i = 0; i < lightPath.Length; i++)
        {
            colliderPath[i] = new Vector2(
                lightPath[i].x,
                lightPath[i].y
            );
        }

        polygonCollider.pathCount = 1;
        polygonCollider.SetPath(0, colliderPath);
    }
}