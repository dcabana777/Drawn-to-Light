using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityAcquisitionPopup : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private string gameplayMapName = "Player";
    [SerializeField] private string systemMapName = "System";
    [SerializeField] private string uiMapName = "UI";
    [SerializeField] private string submitActionName = "Submit";

    [Header("Popup")]
    [SerializeField] private GameObject popupRoot;

    private InputActionMap gameplayMap;
    private InputActionMap systemMap;
    private InputActionMap uiMap;
    private InputAction submitAction;

    private bool gameplayWasEnabled;
    private bool systemWasEnabled;
    private bool uiWasEnabled;

    private float previousTimeScale;

    private bool isOpen;

    private void Awake()
    {
        if (popupRoot == null)
        {
            popupRoot = gameObject;
        }

        if (playerInput == null)
        {
            Debug.LogError("AbilityAcquisitionPopup has no PlayerInput assigned.");

            return;
        }

        gameplayMap = playerInput.actions.FindActionMap(gameplayMapName, true);

        systemMap = playerInput.actions.FindActionMap(systemMapName, true);

        uiMap = playerInput.actions.FindActionMap(uiMapName, true);

        submitAction = uiMap.FindAction(submitActionName, true);

        // Popup should begin hidden.
        popupRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (!isOpen)
            return;

        if (submitAction != null)
        {
            submitAction.performed += OnSubmitPerformed;
        }
    }

    private void OnDisable()
    {
        if (submitAction != null)
        {
            submitAction.performed -= OnSubmitPerformed;
        }
    }

    /// <summary>
    /// Call this from a UnityEvent when an ability is acquired.
    /// </summary>
    public void ShowPopup()
    {
        if (isOpen || playerInput == null)
            return;

        isOpen = true;

        // Remember how input was configured beforehand.
        gameplayWasEnabled = gameplayMap.enabled;

        systemWasEnabled = systemMap.enabled;

        uiWasEnabled = uiMap.enabled;

        previousTimeScale = Time.timeScale;

        // Block all normal gameplay.
        gameplayMap.Disable();

        // This also blocks Pause.
        systemMap.Disable();

        // UI Submit remains available to close the popup.
        uiMap.Enable();

        Time.timeScale = 0f;

        popupRoot.SetActive(true);

        // OnEnable has now run, so subscribe if necessary.
        submitAction.performed -= OnSubmitPerformed;

        submitAction.performed += OnSubmitPerformed;
    }

    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        HidePopup();
    }

    public void HidePopup()
    {
        if (!isOpen)
            return;

        isOpen = false;

        submitAction.performed -= OnSubmitPerformed;

        popupRoot.SetActive(false);

        Time.timeScale = previousTimeScale;

        // Restore the exact map states from before
        // the popup appeared.
        if (gameplayWasEnabled)
            gameplayMap.Enable();
        else
            gameplayMap.Disable();

        if (systemWasEnabled)
            systemMap.Enable();
        else
            systemMap.Disable();

        if (uiWasEnabled)
            uiMap.Enable();
        else
            uiMap.Disable();
    }
}