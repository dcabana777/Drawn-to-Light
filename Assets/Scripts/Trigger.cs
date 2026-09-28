using UnityEngine;

public abstract class Trigger : MonoBehaviour
{
    private GameManager gm;
    private bool isTriggering;
    
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
            return true;
        }
        finally
        {
            isTriggering = false;
        }
    }

    protected abstract void TriggerEffect();


}
