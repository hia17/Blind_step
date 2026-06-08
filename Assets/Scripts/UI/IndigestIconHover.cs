using UnityEngine;
using UnityEngine.EventSystems;

public class IndigestIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("툴팁 내용")]
    [SerializeField] private string iconName = "소화불량";
    [TextArea]
    [SerializeField] private string description = "시간 안에 약을 먹지 않으면 데미지를 받습니다.";

    [Header("툴팁 위치 (앵커 기준 오프셋)")]
    [SerializeField] private Vector2 tooltipOffset = new Vector2(120f, 0f); // 아이콘 오른쪽

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector2 pos = rectTransform.anchoredPosition + tooltipOffset;
        TooltipUI.Instance.Show(iconName, description, pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.Hide();
    }
}