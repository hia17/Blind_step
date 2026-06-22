using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ItemData itemData;
    private bool isHovering = false;

    public void SetItem(ItemData data)
    {
        itemData = data;
    }

    public void ClearItem()
    {
        itemData = null;
        if (isHovering)
        {
            TooltipUI.Instance.Hide();
            isHovering = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData == null) return;
        isHovering = true;
        TooltipUI.Instance.Show(itemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        TooltipUI.Instance.Hide();
    }

    private void Update()
    {
        // IPointerEnterHandler 백업용 - 빌드에서 안될 때 대비
        RectTransform rect = GetComponent<RectTransform>();
        bool mouseOver = RectTransformUtility.RectangleContainsScreenPoint(
            rect,
            Mouse.current.position.ReadValue(),
            null
        );

        if (mouseOver && !isHovering && itemData != null)
        {
            isHovering = true;
            TooltipUI.Instance.Show(itemData);
        }
        else if (!mouseOver && isHovering)
        {
            isHovering = false;
            TooltipUI.Instance.Hide();
        }
    }
}