using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickupDetector : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private LayerMask itemLayer;

    [Header("canGet UI")]
    [SerializeField] private GameObject canGetUI;

    private ItemObject nearestItem;


    private void Update()
    {
        FindNearestItem();
        HandleInput();
    }

    private void FindNearestItem()
    {
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

        nearestItem = closest;
        canGetUI.SetActive(nearestItem != null);

        if (nearestItem != null)
            MoveUIToItem(nearestItem.transform.position);
    }

    private void MoveUIToItem(Vector3 worldPos)
    {
        canGetUI.transform.position = worldPos + new Vector3(0, 1.2f, 0);
    }

    private void HandleInput()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll < 0) // 휠 내리기
        {
            if (Inventory.Instance.Items.Count == 2)
                Inventory.Instance.SwapItems(0, 1);
        }
        if (InputManager.getPressed && nearestItem != null)
        {
            InputConsumer.Consume();
            nearestItem.PickUp();
            nearestItem = null;
            canGetUI.SetActive(false);
        }

        if (InputManager.dropPressed&& Inventory.Instance.Items.Count != 0)
        {
            Vector3 pos = transform.position;
            pos.y += 1f;
            Inventory.Instance.DropItem(0, pos);
        }
            

        if (InputManager.usePressed && !DoorObject.isDetected&& Inventory.Instance.Items.Count != 0)
            Inventory.Instance.UseItem(0);
    }
}