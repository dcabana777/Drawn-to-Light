using UnityEngine;

public class RespawnTrigger : Trigger
{
    protected override void TriggerEffect()
    {
        if (GM == null)
            return;

        GM.RespawnPlayer();
    }
}
