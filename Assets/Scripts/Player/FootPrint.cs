using UnityEngine;

public class FootprintFade : MonoBehaviour
{
    public float lifeTime = 2f; // 발자국이 완전히 사라지는 데 걸리는 시간 (2초)
    private SpriteRenderer spriteRenderer;
    private Color color;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        color = spriteRenderer.color; // 현재 색상 가져오기

        // lifeTime 초 뒤에 이 발자국 오브젝트를 파괴(삭제)함
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 매 프레임마다 투명도(a)를 조금씩 뺌
        color.a -= Time.deltaTime / lifeTime;
        spriteRenderer.color = color; // 변경된 투명도를 다시 적용
    }
}