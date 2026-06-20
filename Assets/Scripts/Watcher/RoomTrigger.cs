using UnityEngine;
using UnityEngine.Events;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private DoorObject[] doors;
    [SerializeField] private float requiredSafeTime = 5f;

    public bool IsPlayerInside { get; private set; }

    private float _safeStartTime = -1f;
    private bool _survived = false;
    private bool _isActive = false;
    private int _lastLoggedSecond = -1;

    public UnityEvent onSurvived;

    public bool IsSafe
    {
        get
        {
            if (!IsPlayerInside) return false;
            if (doors == null || doors.Length == 0) return false;
            foreach (DoorObject door in doors)
            {
                if (door == null || door.IsOpen) return false;
            }
            return true;
        }
    }

    public void Activate()
    {
        _survived = false;
        _safeStartTime = -1f;
        _lastLoggedSecond = -1;
        _isActive = true;
    }

    public void Deactivate()
    {
        _isActive = false;
        _safeStartTime = -1f;
        _lastLoggedSecond = -1;
    }

    public bool IsSucceeded => _survived;

    private void Update()
    {
        if (!_isActive || _survived) return;

        if (IsSafe)
        {
            if (_safeStartTime < 0f)
            {
                _safeStartTime = Time.time;
                _lastLoggedSecond = -1;
                Debug.Log("[RoomTrigger] 안전 타이머 시작");
            }

            float elapsed = Time.time - _safeStartTime;
            float remaining = requiredSafeTime - elapsed;
            int currentSecond = Mathf.CeilToInt(remaining);

            if (currentSecond != _lastLoggedSecond)
            {
                _lastLoggedSecond = currentSecond;
                Debug.Log($"[RoomTrigger] 안전 유지 남은 시간: {Mathf.Max(currentSecond, 0)}초");
            }

            if (elapsed >= requiredSafeTime)
            {
                _survived = true;
                _isActive = false;
                Debug.Log("[RoomTrigger] 생존!");
                BGMManager.Instance.PlayNormalBGM();
                onSurvived?.Invoke();
            }
        }
        else
        {
            if (_safeStartTime >= 0f)
                Debug.Log("[RoomTrigger] 조건 깨짐 - 안전 타이머 리셋");

            _safeStartTime = -1f;
            _lastLoggedSecond = -1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            IsPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IsPlayerInside = false;
            _safeStartTime = -1f;
            _lastLoggedSecond = -1;
        }
    }
}