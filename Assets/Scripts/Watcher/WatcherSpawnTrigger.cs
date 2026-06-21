using UnityEngine;

public class WatcherSpawnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject watcherPrefab;
    [SerializeField] private Transform spawnPoint;

    private bool _isActive = false;

    public void Activate() => _isActive = true;
    public void Deactivate() => _isActive = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isActive) return;
        if (!collision.CompareTag("Player")) return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Instantiate(watcherPrefab, pos, Quaternion.identity);

        Deactivate(); // ★ 소환 후 비활성화 (중복 소환 방지)
        //GameManager.Instance.OnWatcherTriggered(); // ★ 오늘 소환 완료 알림
    }
}