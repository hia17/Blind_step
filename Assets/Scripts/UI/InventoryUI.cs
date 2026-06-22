using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Image[] icons;
    private InventorySlot[] slots;

    private void Start()
    {
        slots = new InventorySlot[icons.Length];
        for (int i = 0; i < icons.Length; i++)
        {
            slots[i] = icons[i].GetComponent<InventorySlot>();
        }

        Inventory.Instance.OnInventoryChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        var items = Inventory.Instance.Items;

        for (int i = 0; i < icons.Length; i++)
        {
            if (i < items.Count)
            {
                icons[i].sprite = items[i].icon;
                icons[i].enabled = true;
                slots[i].SetItem(items[i]);
            }
            else
            {
                icons[i].enabled = false;
                slots[i].ClearItem();
            }
        }
    }
}