using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSchemePrompt : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Prompt Variants")]
    [SerializeField] private GameObject keyboardMousePrompt;
    [SerializeField] private GameObject controllerPrompt;

    [Header("Control Scheme Names")]
    [Tooltip("Must exactly match the control scheme name in the Input Actions asset.")]
    [SerializeField] private string keyboardMouseScheme = "MouseAndKeyboard";

    [Tooltip("Must exactly match the control scheme name in the Input Actions asset.")]
    [SerializeField] private string controllerScheme = "Controller";

    private void OnEnable()
    {
        if (playerInput == null)
        {
            Debug.LogError("ControlSchemePrompt has no PlayerInput assigned.");

            return;
        }

        playerInput.onControlsChanged += OnControlsChanged;

        RefreshPrompt();
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnControlsChanged;
        }
    }

    private void OnControlsChanged(PlayerInput input)
    {
        RefreshPrompt();
    }

    private void RefreshPrompt()
    {
        string scheme = playerInput.currentControlScheme;

        bool usingController = (scheme == controllerScheme);

        bool usingKeyboardMouse = (scheme == keyboardMouseScheme);

        if (controllerPrompt != null)
        {
            controllerPrompt.SetActive(usingController);
        }

        if (keyboardMousePrompt != null)
        {
            keyboardMousePrompt.SetActive(usingKeyboardMouse);
        }
    }
}