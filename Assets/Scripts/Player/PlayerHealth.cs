using UnityEngine;
using UnityEngine.UI; // UI(체력바)를 다루기 위해 꼭 필요해!

public class PlayerHealth : MonoBehaviour
{
    // Inventory.cs 처럼 어디서든 쉽게 접근할 수 있게 Instance(싱글톤) 패턴 적용
    public static PlayerHealth Instance { get; private set; }

    [Header("체력 설정")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI 설정")]
    public Slider healthBar; // 유니티의 체력바 UI

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI(); // 시작할 때 체력바 꽉 채우기
    }

    // 데미지를 입을 때 부르는 함수 (장애물, 상한 음식)
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0; // 체력이 0 밑으로 내려가지 않게

        UpdateHealthUI();
        Debug.Log($"데미지 {damage} 받음. 현재 체력: {currentHealth}");

        // 체력이 0이 되면 게임오버 처리 (기획서의 실패 조건!)
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 회복할 때 부르는 함수 (정상 음식)
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth; // 최대 체력 넘지 않게

        UpdateHealthUI();
        Debug.Log($"체력 {amount} 회복. 현재 체력: {currentHealth}");
    }

    // 체력바 UI의 길이를 현재 체력에 맞게 조절
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    private void Die()
    {
        Debug.Log("플레이어 체력 0! 다음 날로 넘어갑니다.");

        // 체력이 0이 되면 GameManager를 통해 강제로 하루를 넘깁니다.
        GameManager.Instance.PassDay();
    }
}