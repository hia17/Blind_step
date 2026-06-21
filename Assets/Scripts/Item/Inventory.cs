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
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= items.Count) return;
        items.RemoveAt(index);
        OnInventoryChanged?.Invoke();
    }
    public void UseItem(int index)
    {
        if (index < 0) return;
        ItemData data = items[index];

        if (data.itemType == ItemData.ItemType.Food)
        {
            Food(data.healAmount);
            items.RemoveAt(index);
        }
        else if (data.itemType == ItemData.ItemType.badFood)
        { 
            BadFood(data.healAmount, data.buffTime);
            items.RemoveAt(index);
        }
        else if (data.itemType == ItemData.ItemType.stick)
        {
            Debug.Log($"Used {data.itemName}, it was a stick!");
        }
        else if (data.itemType == ItemData.ItemType.key)
        {
            Debug.Log($"Used {data.itemName}, it was a key!");
        }
        else if(data.itemType == ItemData.ItemType.medicine)
        {
            PlayerController.instance.GetMedicine();
            items.RemoveAt(index);
        }
        else if (data.itemType == ItemData.ItemType.obj)
        {
            return;
        }

        //items.RemoveAt(index);
        OnInventoryChanged?.Invoke();
    }

    private void Food(float hp)
    {
        PlayerHealth.Instance.Heal(hp);
    }
    private void BadFood(float hp, float t)
    {
        PlayerHealth.Instance.Heal(hp);
        PlayerController.instance.InDigest(hp+15f,t);

    }
}