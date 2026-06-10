using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemData")]


public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        Food = 0,
        badFood = 1,
        stick =2,
        key = 3,
        medicine = 4,

    }
    [Header("기본 정보")]
    public string itemName = "아이템";
    public Sprite icon;                  // 인벤토리 UI에 표시될 아이콘(선택)
    public GameObject worldPrefab;       // 맵에 떨어질 때 생성될 프리팹
    public ItemType itemType;
    public string keyID = null;
    [Header("설명")]
    [TextArea] public string description;
    public float healAmount = 0f;
    public float buffTime = 0f;
}
