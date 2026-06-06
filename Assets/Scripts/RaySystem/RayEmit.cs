using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RayEmit : MonoBehaviour
{
    [Header("Ray Angles & Ray Counts")]
    [Tooltip("마우스 방향 기준 양쪽으로 퍼지는 각도 (총 범위 = halfAngle * 2)")]
    [SerializeField] private float halfAngle = 15f;
    [SerializeField] private float rayCount = 3; 

    [Header("Ray Distance & Speed")]
    [Tooltip("레이가 뻗어나가는 최대 거리")]
    [SerializeField] private float maxDistance = 10f;
    [Tooltip("레이가 뻗어나가는 속도 (유닛/초)")]
    [SerializeField] private float raySpeed = 15f;

    [Header("Ray Shape")]
    [Tooltip("0 = 직선, 1 = 파형")]
    [SerializeField] private int rayShape = 1;

    [Header("Wave Shape")]
    [Tooltip("사인파 높이 (위아래 진폭)")]
    [SerializeField] private float waveHeight = 0.15f;
    [Tooltip("사인파 한 주기의 길이 (짧을수록 촘촘)")]
    [SerializeField] private float waveLength = 0.6f;
    [Tooltip("포인트 밀도 (높을수록 부드러움)")]
    [SerializeField] private int pointsPerUnit = 20;

    [Header("Tail")]
    [Tooltip("꼬리 길이 (유닛). 앞이 이만큼 앞서나가면 뒤가 잘림)")]
    [SerializeField] private float tailLength = 3f;

    [Header("Visual")]
    [Tooltip("레이 선의 두께")]
    [SerializeField] private float lineWidth = 0.04f;
    [Tooltip("레이 선의 색상")]
    [SerializeField] private Color rayColor = Color.white;
    [Tooltip("최대 거리 도달 후 전체 사라지기까지의 시간")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Ray Delay")]
    [Tooltip("레이 발사 간격 (초). 0이면 즉시 발사")]
    [SerializeField] private float rayDelay = 2f;

    private float lastFireTime = -999f; // 마지막 발사 시각
    private bool hitDetected = false;   // 충돌 감지 여부

    [Header("Detect")]
    [Tooltip("레이에 감지될 레이어")]
    [SerializeField] private LayerMask detectLayer;

    private Coroutine echoCoroutine;
    private List<LineRenderer> activeLines = new List<LineRenderer>();
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (InputManager.detectPressed)
            TriggerEcho();
    }

    private void TriggerEcho()
    {
        // 딜레이 체크 — 충돌 감지됐으면 딜레이 무시
        bool delayPassed = (Time.time - lastFireTime) >= rayDelay;

        if (!delayPassed && !hitDetected)
            return; // 딜레이 안 지났고 충돌도 없으면 발사 불가

        if (echoCoroutine != null)
        {
            return;
            //ClearAllLines();
            //StopAllCoroutines();
        }

        lastFireTime = Time.time;
        hitDetected = false; // 초기화
        echoCoroutine = StartCoroutine(FireRays());
    }

    private IEnumerator FireRays()
    {
        // 발사 시점 origin 고정 (캐릭터가 움직여도 이 값은 변하지 않음)
        Vector2 fixedOrigin = transform.position;

        Vector2 mouseWorld = InputManager.mouseWorldPos;
        Vector2 toMouse = (mouseWorld - fixedOrigin).normalized;
        float baseAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;

        //float[] angles = new float[]
        //{
        //    baseAngle - halfAngle,
        //    baseAngle,
        //    baseAngle + halfAngle
        //};

        float[] angles = new float[(int)rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            float t = (rayCount == 1) ? 0.5f : (float)i / (rayCount - 1);

            float angleOffset = Mathf.Lerp(-halfAngle, halfAngle, t);

            angles[i] = baseAngle + angleOffset;
        }
        // 각 레이 방향 벡터 미리 계산
        Vector2[] forwards = new Vector2[angles.Length];
        Vector2[] perps = new Vector2[angles.Length];
        for (int i = 0; i < angles.Length; i++)
        {
            float rad = angles[i] * Mathf.Deg2Rad;
            forwards[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            perps[i] = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        }

        // LineRenderer 생성
        LineRenderer[] lines = new LineRenderer[angles.Length];
        for (int i = 0; i < angles.Length; i++)
        {
            lines[i] = CreateLineRenderer();
            activeLines.Add(lines[i]);
        }

        float[] stoppedAt = new float[angles.Length];
        bool[] stopped = new bool[angles.Length];
        for (int i = 0; i < stoppedAt.Length; i++) stoppedAt[i] = maxDistance;

        // --- 뻗어나가는 단계 ---
        float headDist = 0f; // 앞부분(head)이 나간 거리

        while (headDist < maxDistance)
        {
            headDist += raySpeed * Time.deltaTime;
            headDist = Mathf.Min(headDist, maxDistance);

            // 꼬리 시작 거리 (headDist - tailLength, 0 이상)
            float tailStart = Mathf.Max(0f, headDist - tailLength);

            for (int i = 0; i < angles.Length; i++)
            {
                // 벽 감지
                if (!stopped[i])
                {
                    RaycastHit2D hit = Physics2D.Raycast(fixedOrigin, forwards[i], headDist, detectLayer);
                    if (hit.collider != null)
                    {
                        stopped[i] = true;
                        stoppedAt[i] = Vector2.Distance(fixedOrigin, hit.point);
                        hitDetected = true;

                        // 충돌 오브젝트에 EchoHitOutline이 있으면 외곽선 표시 요청
                        EchoHitOutline outline = hit.collider.GetComponent<EchoHitOutline>();
                        if (outline != null)
                            outline.OnRayHit(hit.point, hit.normal);
                    }
                }

                float drawHead = stopped[i] ? stoppedAt[i] : headDist;
                float drawTail = stopped[i] ? Mathf.Min(tailStart, stoppedAt[i]) : tailStart;

                DrawWaveSegment(lines[i], fixedOrigin, forwards[i], perps[i], drawTail, drawHead, 1f);
            }

            yield return null;
        }

        // --- 꼬리가 끝까지 밀려나가는 단계 ---
        float tailDist = Mathf.Max(0f, maxDistance - tailLength);

        while (tailDist < maxDistance)
        {
            tailDist += raySpeed * Time.deltaTime;
            tailDist = Mathf.Min(tailDist, maxDistance);

            for (int i = 0; i < angles.Length; i++)
            {
                float drawTail = Mathf.Min(tailDist, stoppedAt[i]);
                float drawHead = stoppedAt[i];

                // 꼬리가 head를 넘으면 선 숨김
                if (drawTail >= drawHead)
                {
                    lines[i].positionCount = 0;
                    continue;
                }

                DrawWaveSegment(lines[i], fixedOrigin, forwards[i], perps[i], drawTail, drawHead, 1f);
            }

            yield return null;
        }

        // --- 전체 페이드아웃 (이미 선이 사라진 경우 즉시 종료) ---
        bool anyVisible = false;
        foreach (var lr in lines)
            if (lr != null && lr.positionCount > 0) anyVisible = true;

        if (anyVisible)
            yield return StartCoroutine(FadeOutAll(lines, forwards, perps, fixedOrigin, stoppedAt, fadeDuration));

        ClearAllLines();
        echoCoroutine = null;
    }

    // tailDist ~ headDist 구간만 사인파로 그림
    private void DrawWaveSegment(
        LineRenderer lr,
        Vector2 origin, Vector2 forward, Vector2 perp,
        float tailDist, float headDist,
        float globalAlpha)
    {
        float segLen = headDist - tailDist;
        if (segLen <= 0f)
        {
            lr.positionCount = 0;
            return;
        }

        int totalPoints = Mathf.Max(2, Mathf.CeilToInt(segLen * pointsPerUnit));
        lr.positionCount = totalPoints;

        for (int p = 0; p < totalPoints; p++)
        {
            float t = (float)p / (totalPoints - 1);
            float dist = Mathf.Lerp(tailDist, headDist, t);


            // 사인파 offset
            //float offset = Mathf.Sin((dist / waveLength) * Mathf.PI * 2f) * waveHeight;
            float offset = (rayShape == 1)
            ? Mathf.Sin((dist / waveLength) * Mathf.PI * 2f) * waveHeight
            : 0f;


            Vector2 pos = origin + forward * dist + perp * offset;
            lr.SetPosition(p, new Vector3(pos.x, pos.y, -1f));
        }

        // 꼬리(t=0) 투명 → 머리(t=1) 불투명 그라디언트
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(rayColor, 0f),
                new GradientColorKey(rayColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f,           0f),
                new GradientAlphaKey(globalAlpha,  0.3f),
                new GradientAlphaKey(globalAlpha,  1f)
            }
        );
        lr.colorGradient = gradient;
    }

    private IEnumerator FadeOutAll(
        LineRenderer[] lines,
        Vector2[] forwards, Vector2[] perps,
        Vector2 fixedOrigin,
        float[] stoppedAt,
        float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == null) continue;
                DrawWaveSegment(lines[i], fixedOrigin, forwards[i], perps[i], 0f, stoppedAt[i], alpha);
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
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.sortingOrder = 10;

        return lr;
    }

    private void ClearAllLines()
    {
        foreach (var lr in activeLines)
            if (lr != null) Destroy(lr.gameObject);
        activeLines.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector2 mouseWorld = InputManager.mouseWorldPos;
        Vector2 origin = transform.position;
        Vector2 toMouse = mouseWorld - origin;
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

    private void OnDestroy() => ClearAllLines();
}