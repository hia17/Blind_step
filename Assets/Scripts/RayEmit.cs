using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EchoEmit : MonoBehaviour
{
    [Header("Ray Angles")]
    [Tooltip("마우스 방향 기준 양쪽으로 퍼지는 각도 (총 범위 = halfAngle * 2)")]
    [SerializeField] private float halfAngle = 15f;

    [Header("Ray Distance & Speed")]
    [Tooltip("레이가 뻗어나가는 최대 거리")]
    [SerializeField] private float maxDistance = 10f;
    [Tooltip("레이가 뻗어나가는 속도 (유닛/초)")]
    [SerializeField] private float raySpeed = 15f;

    [Header("Visual")]
    [Tooltip("레이 선의 두께")]
    [SerializeField] private float lineWidth = 0.05f;
    [Tooltip("레이 선의 색상")]
    [SerializeField] private Color rayColor = Color.white;
    [Tooltip("최대 거리 도달 후 사라지기까지의 시간")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Detect")]
    [Tooltip("레이에 감지될 레이어")]
    [SerializeField] private LayerMask detectLayer;

    // 현재 실행 중인 레이 코루틴
    private Coroutine echoCoroutine;

    // 활성 LineRenderer 목록
    private List<LineRenderer> activeLines = new List<LineRenderer>();

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (InputManager.usePressed)
        {
            TriggerEcho();
        }
    }

    private void TriggerEcho()
    {
        // 이전 에코가 실행 중이면 즉시 중단하고 초기화
        if (echoCoroutine != null)
        {
            StopCoroutine(echoCoroutine);
            ClearAllLines();
        }

        echoCoroutine = StartCoroutine(FireRays());
    }

    private IEnumerator FireRays()
    {
        // 마우스 방향 계산
        Vector2 mouseWorld = InputManager.mouseWorldPos;
        Vector2 origin = transform.position;
        Vector2 toMouse = (mouseWorld - origin).normalized;
        float baseAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;

        // 3개의 레이 각도: -halfAngle, 0, +halfAngle
        float[] angles = new float[]
        {
            baseAngle - halfAngle,
            baseAngle,
            baseAngle + halfAngle
        };

        // 각 레이마다 LineRenderer 생성
        LineRenderer[] lines = new LineRenderer[angles.Length];
        for (int i = 0; i < angles.Length; i++)
        {
            lines[i] = CreateLineRenderer();
            lines[i].positionCount = 2;
            lines[i].SetPosition(0, new Vector3(origin.x, origin.y, -1f));
            lines[i].SetPosition(1, new Vector3(origin.x, origin.y, -1f)); // 아직 길이 0
            activeLines.Add(lines[i]);
        }

        // 레이를 속도에 맞춰 뻗어나가게 함
        float currentDistance = 0f;
        bool[] stopped = new bool[angles.Length]; // 각 레이가 벽에 막혔는지

        while (currentDistance < maxDistance)
        {
            currentDistance += raySpeed * Time.deltaTime;
            currentDistance = Mathf.Min(currentDistance, maxDistance);

            for (int i = 0; i < angles.Length; i++)
            {
                if (stopped[i]) continue;

                float rad = angles[i] * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                // 현재 거리까지 레이캐스트
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, currentDistance, detectLayer);

                Vector2 endPoint;
                if (hit.collider != null)
                {
                    endPoint = hit.point;
                    stopped[i] = true; // 벽에 막히면 해당 레이 정지
                }
                else
                {
                    endPoint = origin + dir * currentDistance;
                }

                lines[i].SetPosition(1, new Vector3(endPoint.x, endPoint.y, -1f));
            }

            yield return null;
        }

        // 최대 거리 도달 후 페이드아웃
        yield return StartCoroutine(FadeOutLines(lines, fadeDuration));

        ClearAllLines();
        echoCoroutine = null;
    }

    private IEnumerator FadeOutLines(LineRenderer[] lines, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            Color c = new Color(rayColor.r, rayColor.g, rayColor.b, alpha);

            foreach (var lr in lines)
            {
                if (lr == null) continue;
                lr.startColor = c;
                lr.endColor = c;
            }

            yield return null;
        }
    }

    private LineRenderer CreateLineRenderer()
    {
        GameObject obj = new GameObject("EchoRay");
        obj.transform.SetParent(this.transform);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = rayColor;
        lr.endColor = rayColor;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.sortingOrder = 10;

        return lr;
    }

    private void ClearAllLines()
    {
        foreach (var lr in activeLines)
        {
            if (lr != null)
                Destroy(lr.gameObject);
        }
        activeLines.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector2 mouseWorld = InputManager.mouseWorldPos;
        Vector2 origin = transform.position;
        Vector2 toMouse = (mouseWorld - origin);
        if (toMouse == Vector2.zero) return;

        float baseAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;
        float[] angles = { baseAngle - halfAngle, baseAngle, baseAngle + halfAngle };

        Gizmos.color = Color.white;
        foreach (float a in angles)
        {
            float rad = a * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Gizmos.DrawLine(origin, origin + dir * maxDistance);
        }
    }

    private void OnDestroy()
    {
        ClearAllLines();
    }
}