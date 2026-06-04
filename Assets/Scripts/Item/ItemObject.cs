using UnityEngine;

/// <summary>
/// 맵 위에 존재하는 아이템 오브젝트.
/// 플레이어가 범위 내에 들어오면 canGet UI가 표시되고,
/// Get 키를 누르면 인벤토리에 추가된 후 맵에서 제거된다.
/// </summary>
public class ItemObject : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    // ItemPickupDetector가 감지 후 등록/해제
    public ItemData Data => itemData;

    /// <summary>아이템을 인벤토리에 추가하고 맵에서 제거한다.</summary>
    public void PickUp()
    {
        if (Inventory.Instance.AddItem(itemData))
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
        }
    }
}