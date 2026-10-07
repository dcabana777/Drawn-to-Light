using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelectionController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Selectable defaultSelectable;
    [SerializeField] private List<Selectable> menuSelectables;

    [Header("Selection")]
    [SerializeField] private bool keepSelectionWhenLost = true;

    public Selectable CurrentSelectable { get; private set; }

    private void OnEnable()
    {
        // wait until menu fully loaded before assigning EventSystem selection
        StartCoroutine(SelectDefaultNextFrame());
    }

    private IEnumerator SelectDefaultNextFrame()
    {
        yield return null;

        SelectDefault();
    }

    private void Update()
    {
        if (EventSystem.current == null)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject != null)
        {
            Selectable selectedSelectable = selectedObject.GetComponent<Button>();

            if (selectedSelectable != null && menuSelectables.Contains(selectedSelectable))
            {
                CurrentSelectable = selectedSelectable;

                return;
            }

            if (CurrentSelectable is TMP_Dropdown dropdown && dropdown.IsExpanded)
            {
                return;
            }

            return;
        }

        if (!keepSelectionWhenLost)
            return;

        if (CurrentSelectable is TMP_Dropdown currentDropdown && currentDropdown.IsExpanded)
            return;

        if (IsSelectableValid(CurrentSelectable))
        {
            SelectSelectable(CurrentSelectable);
        }
    }

    public void SelectSelectable(Selectable selectable)
    {
        if (!IsSelectableValid(selectable))
        {
            return;
        }

        CurrentSelectable = selectable;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }
    }

    public void SelectDefault()
    {
        SelectSelectable(defaultSelectable);
    }

    private void ActivateCurrentSelectable()
    {
        if (EventSystem.current == null) 
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject == null && CurrentSelectable != null)
        {
            selectedObject = CurrentSelectable.gameObject;
        }

        if (selectedObject == null)
        {
            return;
        }

        BaseEventData eventData = new BaseEventData(EventSystem.current);

        ExecuteEvents.Execute(selectedObject, eventData, ExecuteEvents.submitHandler);
    }

    private bool IsSelectableValid(Selectable selectable) {
        return selectable != null && selectable.gameObject.activeInHierarchy && selectable.IsInteractable();
    }
}
