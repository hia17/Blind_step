using UnityEngine;

public class RailObstacle : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints; // 다각형 꼭짓점들
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool loop = true; // 도착 후 처음으로 돌아갈지

    private int targetIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[targetIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // 목표 지점에 도달했는지 체크
        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            targetIndex = (targetIndex + 1) % waypoints.Length; // 다음 지점으로
        }
    }
}