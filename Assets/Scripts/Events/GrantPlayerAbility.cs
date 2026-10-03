using UnityEngine;

public class GrantPlayerAbility : Interactable
{
    public enum PlayerAbility
    {
        LightControl,
        Lantern
    }

    [Header("Ability")]
    [SerializeField] private PlayerAbility abilityToGrant;
    [Tooltip("Optional. If assigned, will also set abilityObject to active.")]
    [SerializeField] private GameObject abilityObject;

    private GameObject player;

    protected override void Awake()
    {
        base.Awake();
        
        GameManager gm = FindAnyObjectByType<GameManager>();

        if (gm != null)
        {
            player = gm.Player;
        }

        if (player == null)
        {
            Debug.LogError("GrantPlayerAbility could not find the Player.");
        }
    }

    protected override bool OnInteract()
    {
        if (player == null)
            return false;

        switch (abilityToGrant)
        {
            case PlayerAbility.LightControl:
                GrantLightControl();
                break;

            case PlayerAbility.Lantern:
                GrantLantern();
                break;
        }

        if (abilityObject != null)
            abilityObject.SetActive(true);

        OnAbilityGranted();

        return true;
    }

    private void GrantLantern()
    {
        PlayerLantern lantern = player.GetComponent<PlayerLantern>();

        if (lantern == null)
        {
            Debug.LogError("Player does not have a PlayerLantern component.");

            return;
        }

        lantern.enabled = true;
    }

    public void RemoveLantern()
    {
        PlayerLantern lantern = player.GetComponent<PlayerLantern>();

        if (lantern == null)
        {
            Debug.LogError("Player does not have a PlayerLantern component.");

            return;
        }

        lantern.enabled = false;

        if (abilityObject != null)
            abilityObject.SetActive(false);
    }

    private void GrantLightControl()
    {
        LightController lightController = player.GetComponent<LightController>();

        if (lightController == null)
        {
            Debug.LogError("Player does not have a LightController component.");

            return;
        }

        lightController.enabled = true;
    }

    public void RemoveLightControl()
    {
        LightController lightController = player.GetComponent<LightController>();

        if (lightController == null)
        {
            Debug.LogError("Player does not have a LightController component.");

            return;
        }

        lightController.enabled = false;

        if (abilityObject != null)
            abilityObject.SetActive(false);
    }

    private void OnAbilityGranted()
    {
        Debug.Log("Granted player ability: " + abilityToGrant);

        // Later:
        // - pause or suspend player input
        // - show ability acquisition popup
        // - show controls/tutorial text
        // - play sound/animation
        // - resume gameplay
    }
}
