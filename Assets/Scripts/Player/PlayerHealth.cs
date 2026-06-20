using UnityEngine;
using UnityEngine.UI; // UI를 다루기 위해 꼭 필요해!

public class PlayerHealth : MonoBehaviour
{
    // Inventory.cs 처럼 어디서든 쉽게 접근할 수 있게 Instance(싱글톤) 패턴 적용
    public static PlayerHealth Instance { get; private set; }

    [Header("체력 설정")]
    public float maxHealth = 100;
    public float currentHealth;

    [Header("UI 설정")]
    // ★ Slider 대신 Image로 변경되었습니다!
    public Image healthBarFill;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // (주의: 기존에 있던 FindWithTag 코드는 Image 방식에서 오류를 낼 수 있어 깔끔하게 지웠습니다. 에디터에서 직접 연결해 주시면 됩니다!)
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI(); // 시작할 때 체력바 꽉 채우기
    }

    // 데미지를 입을 때 부르는 함수
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0; // 체력이 0 밑으로 내려가지 않게

        UpdateHealthUI();
        PlayerStateList.isDamaged = true;
        StartCoroutine(PlayerController.instance.HitLightEffect());
        Debug.Log($"데미지 {damage} 받음. 현재 체력: {currentHealth}");

        // 체력이 0이 되면 게임오버 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 회복할 때 부르는 함수
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth; // 최대 체력 넘지 않게

        UpdateHealthUI();
        Debug.Log($"체력 {amount} 회복. 현재 체력: {currentHealth}");
    }

    // 체력바 UI의 길이를 현재 체력에 맞게 조절
    private void UpdateHealthUI()
    {
        // ★ Image의 fillAmount(0~1 비율)를 사용하여 체력을 깎습니다.
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    // 걸을 때 피격 이펙트 없이 조용히 기력만 소모하는 함수
    public void ConsumeStamina(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("플레이어 체력 0! 다음 날로 넘어갑니다.");

        // 체력이 0이 되면 GameManager를 통해 강제로 하루를 넘깁니다.
        GameManager.Instance.PassDay();
    }
}