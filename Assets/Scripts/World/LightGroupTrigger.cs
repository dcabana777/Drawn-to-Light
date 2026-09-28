using UnityEngine;

public class LightGroupTrigger : Trigger
{
    [SerializeField] private int newLightGroup;

    protected override void TriggerEffect()
    {
        if (GM == null)
            return;

        LightController playerController = GM.Player.GetComponent<LightController>();

        if (playerController == null)
        {
            Debug.LogError("LightGroupTrigger could not find Player LightController!");
            return;
        }
        
        playerController.ChangeLightGroup(newLightGroup);
    }
}
