using System.Collections;
using UnityEngine;

/// <summary>
/// 무게 감지 발판 트리거.
/// "Player" 태그 또는 "HeavyObject" 태그를 가진 오브젝트가 트리거에 들어오면
/// 문 열림 소리 재생 + 활성 UI 표시, 나가면 문 닫힘 소리 + 닫힘 알림 UI 표시.
/// RemoteDoorObject를 doorTarget에 연결하면 문 상태를 제어합니다.
/// </summary>
public class WeightPlatformTrigger : MonoBehaviour
{
    [Header("감지 설정")]
    [Tooltip("감지할 태그 목록 (예: Player, HeavyObject)")]
    [SerializeField] private string[] detectedTags = { "Player", "HeavyObject" };

    [Header("연결된 문")]
    [SerializeField] private RemoteDoorObject doorTarget;

    [Header("사운드 볼륨")]
    [SerializeField] private float soundVolume = 1f;

    [Header("UI")]
    [SerializeField] private GameObject activeUI;        // 발판 눌렸을 때 표시 UI
    [SerializeField] private GameObject doorClosedUI;    // 문 닫힐 때 잠깐 표시 UI
    [SerializeField] private float doorClosedUIDuration = 2f;

    // 현재 트리거 안에 있는 유효 오브젝트 수
    private int _occupantCount = 0;
    private Coroutine _closedUICoroutine;

    private void Start()
    {
        if (activeUI != null)
        {
            activeUI.SetActive(false);
            activeUI.transform.rotation = Quaternion.identity;
        }
        if (doorClosedUI != null)
        {
            doorClosedUI.SetActive(false);
            doorClosedUI.transform.rotation = Quaternion.identity;
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (!IsDetected(other)) return;

        _occupantCount++;
        Debug.Log($"Enter : {other.name} / Count = {_occupantCount}");
        // 처음 올라왔을 때만 문 열기
        if (_occupantCount == 1)
            OnPlatformActivated();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        
        if (!IsDetected(other)) return;

        _occupantCount = Mathf.Max(0, _occupantCount - 1);
        Debug.Log($"Exit : {other.name} / Count = {_occupantCount}");
        // 발판이 완전히 비었을 때 문 닫기
        if (_occupantCount == 0)
            OnPlatformDeactivated();
    }

    // ───────────────────────────── 활성화 ─────────────────────────────
    private void OnPlatformActivated()
    {
        // 닫힘 UI 코루틴이 남아있으면 중단
        if (_closedUICoroutine != null)
        {
            StopCoroutine(_closedUICoroutine);
            _closedUICoroutine = null;
        }
        if (doorClosedUI != null) doorClosedUI.SetActive(false);

        // 소리
        SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.SFX.door, transform, soundVolume);

        // UI
        if (activeUI != null) activeUI.SetActive(true);

        // 문 열기
        if (doorTarget != null) doorTarget.OpenDoor();
    }

    // ───────────────────────────── 비활성화 ────────────────────────────
    private void OnPlatformDeactivated()
    {
        // 활성 UI 끄기
        if (activeUI != null) activeUI.SetActive(false);

        // 소리
        SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.SFX.door, transform, soundVolume);

        // 문 닫기
        if (doorTarget != null) doorTarget.CloseDoor();

        // 닫힘 알림 UI 잠깐 표시
        if (doorClosedUI != null)
            _closedUICoroutine = StartCoroutine(ShowClosedUI());
    }

    // ───────────────────────────── 유틸 ───────────────────────────────
    private bool IsDetected(Collider2D other)
    {
        foreach (string tag in detectedTags)
        {
            if (other.CompareTag(tag)) return true;
        }
        return false;
    }

    private IEnumerator ShowClosedUI()
    {
        doorClosedUI.SetActive(true);
        yield return new WaitForSeconds(doorClosedUIDuration);
        doorClosedUI.SetActive(false);
        _closedUICoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}