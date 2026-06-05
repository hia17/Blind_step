using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Image[] icons;

    private void Start()
    {
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
            }
            else
            {
                icons[i].enabled = false;
            }
        }
    }
}