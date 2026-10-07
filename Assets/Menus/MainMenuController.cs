using Microsoft.Extensions.DependencyInjection;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;
    [SerializeField] private GameObject changeDisplayConfirmationPanel;

    [Header("Selection Controllers")]
    [SerializeField] private MenuSelectionController mainMenuSelection;
    [SerializeField] private MenuSelectionController optionsMenuSelection;
    [SerializeField] private MenuSelectionController changeDisplayConfirmationSelection;

    private bool isInOptions = false;
    private bool isInChangeDisplayConfirmation = false;
    private int currentDisplayOption;
    private int selectedDisplayOption;

    private void Awake()
    {
        if (Screen.fullScreenMode == FullScreenMode.Windowed)
            currentDisplayOption = 1;
        else
            currentDisplayOption = 0;

        selectedDisplayOption = currentDisplayOption;
    }
    
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenOptions()
    {
        if (isInOptions)
            return;

        optionsMenuPanel.SetActive(true);

        optionsMenuSelection.SelectDefault();

        isInOptions = true;

        mainMenuPanel.SetActive(false);
    }

    public void CloseOptions()
    {
        if (!isInOptions)
            return;

        mainMenuPanel.SetActive(true);

        mainMenuSelection.SelectDefault();

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
    
    public void QuitGame()
    {
        Debug.Log("Exit application.");
        Application.Quit();
    }
}
