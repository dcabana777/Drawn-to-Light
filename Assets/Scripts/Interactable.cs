using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Limited Use Settings")]
    [SerializeField] private bool destroySelfAfterLimitReached;
    [SerializeField, Min(1)] private int maxUses;
    private int currentUseCount;

    protected virtual void Awake()
    {
        currentUseCount = 0;
    }
    
    public void Interact()
    {
        if (currentUseCount < maxUses)
        {
            currentUseCount++;
            OnInteract();
        }
        if (currentUseCount == maxUses && destroySelfAfterLimitReached)
        {
            SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.enabled = false;

            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.simulated = false;

            Collider2D col = gameObject.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
    }

    protected abstract void OnInteract();
}
