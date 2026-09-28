using UnityEngine;

public class LightGroupTrigger : Trigger
{
    [SerializeField] private int newLightGroup;

    private LightController playerController;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerController = other.GetComponent<LightController>();
        if (playerController == null)
        {
            Debug.LogError("LightGroupTrigger could not find Player LightController!");
            return;
        }

        TriggerEffect();
    }

    protected override void TriggerEffect()
    {
        playerController.ChangeLightGroup(newLightGroup);
    }
}
