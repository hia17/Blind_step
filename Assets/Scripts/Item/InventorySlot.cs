using UnityEngine;
using UnityEngine.EventSystems;

// 각 인벤토리 슬롯 Image 오브젝트에 부착
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ItemData itemData;

    // InventoryUI에서 아이템 정보를 주입
    public void SetItem(ItemData data)
    {
        itemData = data;
    }

    public void ClearItem()
    {
        itemData = null;
    }

    // 마우스가 슬롯 위에 올라왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData == null) return;
        TooltipUI.Instance.Show(itemData);
    }

    // 마우스가 슬롯에서 벗어났을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }
}