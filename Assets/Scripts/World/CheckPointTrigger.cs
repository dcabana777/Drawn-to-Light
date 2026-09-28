using UnityEngine;

public class CheckPointTrigger : Trigger
{
    [SerializeField] private CheckPoint newCheckPoint;
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        TriggerEffect();
    }

    protected override void TriggerEffect()
    {
        gm.CurrentRoom.ChangeCheckPoint(newCheckPoint);
    }
}
