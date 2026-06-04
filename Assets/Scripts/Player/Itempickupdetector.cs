using UnityEngine;

/// <summary>
/// 플레이어 오브젝트에 부착.
/// 근처 ItemObject 감지 → canGet UI 표시 → Get 키 입력 시 획득.
/// </summary>
public class ItemPickupDetector : MonoBehaviour
{
    [Header("감지 범위")]
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private LayerMask itemLayer;          // 아이템이 있는 레이어

    [Header("canGet UI (선택)")]
    [Tooltip("'F to Pick Up' 같은 안내 UI 오브젝트. 없으면 무시됨.")]
    [SerializeField] private GameObject canGetUI;

    private ItemObject nearestItem;

    private void Update()
    {
        FindNearestItem();
        HandleInput();
    }

    private void FindNearestItem()
    {
        // 범위 내 모든 콜라이더 검색
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius, itemLayer);

        ItemObject closest = null;
        float minDist = float.MaxValue;

        foreach (var col in hits)
        {
            ItemObject item = col.GetComponent<ItemObject>();
            
            if (item == null) continue;
          
            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = item;
            }
        }

        // 가장 가까운 아이템 교체
        nearestItem = closest;

        // canGet UI 표시 여부
        if (canGetUI != null)
            canGetUI.SetActive(nearestItem != null);
    }

    private void HandleInput()
    {
        // InputManager.usePressed 를 Get 키로 활용 (기존 구조 재사용)
        if (InputManager.getPressed && nearestItem != null)
        {
            nearestItem.PickUp();
            nearestItem = null;

            if (canGetUI != null)
                canGetUI.SetActive(false);
        }

        if (InputManager.dropPressed)
        {
            Inventory.Instance.DropItem(0, transform.position); // 예시로 첫 번째 아이템 드롭
        }
        if (InputManager.usePressed)
        {
            Inventory.Instance.UseItem(0); // 예시로 첫 번째 아이템 사용
        }
    }

  
    
}