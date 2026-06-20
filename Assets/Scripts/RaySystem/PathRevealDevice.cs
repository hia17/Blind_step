using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 발판 위에서 키를 누르면 길 A(긴 길)와 길 B(짧은 길+장애물)의
/// 외곽선이 지정된 시작 지점부터 쭉 퍼져나가듯 표시됩니다.
///
/// [사용법]
/// 1. 이 컴포넌트를 발판 오브젝트에 부착합니다.
/// 2. 인스펙터에서 Path A / Path B 콜라이더, 시작 Transform, 장애물 목록을 설정합니다.
/// 3. 플레이어가 발판 위에 있을 때 activateKey를 누르면 작동합니다.
/// </summary>
public class PathRevealDevice : MonoBehaviour
{
    // ─────────────────────────────────────────────
    #region Inspector Fields

    [Header("발동 설정")]
    [SerializeField] private KeyCode activateKey = KeyCode.F;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionRadius = 1.5f;

    [Header("길 A (긴 길)")]
    [Tooltip("길 A를 구성하는 콜라이더 목록 (좌/우 라인 각각 넣어주세요)")]
    [SerializeField] private Collider2D[] pathA_Colliders;
    [Tooltip("길 A 외곽선이 시작될 Transform (없으면 콜라이더 중심)")]
    [SerializeField] private Transform pathA_StartPoint;

    [Header("길 B (짧은 길 + 장애물)")]
    [Tooltip("길 B를 구성하는 콜라이더 목록 (좌/우 라인 각각 넣어주세요)")]
    [SerializeField] private Collider2D[] pathB_Colliders;
    [Tooltip("길 B 외곽선이 시작될 Transform (없으면 콜라이더 중심)")]
    [SerializeField] private Transform pathB_StartPoint;
    [Tooltip("길 B와 함께 드러날 장애물 콜라이더 목록")]
    [SerializeField] private Collider2D[] pathB_Obstacles;

    [Header("외곽선 비주얼")]
    [SerializeField] private Color pathA_Color = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private Color pathB_Color = new Color(1f, 0.6f, 0.1f, 1f);
    [SerializeField] private Color obstacle_Color = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private int sampleCount = 60;

    [Header("퍼져나가기 타이밍")]
    [Tooltip("외곽선이 끝까지 퍼져나가는 데 걸리는 시간 (초)")]
    [SerializeField] private float revealDuration = 1.2f;
    [Tooltip("완전히 표시된 뒤 유지 시간 (초)")]
    [SerializeField] private float holdDuration = 3f;
    [Tooltip("사라지는 데 걸리는 시간 (초)")]
    [SerializeField] private float fadeDuration = 0.8f;
    [Tooltip("길 A와 길 B를 동시에 시작할지 여부")]
    [SerializeField] private bool revealSimultaneously = true;

    #endregion
    // ─────────────────────────────────────────────

    private bool playerInRange = false;
    private bool isRevealing = false;
    private readonly List<LineRenderer> activeLines = new List<LineRenderer>();

    // ─────────────────────────────────────────────
    #region Unity Lifecycle

    private void Update()
    {
        CheckPlayerRange();

        if (playerInRange && !isRevealing && InputManager.getPressed)
            StartCoroutine(RevealRoutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        ClearLines();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (pathA_StartPoint != null)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f);
            Gizmos.DrawSphere(pathA_StartPoint.position, 0.15f);
        }
        if (pathB_StartPoint != null)
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f);
            Gizmos.DrawSphere(pathB_StartPoint.position, 0.15f);
        }
    }

    #endregion
    // ─────────────────────────────────────────────
    #region Range Check

    private void CheckPlayerRange()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        playerInRange = (hit != null);
    }

    #endregion
    // ─────────────────────────────────────────────
    #region Main Reveal Routine

    private IEnumerator RevealRoutine()
    {
        isRevealing = true;
        ClearLines();

        if (revealSimultaneously)
        {
            // 길 A와 B를 동시에 시작
            StartCoroutine(RevealPathGroup(pathA_Colliders, pathA_StartPoint, pathA_Color));
            StartCoroutine(RevealPathGroup(pathB_Colliders, pathB_StartPoint, pathB_Color));
            StartCoroutine(RevealObstacles(pathB_Obstacles, pathB_StartPoint));
        }
        else
        {
            // 길 A 먼저, 이후 길 B
            yield return StartCoroutine(RevealPathGroup(pathA_Colliders, pathA_StartPoint, pathA_Color));
            StartCoroutine(RevealPathGroup(pathB_Colliders, pathB_StartPoint, pathB_Color));
            StartCoroutine(RevealObstacles(pathB_Obstacles, pathB_StartPoint));
        }

        // holdDuration + fadeDuration 대기 후 정리
        yield return new WaitForSeconds(holdDuration + fadeDuration + 0.1f);

        ClearLines();
        isRevealing = false;
    }

    #endregion
    // ─────────────────────────────────────────────
    #region Path Reveal

    /// <summary>콜라이더 그룹의 외곽선을 시작점부터 퍼져나가게 표시합니다.</summary>
    private IEnumerator RevealPathGroup(Collider2D[] colliders, Transform startPoint, Color color)
    {
        if (colliders == null || colliders.Length == 0) yield break;

        foreach (var col in colliders)
        {
            if (col == null) continue;
            StartCoroutine(RevealSingleCollider(col, startPoint, color));
        }

        yield return new WaitForSeconds(revealDuration + holdDuration + fadeDuration);
    }

    private IEnumerator RevealSingleCollider(Collider2D col, Transform startPoint, Color color)
    {
        // 외곽선 포인트 샘플링
        List<Vector2> boundary = GetBoundaryPoints(col);
        if (boundary == null || boundary.Count < 2) yield break;

        // 누적 거리 계산
        List<float> cumul = BuildCumulative(boundary);
        float totalLength = cumul[cumul.Count - 1];

        // 시작 지점에서 가장 가까운 경계 위의 t값 찾기
        Vector2 origin = startPoint != null ? (Vector2)startPoint.position : (Vector2)col.bounds.center;
        float startT = FindClosestT(boundary, cumul, origin);

        // LineRenderer 생성
        LineRenderer lr = CreateLineRenderer(color);
        lock (activeLines) { activeLines.Add(lr); }

        // 퍼져나가기: startT에서 양방향으로 totalLength/2씩 확장
        float elapsed = 0f;
        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / revealDuration);
            float halfSpread = (totalLength * 0.5f) * progress;

            Vector3[] pts = SampleArc(boundary, cumul, startT, halfSpread, totalLength, sampleCount);
            lr.positionCount = pts.Length;
            lr.SetPositions(pts);
            SetAlpha(lr, color, 1f);
            yield return null;
        }

        // 전체 표시 확정
        Vector3[] fullPts = SampleArc(boundary, cumul, startT, totalLength * 0.5f, totalLength, sampleCount);
        lr.positionCount = fullPts.Length;
        lr.SetPositions(fullPts);

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
            SetAlpha(lr, color, alpha);
            yield return null;
        }

        if (lr != null)
        {
            lock (activeLines) { activeLines.Remove(lr); }
            Destroy(lr.gameObject);
        }
    }

    #endregion
    // ─────────────────────────────────────────────
    #region Obstacle Reveal

    private IEnumerator RevealObstacles(Collider2D[] obstacles, Transform startPoint)
    {
        if (obstacles == null || obstacles.Length == 0) yield break;

        // 장애물은 시작점에서 거리 순으로 순차 등장하면 더 자연스러움
        // 원하면 동시 등장으로 변경 가능
        foreach (var obs in obstacles)
        {
            if (obs == null) continue;
            StartCoroutine(RevealSingleCollider(obs, startPoint, obstacle_Color));
            // 약간의 시차를 두려면 아래 yield 주석 해제
            // yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(revealDuration + holdDuration + fadeDuration);
    }

    #endregion
    // ─────────────────────────────────────────────
    #region Geometry Helpers

    /// <summary>시작 T에서 양방향으로 halfSpread만큼 경계를 샘플링합니다.</summary>
    private Vector3[] SampleArc(List<Vector2> boundary, List<float> cumul,
                                 float startT, float halfSpread, float totalLength, int count)
    {
        Vector3[] pts = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            float t = Mathf.Lerp(startT - halfSpread, startT + halfSpread, (float)i / (count - 1));
            t = ((t % totalLength) + totalLength) % totalLength;
            Vector2 pos = SampleAtDistance(boundary, cumul, t);
            pts[i] = new Vector3(pos.x, pos.y, -1f);
        }
        return pts;
    }

    private List<Vector2> GetBoundaryPoints(Collider2D col)
    {
        var pts = new List<Vector2>();

        if (col is PolygonCollider2D poly)
        {
            for (int pi = 0; pi < poly.pathCount; pi++)
            {
                Vector2[] path = poly.GetPath(pi);
                foreach (var p in path)
                    pts.Add(col.transform.TransformPoint(p));
                if (path.Length > 0)
                    pts.Add(col.transform.TransformPoint(path[0]));
            }
        }
        else if (col is BoxCollider2D box)
        {
            Vector2 s = box.size * 0.5f;
            Vector2[] corners =
            {
                box.transform.TransformPoint(box.offset + new Vector2(-s.x, -s.y)),
                box.transform.TransformPoint(box.offset + new Vector2( s.x, -s.y)),
                box.transform.TransformPoint(box.offset + new Vector2( s.x,  s.y)),
                box.transform.TransformPoint(box.offset + new Vector2(-s.x,  s.y)),
            };
            pts.AddRange(corners);
            pts.Add(corners[0]);
        }
        else if (col is EdgeCollider2D edge)
        {
            foreach (var p in edge.points)
                pts.Add(col.transform.TransformPoint(p));
        }
        else if (col is CircleCollider2D circle)
        {
            Vector2 center = (Vector2)circle.transform.TransformPoint(circle.offset);
            float radius = circle.radius * Mathf.Max(
                circle.transform.lossyScale.x,
                circle.transform.lossyScale.y);
            int seg = 64;
            for (int i = 0; i <= seg; i++)
            {
                float angle = (float)i / seg * Mathf.PI * 2f;
                pts.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }
        else if (col is CompositeCollider2D comp)
        {
            for (int pi = 0; pi < comp.pathCount; pi++)
            {
                Vector2[] path = new Vector2[comp.GetPathPointCount(pi)];
                comp.GetPath(pi, path);
                foreach (var p in path)
                    pts.Add(col.transform.TransformPoint(p));
                if (path.Length > 0)
                    pts.Add(col.transform.TransformPoint(path[0]));
            }
        }
        else
        {
            return null;
        }

        return pts;
    }

    private List<float> BuildCumulative(List<Vector2> pts)
    {
        var cumul = new List<float> { 0f };
        for (int i = 1; i < pts.Count; i++)
            cumul.Add(cumul[i - 1] + Vector2.Distance(pts[i - 1], pts[i]));
        return cumul;
    }

    private float FindClosestT(List<Vector2> boundary, List<float> cumul, Vector2 origin)
    {
        float bestT = 0f;
        float bestDist = float.MaxValue;

        for (int i = 0; i < boundary.Count - 1; i++)
        {
            Vector2 closest = ClosestPointOnSegment(boundary[i], boundary[i + 1], origin);
            float d = Vector2.Distance(closest, origin);
            if (d < bestDist)
            {
                bestDist = d;
                float segLen = Vector2.Distance(boundary[i], boundary[i + 1]);
                float localT = segLen > 0f ? Vector2.Distance(boundary[i], closest) / segLen : 0f;
                bestT = cumul[i] + localT * (cumul[i + 1] - cumul[i]);
            }
        }
        return bestT;
    }

    private Vector2 SampleAtDistance(List<Vector2> pts, List<float> cumul, float t)
    {
        for (int i = 1; i < pts.Count; i++)
        {
            if (t <= cumul[i])
            {
                float segLen = cumul[i] - cumul[i - 1];
                float local = segLen > 0f ? (t - cumul[i - 1]) / segLen : 0f;
                return Vector2.Lerp(pts[i - 1], pts[i], local);
            }
        }
        return pts[pts.Count - 1];
    }

    private Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.0001f);
        return a + Mathf.Clamp01(t) * ab;
    }

    #endregion
    // ─────────────────────────────────────────────
    #region LineRenderer Helpers

    private LineRenderer CreateLineRenderer(Color color)
    {
        var obj = new GameObject("PathRevealLine");
        obj.transform.SetParent(transform);

        var lr = obj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.sortingOrder = 12;
        SetAlpha(lr, color, 1f);
        return lr;
    }

    private void SetAlpha(LineRenderer lr, Color baseColor, float alpha)
    {
        if (lr == null) return;
        Color c = baseColor;
        c.a = alpha;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
            new[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(alpha, 1f) }
        );
        lr.colorGradient = g;
    }

    private void ClearLines()
    {
        foreach (var lr in activeLines)
            if (lr != null) Destroy(lr.gameObject);
        activeLines.Clear();
    }

    #endregion
}