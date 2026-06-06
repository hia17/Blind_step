using UnityEngine;

/// <summary>
/// 문 오브젝트에 붙이는 스크립트.
/// 플레이어가 감지 범위 내에 들어오면 canUseUI가 표시되고,
/// Use 키를 누르면 문이 열리거나 닫힌다.
/// </summary>
public class DoorObject : MonoBehaviour
{
    public static bool isDetected = false;
    [Header("감지 설정")]
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform detectionCenter;

    [Header("canUse UI")]
    [SerializeField] private GameObject canOpenUI;   // 닫혔을 때 표시
    [SerializeField] private GameObject canCloseUI;  // 열렸을 때 표시
    [SerializeField] private Vector3 openuiOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 closeuiOffset = new Vector3(0f, 1.5f, 0f);

    [Header("문 설정")]
    [SerializeField] private Transform doorPivot;   // 회전 기준점 오브젝트
    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float openAngle = 90f;

    private bool isOpen = false;
    private bool playerInRange = false;

    private void Update()
    {
        CheckPlayerRange();
        HandleInput();
    }

    // ── 범위 감지 ──────────────────────────────────────────
    private void CheckPlayerRange()
    {
        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
        Collider2D hit = Physics2D.OverlapCircle(center, interactRadius, playerLayer);
        playerInRange = (hit != null);
        isDetected = playerInRange;
        if (playerInRange)
        {
            Vector3 uiPos = center + openuiOffset;
            canOpenUI.transform.position = uiPos;
            uiPos = center + closeuiOffset;
            canCloseUI.transform.position = uiPos;
        }
        canOpenUI.SetActive(playerInRange && !isOpen);
        canCloseUI.SetActive(playerInRange && isOpen);
    }

    // ── 입력 처리 ──────────────────────────────────────────
    private void HandleInput()
    {
        if (playerInRange && InputManager.usePressed)
            ToggleDoor();
    }

    // ── 문 열기/닫기 ───────────────────────────────────────
    private void ToggleDoor()
    {
        isOpen = !isOpen;
        ApplyDoorAngle();
    }

    private void ApplyDoorAngle()
    {
        float targetAngle = isOpen ? openAngle : closedAngle;

        // doorPivot이 지정된 경우 피벗 기준으로, 아니면 자기 자신 회전
        Transform target = doorPivot != null ? doorPivot : transform;
        target.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    // ── 에디터에서 범위 확인용 Gizmo ──────────────────────
    private void OnDrawGizmosSelected()
    {
        // detectionCenter가 지정된 경우 그 위치 기준으로 그리기
        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, interactRadius);
    }
}