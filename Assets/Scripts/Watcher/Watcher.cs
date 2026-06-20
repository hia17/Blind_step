using System.Collections;
using UnityEngine;

public class Watcher : MonoBehaviour
{
    [SerializeField] private float timeLimit = 10f;

    private RoomTrigger[] _rooms;

    private void Start()
    {
        BGMManager.Instance.PlayWatcherBGM();

        _rooms = FindObjectsByType<RoomTrigger>(FindObjectsSortMode.None);
        foreach (RoomTrigger room in _rooms)
            room.Activate();

        StartCoroutine(TimeLimitRoutine());
        StartCoroutine(LogTimeLimitRoutine());
    }

    private IEnumerator TimeLimitRoutine()
    {
        yield return new WaitForSeconds(timeLimit);

        // 이미 생존한 방이 있으면 BGM만 복구하고 종료
        foreach (RoomTrigger room in _rooms)
        {
            if (room.IsSucceeded)
            {
                BGMManager.Instance.PlayNormalBGM();
                Destroy(gameObject);
                yield break;
            }
        }

        // 안전 타이머 진행 중인 방이 있으면 결과 나올 때까지 대기
        bool hasSafeTimer = false;
        foreach (RoomTrigger room in _rooms)
        {
            if (room.IsSafe)
            {
                hasSafeTimer = true;
                break;
            }
        }

        if (hasSafeTimer)
        {
            Debug.Log("[Watcher] 제한시간 종료 - 안전 타이머 진행 중, 계속 대기");

            // 생존 또는 안전 타이머가 끊길 때까지 대기
            yield return new WaitUntil(() =>
            {
                foreach (RoomTrigger room in _rooms)
                    if (room.IsSucceeded) return true;

                // 안전한 방이 하나도 없으면 실패
                bool anyStillSafe = false;
                foreach (RoomTrigger room in _rooms)
                    if (room.IsSafe) { anyStillSafe = true; break; }

                return !anyStillSafe;
            });

            // 결과 판정
            bool survived = false;
            foreach (RoomTrigger room in _rooms)
                if (room.IsSucceeded) { survived = true; break; }

            if (survived)
                Debug.Log("[Watcher] 생존!");
            else
            {
                Debug.Log("[Watcher] 사망 - 안전 타이머가 끊겼습니다.");
                foreach (RoomTrigger room in _rooms)
                    room.Deactivate();
                // TODO: 사망 처리
                GameManager.Instance.PassDay();
            }
        }
        else
        {
            Debug.Log("[Watcher] 사망 - 제한시간 내에 밀폐에 실패했습니다.");
            foreach (RoomTrigger room in _rooms)
                room.Deactivate();
            // TODO: 사망 처리
            GameManager.Instance.PassDay();
        }

        BGMManager.Instance.PlayNormalBGM();
        Destroy(gameObject);
    }

    private IEnumerator LogTimeLimitRoutine()
    {
        float elapsed = 0f;
        while (elapsed < timeLimit)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
            Debug.Log($"[Watcher] 제한시간 남은 시간: {timeLimit - elapsed:F0}초");
        }
    }
    public void StopWatcher()
    {
        StopAllCoroutines();
        BGMManager.Instance.PlayNormalBGM();
        foreach (RoomTrigger room in _rooms)
            room.Deactivate();
        Destroy(gameObject);
    }
}