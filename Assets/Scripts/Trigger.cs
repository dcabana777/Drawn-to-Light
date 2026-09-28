using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Trigger : MonoBehaviour
{
    protected GameManager gm;
    
    protected virtual void Awake()
    {
        gm = FindAnyObjectByType<GameManager>();

        if (gm == null)
        {
            Debug.LogError("Trigger " + gameObject.name + " could not find GameManager!");
        }
    }

    protected abstract void OnTriggerEnter2D(Collider2D other);

    public void ManualTrigger()
    {
        TriggerEffect();
    }

    protected abstract void TriggerEffect();


}
