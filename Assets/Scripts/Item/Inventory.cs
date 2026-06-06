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

    public void SwapItems(int indexA, int indexB)
    {
        if (indexA < 0 || indexB < 0) return;
        if (indexA >= items.Count || indexB >= items.Count) return;

        ItemData temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;

        OnInventoryChanged?.Invoke(); // UI 갱신
    }
    public void UseItem(int index)
    {
        if (index < 0) return;
        ItemData data = items[index];

        if (data.itemType == ItemData.ItemType.Food)
        {
            Debug.Log($"Used {data.itemName}, it was food!");
            PlayerHealth.Instance.Heal(20); // 좋은 음식이면 체력 20 회복
        }
        else if (data.itemType == ItemData.ItemType.badFood)
        {
            Debug.Log($"Used {data.itemName}, it was bad food!");
            PlayerHealth.Instance.TakeDamage(10); // 상한 음식이면 체력 10 깎임
        }
        else if (data.itemType == ItemData.ItemType.stick)
        {
            Debug.Log($"Used {data.itemName}, it was a stick!");
        }
        else if (data.itemType == ItemData.ItemType.key)
        {
            Debug.Log($"Used {data.itemName}, it was a key!");
        }

        items.RemoveAt(index);
        OnInventoryChanged?.Invoke();
    }
}