using UnityEngine;
using TMPro;
using UnityEngine.UI; // ★ 새로 추가됨: 유니티 UI(Image)를 다루기 위해 필요합니다.

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("시간 및 날짜 설정")]
    public int maxDays = 3;
    public int currentDay = 1;
    public float dayDuration = 300f; // 5분 = 300초
    private float currentTime;

    [Header("UI 설정")]
    public TMP_Text dDayText;
    public TMP_Text timeText;
    public Image timeGauge; // ★ 새로 추가됨: 시각적으로 줄어들 시계 게이지 이미지

    [Header("플레이어 초기화 설정")]
    public Transform playerTransform;
    public Transform respawnPoint;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentTime = dayDuration;
    }

    private void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            PassDay();
        }

        UpdateUI();
    }

    public void PassDay()
    {
        if (isGameOver) return;

        if (currentDay >= maxDays)
        {
            GameOver();
        }
        else
        {
            currentDay++;
            currentTime = dayDuration;

            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.Heal(PlayerHealth.Instance.maxHealth);
            }

            if (playerTransform != null && respawnPoint != null)
            {
                playerTransform.position = respawnPoint.position;
            }

            Debug.Log($"{currentDay}일차 시작!");
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        Debug.Log("게임 오버! 3일이 지났거나 마지막 날 체력이 다했습니다.");
    }

    private void UpdateUI()
    {
        if (dDayText != null)
        {
            dDayText.text = $"D - {maxDays - currentDay + 1}";
        }

        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // ★ 새로 추가됨: 남은 시간에 비율을 맞춰서 게이지를 깎습니다 (1.0 = 꽉참, 0.0 = 텅빔)
        if (timeGauge != null)
        {
            timeGauge.fillAmount = currentTime / dayDuration;
        }
    }
}