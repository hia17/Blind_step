using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("데미지 설정")]
    public int damageAmount = 15;

    // 플레이어가 콜라이더(물리 충돌체)에 부딪혔을 때 자동으로 실행됨
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 애가 플레이어인지 확인해! (유니티에서 태그 설정 필요)
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth.Instance.TakeDamage(damageAmount);
        }
    }
}