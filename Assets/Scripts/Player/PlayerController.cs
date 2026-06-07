using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
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


    //recoil변수
    [Header("반동관련 변수")]
    [SerializeField] private float recoilSpeed = 8f; //초기 반동속도
    [SerializeField] private int recoilSteps = 8; //몇프레임에 걸쳐 반동이일어날지
    [SerializeField] private float recoilDecay = 0.75f; //반동속도 감속비율

    private bool lightrecoil = false;
    private bool isRecoiling = false;  //반동중인지
    private int stepsRecoiled = 0;  //현재 몇프레임째 반동중인지
    private Vector2 recoilDir = Vector2.zero; //반동방향
    private float currentRecoilSpeed;  //현재 반동속도
    private Light2D hitLight;
    private Color originalColor;

    private Coroutine indigestCoroutine;
    private void Awake()
    {
        if(instance != null&&instance !=this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastFootprintPos = transform.position;
        hitLight = GetComponentInChildren<Light2D>();
        originalColor = hitLight.color;
    }

    void Update()
    {
        GetInput();


        HandleMovementState();
    }

    void FixedUpdate()
    {
        if (isRecoiling)
        {
            ApplyRecoil();
            return;
        }
        if (PlayerStateList.isDamaged)
        {
            rb.linearVelocity = Vector2.zero; // 피격 중 완전 정지
            return;
        }
        Move();
    }
    #region 이동
    private void GetInput()
    {
        Vector2 moveDir = InputManager.moveDir;
        xAxis = moveDir.x;
        yAxis = moveDir.y;
    }
    private void Move()
    {
        Vector3 moveVector = new Vector3(xAxis * moveSpeed, yAxis * moveSpeed, 0f);
        rb.linearVelocity = moveVector;
    }
    #endregion

    #region 충돌 반동
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Vector2 dir = (transform.position - collision.transform.position).normalized;
            TriggerRecoil(dir);
            TakeDamage();
        }
    }

    public void TriggerRecoil(Vector2 contactNormal)
    {
        recoilDir = contactNormal.normalized;
        currentRecoilSpeed = recoilSpeed;
        stepsRecoiled = 0;
        isRecoiling = true;
    }

    private void ApplyRecoil()
    {
        
        rb.linearVelocity = recoilDir * currentRecoilSpeed;
        currentRecoilSpeed *= recoilDecay;

        stepsRecoiled++;
        if (stepsRecoiled >= recoilSteps )
            StopRecoil();
    }
    private void StopRecoil()
    {
        isRecoiling = false;
        stepsRecoiled = 0;
        recoilDir = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }
    public void TakeDamage()
    {
        PlayerStateList.damagedCount++;
        PlayerHealth.Instance.TakeDamage(10);
        


    }

    
    public IEnumerator HitLightEffect()
    {
        
        int blinkCount = 3;        // 깜빡임 횟수
        float blinkInterval = 0.2f; // 빨강 ↔ 원래색 간격(초)
        

        for (int i = 0; i < blinkCount; i++)
        {
            hitLight.color = Color.red;
            yield return new WaitForSeconds(blinkInterval);
            hitLight.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);
        }
        PlayerStateList.isDamaged = false;
    }
    #endregion

    public void InDigest(float hp, float t)
    {
        if (!PlayerStateList.indigest)
        {
            SetMoveSpeed();
            PlayerStateList.indigest = true;
        }

        if (PlayerStateList.indigest)
        {
            moveSpeed /= 2f;
            ChangeMoveSpeed(moveSpeed);
            if(indigestCoroutine == null)
            {
                indigestCoroutine = StartCoroutine(ShouldGoToToilet(hp,t));
            }
            else
            {
                
                PlayerHealth.Instance.TakeDamage(hp);
            }
        }
    }

    IEnumerator ShouldGoToToilet(float hp, float timelimit)
    {
        yield return new WaitForSeconds(timelimit);

        if (PlayerStateList.indigest)
        {
            //피깎이기
            PlayerHealth.Instance.TakeDamage(hp);
            PlayerStateList.indigest = false;
            ChangeMoveSpeed(PlayerStateList.originSpeed);
        }
        
        indigestCoroutine = null;
    }
    public void GetMedicine()
    {
        PlayerStateList.indigest = false;
        PlayerStateList.isHurt = false;
        ChangeMoveSpeed(PlayerStateList.originSpeed);
    }

    private void GetHurt()
    {
        if (PlayerStateList.damagedCount == 3)
        {
            PlayerStateList.isHurt = true;
            PlayerStateList.damagedCount = 0;
            PlayerStateList.originSpeed = moveSpeed;
            ChangeMoveSpeed(3);
        }
    }
    private void ChangeMoveSpeed(float speed)
    {
        moveSpeed = speed;
        
    }
    private void SetMoveSpeed()
    {
        PlayerStateList.originSpeed = moveSpeed;
    }


    #region 발자국
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
    #endregion

}