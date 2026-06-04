using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("설정")]
    [SerializeField] private int maxSlots = 20;

    // 인벤토리에 담긴 아이템 목록
    private List<ItemData> items = new List<ItemData>();
    public IReadOnlyList<ItemData> Items => items;

    // 인벤토리 변경 시 UI에 알리는 이벤트
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

 
    public bool AddItem(ItemData data)
    {
        if (items.Count >= maxSlots) return false;
        items.Add(data);
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>인덱스 위치의 아이템을 버린다 (플레이어 위치에 월드 오브젝트 생성).</summary>
    public void DropItem(int index, Vector3 dropPosition)
    {
        if (index < 0 || index >= items.Count) return;

        ItemData data = items[index];

        // 월드에 아이템 오브젝트 생성
        if (data.worldPrefab != null)
            Instantiate(data.worldPrefab, dropPosition, Quaternion.identity);

        items.RemoveAt(index);
        OnInventoryChanged?.Invoke();
    }
    public void UseItem(int index)
    {
        if (index < 0) return;
        ItemData data = items[index];

        if(data.itemType == ItemData.ItemType.Food)
        {
            // 음식 아이템 사용 시 체력 회복 등 효과 적용
            Debug.Log($"Used {data.itemName}, it was food!");
            // 예시: PlayerHealth.Instance.Heal(20);
        }
        else if(data.itemType == ItemData.ItemType.badFood)
        {
            // 나쁜 음식 아이템 사용 시 체력 감소 등 효과 적용
            Debug.Log($"Used {data.itemName}, it was bad food!");
            // 예시: PlayerHealth.Instance.TakeDamage(10);
        }
        else if(data.itemType == ItemData.ItemType.stick)
        {
            // 막대기 아이템 사용 시 공격력 증가 등 효과 적용
            Debug.Log($"Used {data.itemName}, it was a stick!");
            // 예시: PlayerAttack.Instance.IncreaseDamage(5, duration: 10f);
        }
        else if(data.itemType == ItemData.ItemType.key)
        {
            // 열쇠 아이템 사용 시 문 열기 등 효과 적용
            Debug.Log($"Used {data.itemName}, it was a key!");
            // 예시: Door.Instance.Unlock();
        }
        items.RemoveAt(index);
        OnInventoryChanged?.Invoke();

    }
}