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
    public Image timeGauge;

    [Header("플레이어 초기화 설정")]
    public Transform playerTransform;
    public Transform respawnPoint;

    private bool isGameOver = false;
    private bool isPassingDay = false;

    [Header("트리거 그룹 설정")]
    [SerializeField] private WatcherTriggerArray[] triggerGroups;

    private List<int>[] _usedIndicesPerGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void Start()
    {
        _usedIndicesPerGroup = new List<int>[triggerGroups.Length];
        for (int i = 0; i < triggerGroups.Length; i++)
            _usedIndicesPerGroup[i] = new List<int>();

        currentTime = dayDuration;
        ActivateRandomTrigger();
    }

    private void Update()
    {
        if (isGameOver || isPassingDay) return;

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
        if (isGameOver || isPassingDay) return;
        StartCoroutine(PassDayRoutine());
    }

    private IEnumerator PassDayRoutine()
    {
        isPassingDay = true;

        Watcher watcher = FindFirstObjectByType<Watcher>();
        if (watcher != null)
            watcher.StopWatcher();

        InputManager.DeactivatePlayerControls();
        SceneFadeManager.instance.StartFadeOut();

        while (SceneFadeManager.instance.IsFadingOut)
            yield return null;

        if (currentDay >= maxDays)
        {
            GameOver();
            yield break;
        }

        currentDay++;
        ActivateRandomTrigger();
        currentTime = dayDuration;

        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.Heal(PlayerHealth.Instance.maxHealth);

        if (playerTransform != null && respawnPoint != null)
            playerTransform.position = respawnPoint.position;

        Debug.Log($"{currentDay}일차 시작!");

        yield return new WaitForSeconds(0.5f);
        SceneFadeManager.instance.StartFadeIn();

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

        if (timeGauge != null)
            timeGauge.fillAmount = currentTime / dayDuration;
    }

    private void ActivateRandomTrigger()
    {
        // 전체 비활성화
        foreach (var group in triggerGroups)
            foreach (var t in group.triggers)
                if (t != null) t.Deactivate();

        // 그룹마다 하나씩 랜덤 활성화
        for (int g = 0; g < triggerGroups.Length; g++)
        {
            WatcherSpawnTrigger[] arr = triggerGroups[g].triggers;
            if (arr == null || arr.Length == 0) continue;

            if (_usedIndicesPerGroup[g].Count >= arr.Length)
                _usedIndicesPerGroup[g].Clear();

            List<int> candidates = new List<int>();
            for (int i = 0; i < arr.Length; i++)
                if (!_usedIndicesPerGroup[g].Contains(i))
                    candidates.Add(i);

            int picked = candidates[Random.Range(0, candidates.Count)];
            if (arr[picked] != null)
                arr[picked].Activate();

            _usedIndicesPerGroup[g].Add(picked);
            Debug.Log($"[GameManager] 그룹 {g} → {arr[picked].gameObject.name} 활성화");
        }
    }
}

[System.Serializable]
public class WatcherTriggerArray
{
    public WatcherSpawnTrigger[] triggers;
}