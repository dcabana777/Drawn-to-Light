using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    private GameManager gm;
    private bool isTriggering;

    [SerializeField] private UnityEvent onTrigger;
    
    protected GameManager GM
    {
        get
        {
            if (gm == null)
            {
                gm = FindAnyObjectByType<GameManager>();
                
                if (gm == null)
                {
                    Debug.LogError("Trigger " + gameObject.name + " could not find GameManager!");   
                }
            }

            return gm;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        ManualTrigger();
    }

    public bool ManualTrigger()
    {
        if (!isActiveAndEnabled || isTriggering)
            return false;

        isTriggering = true;
        
        try 
        { 
            TriggerEffect();
            onTrigger?.Invoke();
            return true;
        }
        finally
        {
            isTriggering = false;
        }
    }

    protected virtual void TriggerEffect()
    {
        // does not do anything by default
    }


}
