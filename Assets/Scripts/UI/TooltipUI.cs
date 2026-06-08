using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RectTransform tooltipRect;

    private Vector2 defaultPosition;
    private void Awake()
    {
        Instance = this;
        
    }

    private void Start()
    {
        defaultPosition = new Vector2(-250f, 200f);
        Debug.Log(defaultPosition);
        gameObject.SetActive(false);
    }
    public void Show(ItemData data)
    {
        itemNameText.text = data.itemName;
        descriptionText.text = data.description;
        tooltipRect.anchoredPosition= defaultPosition;
        gameObject.SetActive(true);
    }
    public void Show(string name, string description, Vector2 anchoredPosition)
    {
        itemNameText.text = name;
        descriptionText.text = description;
        tooltipRect.anchoredPosition = anchoredPosition;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}