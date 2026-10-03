using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelectionController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button defaultButton;
    [SerializeField] private List<Button> menuButtons;

    [Header("Selection")]
    [SerializeField] private bool keepSelectionWhenLost = true;

    public Button CurrentButton { get; private set; }

    private void OnEnable()
    {
        // wait until menu fully loaded before assigning EventSystem selection
        StartCoroutine(SelectDefaultNextFrame());
    }

    private IEnumerator SelectDefaultNextFrame()
    {
        yield return null;

        if (defaultButton != null)
        {
            SelectButton(defaultButton);
        }
    }

    private void Update()
    {
        if (EventSystem.current == null)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject != null)
        {
            Button selectedButton = selectedObject.GetComponent<Button>();

            if (selectedButton != null && menuButtons.Contains(selectedButton))
            {
                CurrentButton = selectedButton;
                
                return;
            }
        }

        if (keepSelectionWhenLost && CurrentButton != null)
        {
            CurrentButton.Select();
        }
    }

    public void SelectButton(Button button)
    {
        if (button == null)
            return;

        if (!button.gameObject.activeInHierarchy || !button.interactable)
            return;

        CurrentButton = button;

        button.Select();
    }

    public void SelectDefault()
    {
        SelectButton(defaultButton);
    }

    private void ActivateCurrentButton()
    {
        if (CurrentButton == null)
            return;

        if (!CurrentButton.gameObject.activeInHierarchy || !CurrentButton.interactable)
            return;

        CurrentButton.onClick.Invoke();
    }
}
