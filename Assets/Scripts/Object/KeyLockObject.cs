using UnityEngine;

/// <summary>
/// 열쇠로 잠금 해제하는 오브젝트.
/// ObjectTrigger를 상속 → 범위 감지 + 2초 조사 후 열쇠 매칭 확인.
/// 맞는 열쇠가 인벤토리에 있으면 소모 후 잠금 해제, 없으면 아무것도 안 함.
/// 잠금 해제 후 추가 동작은 자식 클래스에서 OnUnlocked()를 override해서 구현.
/// </summary>
public class KeyLockObject : ObjectTrigger
{
    [Header("열쇠 설정")]
    [SerializeField] private string requiredKeyID;

    [Header("잠금 해제 UI")]
    [SerializeField] private GameObject noKeyUI;      // 열쇠 없을 때 잠깐 표시
    [SerializeField] private float noKeyUIDuration = 1.5f;

    [Header("문 교체")]
    [SerializeField] private Collider2D lockedDoorCollider;
    [SerializeField] private GameObject unlockedDoorPrefab;
    [SerializeField] private Transform doorSpawnPoint;

    private bool isUnlocked = false;

    protected override void Start()
    {
        base.Start();
        if (noKeyUI != null)
            noKeyUI.SetActive(false);
    }
    protected override void Update()
    {
        // Update에서 isUnlocked 체크 위에 추가
        if (lockedDoorCollider != null && !lockedDoorCollider.enabled) return;

        if (isUnlocked) return;
        base.Update();
    }

    protected override void OnInspectComplete()
    {
        IsShowingUI = true;
        // 인벤토리에서 맞는 열쇠 탐색
        var items = Inventory.Instance.Items;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemType == ItemData.ItemType.key && items[i].keyID == requiredKeyID)
            {
                Inventory.Instance.RemoveItem(i);
                isUnlocked = true;
                SwapDoor();
                return;
            }
        }

        // 맞는 열쇠 없음
        if (noKeyUI != null)
            StartCoroutine(ShowNoKeyUI());
    }

    protected virtual void OnUnlocked()
    {
        Debug.Log($"[{requiredKeyID}] 잠금 해제!");
        // 자식 클래스에서 문 열기, 이벤트 발생 등 구현
    }

    private System.Collections.IEnumerator ShowNoKeyUI()
    {
        if (noKeyUI != null)
        {
            Vector3 center = detectionCenter != null ? detectionCenter.position : transform.position;
            Vector3 uiPos = center + uiOffset;
            noKeyUI.transform.position = uiPos;
            noKeyUI.SetActive(true);
            yield return new WaitForSeconds(noKeyUIDuration);
            noKeyUI.SetActive(false);
            IsShowingUI = false;
        }
    }
    protected override void OnAnyKeyWhileUI()
    {
        IsShowingUI = false;
        noKeyUI?.SetActive(false);
    }
    private void SwapDoor()
    {
        if (lockedDoorCollider != null)
            lockedDoorCollider.enabled = false;

        if (unlockedDoorPrefab != null)
        {
            Vector3 spawnPos = doorSpawnPoint != null ? doorSpawnPoint.position : transform.position;
            Quaternion spawnRot = doorSpawnPoint != null ? doorSpawnPoint.rotation : Quaternion.identity;
            Instantiate(unlockedDoorPrefab, spawnPos, spawnRot);
        }
    }
}