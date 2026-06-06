using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RectTransform tooltipRect;

    private void Awake()
    {
        Instance = this;
        
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }
    public void Show(ItemData data)
    {
        itemNameText.text = data.itemName;
        descriptionText.text = data.description;
        
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}