using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("시간 및 날짜 설정")]
    public int maxDays = 3;
    public int currentDay = 1;
    public float dayDuration = 300f;
    private float currentTime;

    [Header("UI 설정")]
    public TMP_Text dDayText;
    public TMP_Text timeText;
    public Image timeGauge;

    [Header("플레이어 초기화 설정")]
    public Transform playerTransform;
    public Transform respawnPoint;

    private bool isGameOver = false;
    private bool isPassingDay = false; // ★ 중복 호출 방지용

    [Header("감시자 트리거 설정")]
    [SerializeField] private WatcherSpawnTrigger[] watcherTriggers;

    private bool _watcherSpawnedToday = false;
    private List<int> _usedTriggerIndices = new List<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void Start()
    {
        currentTime = dayDuration;
        ActivateRandomTrigger();
    }

    private void Update()
    {
        if (isGameOver || isPassingDay) return; // ★ 페이드 중엔 타이머 정지

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
        if (isGameOver || isPassingDay) return; // ★ 중복 방지
        StartCoroutine(PassDayRoutine());
    }

    private IEnumerator PassDayRoutine()
    {
        isPassingDay = true;

        Watcher watcher = FindFirstObjectByType<Watcher>();
        if (watcher != null)
            watcher.StopWatcher();

        // 1. 입력 차단 + 페이드아웃
        InputManager.DeactivatePlayerControls();
        SceneFadeManager.instance.StartFadeOut();

        while (SceneFadeManager.instance.IsFadingOut)
            yield return null;

        // 2. 날짜 처리 (화면이 검은 동안)
        if (currentDay >= maxDays)
        {
            GameOver();
            yield break; // 게임오버면 페이드인 없이 종료
        }

        currentDay++;
        ActivateRandomTrigger();
        currentTime = dayDuration;

        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.Heal(PlayerHealth.Instance.maxHealth);

        if (playerTransform != null && respawnPoint != null)
            playerTransform.position = respawnPoint.position;

        Debug.Log($"{currentDay}일차 시작!");

        // 3. 잠깐 대기 후 페이드인 (SceneFadeManager의 StartFadeIn이 alpha>=1 체크함)
        yield return new WaitForSeconds(0.5f);
        SceneFadeManager.instance.StartFadeIn();

        // SceneFadeManager의 FadeIn 완료 시 자동으로 InputManager.ActivatePlayerControls() 호출됨

        isPassingDay = false;
    }

    private void GameOver()
    {
        isGameOver = true;
        isPassingDay = false;
        Debug.Log("게임 오버!");
        // TODO: 게임오버 UI 표시
    }

    private void UpdateUI()
    {
        if (dDayText != null)
            dDayText.text = $"D - {maxDays - currentDay + 1}";

        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (timeGauge != null)
            timeGauge.fillAmount = currentTime / dayDuration;
    }


    private void ActivateRandomTrigger()
    {
        if (watcherTriggers == null || watcherTriggers.Length == 0) return;

        foreach (var t in watcherTriggers)
            t.Deactivate();

        // 모든 인덱스가 사용됐으면 초기화 (순환)
        if (_usedTriggerIndices.Count >= watcherTriggers.Length)
            _usedTriggerIndices.Clear();

        // 미사용 인덱스 후보 추출
        List<int> candidates = new List<int>();
        for (int i = 0; i < watcherTriggers.Length; i++)
        {
            if (!_usedTriggerIndices.Contains(i))
                candidates.Add(i);
        }

        int picked = candidates[Random.Range(0, candidates.Count)];
        watcherTriggers[picked].Activate();
        _usedTriggerIndices.Add(picked); // ★ 사용 목록에 추가
        _watcherSpawnedToday = false;

        Debug.Log($"[GameManager] 트리거 {picked}번 활성화 (사용됨: {string.Join(",", _usedTriggerIndices)})");
    }

    public void OnWatcherTriggered()
    {
        _watcherSpawnedToday = true;
        // 나머지 트리거도 전부 끄기
        foreach (var t in watcherTriggers)
            t.Deactivate();
    }
}