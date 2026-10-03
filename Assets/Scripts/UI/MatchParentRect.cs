using UnityEngine;

[ExecuteAlways]
public class MatchParentRect : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform parentRect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateToParent();
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateToParent();
    }

    private void UpdateToParent()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        if (rectTransform.parent != null)
        {
            parentRect = rectTransform.parent.GetComponent<RectTransform>();
        }

        if (parentRect == null) return;

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentRect.rect.width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentRect.rect.height);
    }
}
