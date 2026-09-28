using UnityEngine;

public class RespawnTrigger : Trigger
{

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerEffect();
        }
    }

    protected override void TriggerEffect()
    {
        gm.RespawnPlayer();
    }
}
