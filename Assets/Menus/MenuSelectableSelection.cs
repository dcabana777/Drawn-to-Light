using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class MenuSelectableSelection : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    
    private Selectable selectable;
    private MenuSelectionController menuController;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();

        menuController = GetComponentInParent<MenuSelectionController>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (menuController != null)
            menuController.SelectSelectable(selectable);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (menuController != null)
            menuController.SelectSelectable(selectable);
    }
}