using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;
    [SerializeField] private GameObject checkpointConfirmationPanel;
    [SerializeField] private GameObject exitConfirmationPanel;
    [SerializeField] private GameObject changeDisplayConfirmationPanel;

    [Header("Selection Controllers")]
    [SerializeField] private MenuSelectionController pauseMenuSelection;
    [SerializeField] private MenuSelectionController optionsMenuSelection;
    [SerializeField] private MenuSelectionController restoreConfirmationSelection;
    [SerializeField] private MenuSelectionController exitConfirmationSelection;
    [SerializeField] private MenuSelectionController changeDisplayConfirmationSelection;

    //[Header("Return Selection")]
    //[SerializeField] private Button restoreCheckPointButton;
    //[SerializeField] private Button exitGameButton;

    [Header("Room/Checkpoint Info")]
    [SerializeField] private TMP_Text currentRoomText;
    [SerializeField] private TMP_Text majorCheckPointText;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string gameplayMapName = "Player";
    [SerializeField] private string systemMapName = "System";
    [SerializeField] private string pauseActionName = "Pause";
    private bool isPaused = false;
    private bool isInOptions = false;
    private bool isInRestoreConfirmation = false;
    private bool isInExitConfirmation = false;
    private bool isInChangeDisplayConfirmation = false;
    private int currentDisplayOption;
    private int selectedDisplayOption;

    private InputActionMap gameplayMap;
    private InputActionMap systemMap;
    private InputAction pauseAction;

    private GameManager gm;
    private GameObject player;

    private void Awake()
    {
        gameplayMap = playerInput.actions.FindActionMap(gameplayMapName, true);

        systemMap = playerInput.actions.FindActionMap(systemMapName, true);

        pauseAction = systemMap.FindAction(pauseActionName, true);

        gm = FindAnyObjectByType<GameManager>();

        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            currentDisplayOption = 0;
        else
            currentDisplayOption = 1;

        selectedDisplayOption = currentDisplayOption;
        
        systemMap.Enable();
    }

    private void OnEnable()
    {
        pauseAction.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPausePerformed;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            if (isInOptions)
            {
                CloseOptions();
            }
            else if (isInRestoreConfirmation)
            {
                StopRestoreCheckpoint();
            }
            else if (isInExitConfirmation)
            {
                StopExitGame();
            }
            else
            {
                ResumeGame();
            }
        }
        else
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        gameplayMap.Disable();

        Time.timeScale = 0f; // freeze game time
        
        pauseMenuPanel.SetActive(true);
        pauseMenuSelection.SelectDefault();

        currentRoomText.text = "Current Room: " + gm.CurrentRoom.RoomName;
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        pauseMenuPanel.SetActive(false);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        Time.timeScale = 1f; // resume normal game time

        gameplayMap.Enable();

        isPaused = false;
    }

    public void RestoreCheckpoint()
    {
        if (isInRestoreConfirmation)
            return;

        checkpointConfirmationPanel.SetActive(true);

        restoreConfirmationSelection.SelectDefault();

        majorCheckPointText.text = "Reset to checkpoint:\n" + gm.CurrentMajorCheckpoint.CheckPointName + "?";

        isInRestoreConfirmation = true;

        pauseMenuPanel.SetActive(false);
    }

    public void ConfirmRestoreCheckpoint()
    {
        gm.RestoreCheckPoint();

        StopRestoreCheckpoint();
        ResumeGame();
    }
    
    public void StopRestoreCheckpoint()
    {
        if (!isInRestoreConfirmation)
            return;

        pauseMenuPanel.SetActive(true);
        pauseMenuSelection.SelectDefault();
        checkpointConfirmationPanel.SetActive(false);

        isInRestoreConfirmation = false;
    }

    public void OpenOptions()
    {
        if (isInOptions)
            return;

        optionsMenuPanel.SetActive(true);

        optionsMenuSelection.SelectDefault();

        isInOptions = true;

        pauseMenuPanel.SetActive(false);
    }

    public void CloseOptions()
    {
        if (!isInOptions)
            return;

        pauseMenuPanel.SetActive(true);

        pauseMenuSelection.SelectDefault();

        isInOptions = false;

        optionsMenuPanel.SetActive(false);
    }

    public void ChangeDisplay(int val)
    {
        if (isInChangeDisplayConfirmation)
            return;

        selectedDisplayOption = val;

        changeDisplayConfirmationPanel.SetActive(true);

        changeDisplayConfirmationSelection.SelectDefault();

        isInChangeDisplayConfirmation = true;

        optionsMenuPanel.SetActive(false);
    }

    public void ConfirmChangeDisplay()
    {
        if (!isInChangeDisplayConfirmation)
            return;

        DropdownHandler handler = optionsMenuPanel.GetComponentInChildren<DropdownHandler>();

        handler.HandleInput(selectedDisplayOption);

        StopDisplayChange();
    }

    public void StopDisplayChange()
    {
        if (!isInChangeDisplayConfirmation)
            return;

        optionsMenuPanel.SetActive(true);

        optionsMenuSelection.SelectDefault();

        isInChangeDisplayConfirmation = false;

        changeDisplayConfirmationPanel.SetActive(false);
    }

    public void ExitGame()
    {
        if (isInExitConfirmation)
            return;

        exitConfirmationPanel.SetActive(true);

        exitConfirmationSelection.SelectDefault();

        isInExitConfirmation = true;

        pauseMenuPanel.SetActive(false);
    }
        
    public void ConfirmExitGame() 
    {      
        Debug.Log("Exit application.");
        Application.Quit();

        StopExitGame();
    }

    public void StopExitGame()
    {
        if (!isInExitConfirmation)
            return;

        pauseMenuPanel.SetActive(true);
        pauseMenuSelection.SelectDefault();
        exitConfirmationPanel.SetActive(false);

        isInExitConfirmation = false;
    }
}
