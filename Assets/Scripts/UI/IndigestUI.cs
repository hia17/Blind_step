using UnityEngine;
using UnityEngine.UI;

public class IndigestUI : MonoBehaviour
{
    public static IndigestUI Instance { get; private set; }

    [Header("UI 레퍼런스")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image timerImage;   // Slider 대신 Image

    [Header("색상 변화")]
    [SerializeField] private Color startColor = Color.yellow;
    [SerializeField] private Color endColor = Color.red;

    private float totalTime;
    private float elapsed;
    private bool isRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => HideUI();

    private void Update()
    {
        if (!isRunning) return;

        if (!PlayerStateList.indigest)
        {
            HideUI();
            return;
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / totalTime);

        timerImage.fillAmount = t;                            // 밑에서 위로 차오름
        timerImage.color = Color.Lerp(startColor, endColor, t); // 노랑 → 빨강

        if (elapsed >= totalTime)
            HideUI();
    }

    public void StartTimer(float duration)
    {
        totalTime = duration;
        elapsed = 0f;
        isRunning = true;
        timerImage.fillAmount = 0f;
        timerImage.color = startColor;
        panel.SetActive(true);
    }

    private void HideUI()
    {
        isRunning = false;
        panel.SetActive(false);
        TooltipUI.Instance.Hide();
    }
}