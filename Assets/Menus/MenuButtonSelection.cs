using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonSelection : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    private Button button;
    private MenuSelectionController menuController;

    private void Awake()
    {
        button = GetComponent<Button>();

        menuController = GetComponentInParent<MenuSelectionController>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (menuController != null)
            menuController.SelectButton(button);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (menuController != null)
            menuController.SelectButton(button);
    }
}