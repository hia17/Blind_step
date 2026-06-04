using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float moveSpeed = 5f;
    private float xAxis;
    private float yAxis;

    [Header("발자국 설정")]
    [SerializeField] private GameObject leftFootPrefab;
    [SerializeField] private GameObject rightFootPrefab;
    [SerializeField] private float stepDistance = 1.0f;
    [SerializeField] private float stepWidth = 0.2f;

    [Header("회전 설정")]
    [SerializeField] private float rotationSpeed = 20f;

    private Vector2 lastFootprintPos;
    private bool isLeftStep = true;
    private Vector2 lastMoveDir = Vector2.up;

    // ★ 상태 체크 및 "마지막 발자국 하나"만 기억하기 위한 변수
    private bool wasMoving = false;
    private GameObject lastSpawnedFootprint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastFootprintPos = transform.position;
    }

    void Update()
    {
        GetInput();

        if (InputManager.usePressed)
        {
            Debug.Log("Use Pressed");
        }

        HandleMovementState();
    }

    void FixedUpdate()
    {
        Vector3 moveVector = new Vector3(xAxis * moveSpeed, yAxis * moveSpeed, 0f);
        rb.linearVelocity = moveVector;
    }

    private void GetInput()
    {
        Vector2 moveDir = InputManager.moveDir;
        xAxis = moveDir.x;
        yAxis = moveDir.y;
    }

    private void HandleMovementState()
    {
        bool isMoving = (xAxis != 0 || yAxis != 0);

        if (isMoving)
        {
            spriteRenderer.enabled = false;
            wasMoving = true;

            lastMoveDir = new Vector2(xAxis, yAxis).normalized;
            float targetAngle = Mathf.Atan2(lastMoveDir.y, lastMoveDir.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            HandleFootprints();
        }
        else
        {
            // 방금 막 멈춘 '그 순간'
            if (wasMoving)
            {
                // 지금 멈춘 위치와 마지막 한 발자국 사이의 거리를 계산합니다.
                float distanceToLast = Vector2.Distance(transform.position, lastFootprintPos);

                // 만약 그 거리가 보폭(stepDistance)의 60%보다 짧아서 너무 겹친다면
                if (distanceToLast < (stepDistance * 0.6f) && lastSpawnedFootprint != null)
                {
                    // 겹치는 마지막 한 발자국만 제거하여 양발자국 하나만 깔끔하게 남깁니다.
                    Destroy(lastSpawnedFootprint);
                }

                wasMoving = false;
            }

            spriteRenderer.enabled = true;
        }
    }

    private void HandleFootprints()
    {
        float distance = Vector2.Distance(transform.position, lastFootprintPos);

        if (distance >= stepDistance)
        {
            Vector2 rightDirection = transform.right;
            Vector3 spawnPosition;
            GameObject prefabToSpawn;

            if (isLeftStep)
            {
                spawnPosition = transform.position - (Vector3)(rightDirection * stepWidth);
                prefabToSpawn = leftFootPrefab;
            }
            else
            {
                spawnPosition = transform.position + (Vector3)(rightDirection * stepWidth);
                prefabToSpawn = rightFootPrefab;
            }

            // 발자국을 찍고, 그 발자국을 '마지막 발자국'으로 기억해둡니다.
            lastSpawnedFootprint = Instantiate(prefabToSpawn, spawnPosition, transform.rotation);

            isLeftStep = !isLeftStep;
            lastFootprintPos = transform.position;
        }
    }
}