using UnityEngine;

public class CheckPointTrigger : Trigger
{
    [SerializeField] private CheckPoint newCheckPoint;

    protected override void TriggerEffect()
    {
        if (GM == null || newCheckPoint == null)
            return;
        
        GM.CurrentRoom.ChangeCheckPoint(newCheckPoint);
    }
}
