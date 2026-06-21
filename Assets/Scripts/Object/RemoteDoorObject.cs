using System.Collections;
using UnityEngine;

/// <summary>
/// 발판 트리거로 제어되는 문.
/// ObjectTrigger를 상속하므로 플레이어가 가까이서 조사(G키)하면
/// "다른 곳에서 열 수 있는 문입니다." UI가 표시됩니다.
/// WeightPlatformTrigger가 OpenDoor() / CloseDoor()를 호출해 doorPivot을 60도 회전시킵니다.
/// </summary>
public class RemoteDoorObject : ObjectTrigger
{
    [Header("원격 문 UI")]
    [Tooltip("'다른 곳에서 열 수 있는 문' 설명 UI")]
    [SerializeField] private GameObject remoteOpenInfoUI;
    [SerializeField] private float remoteInfoDuration = 2.5f;

    [Header("문 회전 설정")]
    [SerializeField] private Transform doorPivot;   // 회전 기준점
    [SerializeField] private float openAngleDelta = 60f; // 열릴 때 추가 회전각

    private float _baseAngle;
    private float _openAngle;

    public bool IsOpen { get; private set; } = false;

    private Coroutine _infoUICoroutine;
    protected override bool CanInteract()
    {
        return !IsOpen;
    }
    protected override void Start()
    {
        base.Start();

        if (remoteOpenInfoUI != null)
        {
            remoteOpenInfoUI.transform.rotation = Quaternion.identity;
            remoteOpenInfoUI.SetActive(false);
        }

        // 현재 배치 각도를 기준으로 열림 각도 계산
        Transform target = doorPivot != null ? doorPivot : transform;
        _baseAngle = target.localEulerAngles.z;
        _openAngle = _baseAngle + openAngleDelta;
    }

    // ─────────────────── ObjectTrigger 조사 완료 ───────────────────────
    protected override void OnInspectComplete()
    {
        IsShowingUI = true;

        if (_infoUICoroutine != null)
        {
            StopCoroutine(_infoUICoroutine);
            _infoUICoroutine = null;
        }

        if (remoteOpenInfoUI != null)
            _infoUICoroutine = StartCoroutine(ShowRemoteInfoUI());
    }

    protected override void OnAnyKeyWhileUI()
    {
        IsShowingUI = false;

        if (_infoUICoroutine != null)
        {
            StopCoroutine(_infoUICoroutine);
            _infoUICoroutine = null;
        }

        if (remoteOpenInfoUI != null) remoteOpenInfoUI.SetActive(false);
    }

    // ─────────────────── 발판에서 호출하는 문 제어 ─────────────────────
    public void OpenDoor()
    {
        if (IsOpen) return;
        IsOpen = true;
        ApplyDoorAngle();
    }

    public void CloseDoor()
    {
        if (!IsOpen) return;
        IsOpen = false;
        ApplyDoorAngle();
    }

    // ─────────────────── 내부 유틸 ─────────────────────────────────────
    private void ApplyDoorAngle()
    {
        float targetAngle = IsOpen ? _openAngle : _baseAngle;
        Transform target = doorPivot != null ? doorPivot : transform;
        target.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    private IEnumerator ShowRemoteInfoUI()
    {
        Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
        remoteOpenInfoUI.transform.position = center + uiOffset;
        remoteOpenInfoUI.SetActive(true);

        yield return new WaitForSeconds(remoteInfoDuration);

        remoteOpenInfoUI.SetActive(false);
        IsShowingUI = false;
        _infoUICoroutine = null;
    }
}