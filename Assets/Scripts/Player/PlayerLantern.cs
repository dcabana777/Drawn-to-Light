using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Rendering.Universal;

public class PlayerLantern : MonoBehaviour
{
    private PlayerState playerData;
    
    [SerializeField] private InputActionReference interactAction;

    [Header("Lantern Defaults")]
    [SerializeField] private bool isLanternOnDefault;
    [SerializeField] private bool isLanternGlobalDefault;

    [Header("Lantern Output")]
    [SerializeField] private Light2D globalLightVisual;
    [SerializeField] private Collider2D globalLightCollider;
    [SerializeField] private Light2D localLightVisual;
    [SerializeField] private Collider2D localLightCollider;

    [Header("Lantern Cooldowns")]
    [SerializeField, Min(0f)] private float modeSwitchCooldown = 0.5f;
    [SerializeField, Min(0f)] private float localToggleCooldown = 0f;
    [SerializeField, Min(0f)] private float globalToggleCooldown = 1f;

    [Header("Global Mode")]
    [Tooltip("Maximum continuous duration of Global mode. 0 or less means unlimited.")]
    [SerializeField] private float globalModeDuration = 5f;

    private bool isOn;
    private bool isGlobal;

    private float nextModeSwitchTime;
    private float nextLocalToggleTime;
    private float nextGlobalToggleTime;

    private Coroutine globalDurationCoroutine;
    
    private void Start()
    {
        isOn = isLanternOnDefault;
        isGlobal = isLanternGlobalDefault;

        GameManager gm = FindAnyObjectByType<GameManager>();
        GameObject player = gm.Player;
        if (player == null)
        {
            Debug.LogError("Lantern could not find Player!");
        }
        playerData = player.GetComponent<PlayerState>();


        UpdateLighting();

        if (isOn && isGlobal)
        {
            StartGlobalDuration();
        }
    }

    private void OnEnable()
    {
        interactAction.action.performed += OnInteractPerformed;
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteractPerformed;
        interactAction.action.Disable();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (context.interaction is HoldInteraction)
        {
            // if lantern is on, toggle between local and global lighting
            if (isOn) {
                TrySwitchMode();
            }
        }
        else if (context.interaction is TapInteraction)
        {
            // toggle lantern on or off
            TryToggleLantern();
        }
    }

    private void TryToggleLantern()
    {
        // can only toggle when on solid ground
        if (!playerData.IsGrounded) return;
        
        // Lantern remembers whether it is in local or global mode even when off,
        // so that mode determines the cooldown
        if (isGlobal)
        {
            if (!CooldownReady(nextGlobalToggleTime)) return;

            if (globalToggleCooldown > 0f)
            {
                nextGlobalToggleTime = Time.time + globalToggleCooldown;
            }
        }
        else
        {
            if (!CooldownReady(nextLocalToggleTime)) return;

            if (nextLocalToggleTime > 0f)
            {
                nextLocalToggleTime = Time.time + localToggleCooldown;
            }
        }

        isOn = !isOn;

        if (!isOn)
        {
            isGlobal = false;
            StopGlobalDuration();
        }

        UpdateLighting();
    }

    private void TrySwitchMode()
    {
        // can only toggle when on solid ground
        if (!playerData.IsGrounded) return;
        
        // Holding only changes mode while the lantern is on
        if (!isOn) return;
        if (!CooldownReady(nextModeSwitchTime)) return;

        if (modeSwitchCooldown > 0f)
        {
            nextModeSwitchTime = Time.time + modeSwitchCooldown;
        }

        isGlobal = !isGlobal;

        if (isGlobal) StartGlobalDuration();
        else StopGlobalDuration();

        UpdateLighting();
    }

    private bool CooldownReady(float nextAllowedTime)
    {
        return Time.time >= nextAllowedTime;
    }

    private void StartGlobalDuration()
    {
        StopGlobalDuration();

        // 0 or less means Global mode has no time limit
        if (globalModeDuration <= 0f) return;

        globalDurationCoroutine = StartCoroutine(GlobalDurationRoutine());
    }

    private void StopGlobalDuration()
    {
        if (globalDurationCoroutine == null) return;

        StopCoroutine(globalDurationCoroutine);
        globalDurationCoroutine = null;
    }

    private IEnumerator GlobalDurationRoutine()
    {
        yield return new WaitForSeconds(globalModeDuration);

        globalDurationCoroutine = null;

        // make sure state is still valid when timer finishes
        if (!isOn || !isGlobal) yield break;

        isGlobal = false;

        UpdateLighting();
    }

    private void UpdateLighting()
    {
        if (isOn)
        {
            if (!isGlobal) // local lighting mode
            {
                if (localLightVisual != null) localLightVisual.enabled = true;
                if (localLightCollider != null) localLightCollider.enabled = true;
                if (globalLightVisual != null) globalLightVisual.enabled = false;
                if (globalLightCollider != null) globalLightCollider.enabled = false;
            }
            else // global lighting mode
            {
                if (globalLightVisual != null) globalLightVisual.enabled = true;
                if (globalLightCollider != null) globalLightCollider.enabled = true;
                if (localLightVisual != null) localLightVisual.enabled = false;
                if (localLightCollider != null) localLightCollider.enabled = false;
            }
        }
        else // lantern is off, can disable all lighting components
        {
            if (localLightVisual != null) localLightVisual.enabled = false;
            if (localLightCollider != null) localLightCollider.enabled = false;
            if (globalLightVisual != null) globalLightVisual.enabled = false;
            if (globalLightCollider != null) globalLightCollider.enabled = false;
        }
    }
}
