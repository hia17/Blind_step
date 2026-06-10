using System.Collections;
using UnityEngine;

/// <summary>
/// 조사 가능한 오브젝트의 부모 클래스.
/// Physics2D.OverlapCircle로 플레이어 감지 → UI 표시 → 조사 코루틴 → OnInspectComplete() 호출.
/// 컬라이더를 범위 감지에 사용하지 않으므로 별도 Collider 추가 불필요.
///
/// [자식 클래스 작성법]
///   public class DeskInspectable : InspectableBase
///   {
///       protected override void OnInspectComplete()
///       {
///           // 조사 완료 후 실행할 로직
///       }
///   }
/// </summary>
public class ObjectTrigger : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float interactRadius = 2f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] protected Transform detectionCenter; // 미지정 시 자기 자신 위치 사용

    [Header("조사 UI")]
    [SerializeField] private GameObject inspectPromptUI;  // "조사하기" 안내 UI (태그: InspectPromptUI)
    [SerializeField] private GameObject inspectingUI;     // "조사 중..." UI     (태그: InspectingUI)
    [SerializeField] protected Vector3 uiOffset = new Vector3(0f, 1.5f, 0f);

    [Header("조사 설정")]
    [SerializeField] private float inspectDuration = 2f;

    private bool playerInRange = false;
    private bool isInspecting = false;
    private Coroutine inspectCoroutine;

    protected bool IsShowingUI = false;
    // ── 초기화 ────────────────────────────────────────────
    protected virtual void Start()
    {
        if (inspectPromptUI == null)
            inspectPromptUI = GameObject.FindWithTag("InspectPromptUI");
        if (inspectingUI == null)
            inspectingUI = GameObject.FindWithTag("InspectingUI");

        HideAllUI();
    }

    // ── 매 프레임 ─────────────────────────────────────────
    protected virtual void Update()
    {
        if (IsShowingUI)
        {
            if (InputManager.anyKeyPressed)
                OnAnyKeyWhileUI();
            return; // 조사 입력 차단
        }

        CheckPlayerRange();
        HandleUI();
        HandleInput();
    }

    // ── 범위 감지 (DoorObject와 동일 방식) ────────────────
    private void CheckPlayerRange()
    {
        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
        Collider2D hit = Physics2D.OverlapCircle(center, interactRadius, playerLayer);
        playerInRange = (hit != null);
    }

    // ── UI 갱신 ───────────────────────────────────────────
    private void HandleUI()
    {
        if (isInspecting) return; // 조사 중에는 코루틴이 UI를 직접 관리

        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
        Vector3 uiPos = center + uiOffset;

        if (inspectPromptUI != null)
        {
            inspectPromptUI.transform.position = uiPos;
            inspectPromptUI.SetActive(playerInRange);
        }
        if (inspectingUI != null)
        {
            inspectingUI.SetActive(false);
        }
    }

    // ── 입력 처리 ─────────────────────────────────────────
    private void HandleInput()
    {
        if (playerInRange && !isInspecting && InputManager.getPressed)
        {

            inspectCoroutine = StartCoroutine(InspectRoutine());
        }
    }

    // ── 조사 코루틴 ───────────────────────────────────────
    private IEnumerator InspectRoutine()
    {
        isInspecting = true;

        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
        Vector3 uiPos = center + uiOffset;

        inspectPromptUI?.SetActive(false);

        if (inspectingUI != null)
        {
            inspectingUI.transform.position = uiPos;
            inspectingUI.SetActive(true);
        }

        yield return new WaitForSeconds(inspectDuration);

        inspectingUI?.SetActive(false);

        OnInspectComplete();

        isInspecting = false;
        inspectCoroutine = null;
    }

    // ── 자식 클래스에서 반드시 구현 ───────────────────────
    /// <summary>조사 완료 시 호출. 자식 클래스에서 결과 로직을 구현하세요.</summary>
    protected virtual void OnInspectComplete() { 
        
    }

    // ── 유틸리티 ──────────────────────────────────────────
    private void HideAllUI()
    {
        inspectPromptUI?.SetActive(false);
        inspectingUI?.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, interactRadius);
    }
    protected virtual void OnAnyKeyWhileUI() { }

}