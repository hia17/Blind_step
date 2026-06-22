using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text pointText;
    public TMP_Text staminaLevelText;
    public TMP_Text waveCountLevelText;
    public TMP_Text waveSpeedLevelText;

    private int currentPoints;

    private void Start()
    {
        // 현재 보유한 포인트를 불러옵니다. (테스트용으로 기본값 10을 줬습니다)
        currentPoints = PlayerPrefs.GetInt("UpgradePoints", 5);
        UpdateUI();
    }

    // 1. 기력 증가
    public void UpgradeStamina()
    {
        if (currentPoints > 0)
        {
            currentPoints--;
            PlayerStateList.healthUpgrade++;
            int level = PlayerPrefs.GetInt("StaminaLevel", 0) + 1;
            PlayerPrefs.SetInt("StaminaLevel", level);
            SavePoints();
        }
    }

    // 2. 파형 갯수 증가
    public void UpgradeWaveCount()
    {
        if (currentPoints > 0)
        {
            currentPoints--;
            PlayerStateList.rayUpgrade++;
            int level = PlayerPrefs.GetInt("WaveCountLevel", 0) + 1;
            PlayerPrefs.SetInt("WaveCountLevel", level);
            SavePoints();
        }
    }

    // 3. 파형 속도 증가
    public void UpgradeWaveSpeed()
    {
        if (currentPoints > 0)
        {
            currentPoints--;
            PlayerStateList.raySpeedUpgrade++;
            int level = PlayerPrefs.GetInt("WaveSpeedLevel", 0) + 1;
            PlayerPrefs.SetInt("WaveSpeedLevel", level);
            SavePoints();
        }
    }

    // 메인 화면으로 돌아가기
    public void GoBackToMain()
    {
        // 씬 로더가 있다면 페이드 효과와 함께 MainScene으로 이동
        SceneFadeManager.instance.StartFadeOut();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    private void SavePoints()
    {
        PlayerPrefs.SetInt("UpgradePoints", currentPoints);
        PlayerPrefs.Save(); // 디스크에 즉시 저장
        UpdateUI();
    }

    // ★ 추가됨: 포인트 초기화 및 환불 함수
    public void ResetPoints()
    {
        // 1. 지금까지 투자한 레벨의 총합을 구해서 포인트로 100% 환불해줍니다.
        int spentPoints = PlayerPrefs.GetInt("StaminaLevel", 0) +
                          PlayerPrefs.GetInt("WaveCountLevel", 0) +
                          PlayerPrefs.GetInt("WaveSpeedLevel", 0);
        PlayerStateList.healthUpgrade = 0;
        PlayerStateList.raySpeedUpgrade = 0;
        PlayerStateList.rayUpgrade = 0;
        currentPoints += spentPoints;

        // 2. 모든 능력치 레벨을 0으로 초기화합니다.
        PlayerPrefs.SetInt("StaminaLevel", 0);
        PlayerPrefs.SetInt("WaveCountLevel", 0);
        PlayerPrefs.SetInt("WaveSpeedLevel", 0);

        // 3. 환불된 포인트를 저장하고 화면 글자를 업데이트합니다.
        SavePoints();

        Debug.Log($"포인트 초기화 완료! 환불된 포인트: {spentPoints}");
    }

    private void UpdateUI()
    {
        pointText.text = $"현재 보유 포인트 : {currentPoints}";
        staminaLevelText.text = $"기력: Lv.{PlayerPrefs.GetInt("StaminaLevel", 0)}";
        waveCountLevelText.text = $"지팡이 파형 갯수: Lv.{PlayerPrefs.GetInt("WaveCountLevel", 0)}";
        waveSpeedLevelText.text = $"지팡이 파형 발사 속도: Lv.{PlayerPrefs.GetInt("WaveSpeedLevel", 0)}";
    }
}