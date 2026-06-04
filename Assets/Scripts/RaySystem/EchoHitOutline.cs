using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// detectLayer 오브젝트에 붙이는 스크립트.
/// RayEmit의 레이가 이 오브젝트에 닿으면 충돌 지점 주변 외곽선을 표시합니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EchoHitOutline : MonoBehaviour
{
    [Header("Outline Shape")]
    [Tooltip("충돌 지점 기준 양쪽으로 그려지는 외곽선 길이 (유닛)")]
    [SerializeField] private float outlineLength = 1.5f;

    [Tooltip("외곽선을 구성하는 포인트 수 (높을수록 부드러움)")]
    [SerializeField] private int outlinePoints = 40;

    [Tooltip("외곽선 두께")]
    [SerializeField] private float outlineWidth = 0.05f;

    [Header("Timing")]
    [Tooltip("완전히 표시된 상태로 유지되는 시간 (초)")]
    [SerializeField] private float holdDuration = 1.2f;

    [Tooltip("페이드아웃 시간 (초)")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Visual")]
    [Tooltip("외곽선 색상")]
    [SerializeField] private Color outlineColor = Color.cyan;

    // 현재 활성화된 외곽선 코루틴 목록 (중복 히트 허용)
    private List<Coroutine> activeCoroutines = new List<Coroutine>();
    private List<LineRenderer> activeRenderers = new List<LineRenderer>();

    /// <summary>
    /// RayEmit에서 레이 충돌 시 호출합니다.
    /// hitPoint : 월드 좌표 충돌 지점
    /// hitNormal: 충돌면 법선 벡터
    /// </summary>
    public void OnRayHit(Vector2 hitPoint, Vector2 hitNormal)
    {
        Coroutine c = StartCoroutine(ShowOutline(hitPoint, hitNormal));
        activeCoroutines.Add(c);
    }

    private IEnumerator ShowOutline(Vector2 hitPoint, Vector2 hitNormal)
    {
        // 충돌면의 접선 방향 (법선을 90도 회전)
        Vector2 tangent = new Vector2(-hitNormal.y, hitNormal.x);

        LineRenderer lr = CreateLineRenderer();
        activeRenderers.Add(lr);

        // --- 외곽선 포인트 계산 ---
        // 충돌 지점 기준 ±outlineLength/2 구간을 오브젝트 외곽선 위에 샘플링
        Collider2D col = GetComponent<Collider2D>();
        Vector3[] points = SampleOutlinePoints(col, hitPoint, tangent, outlineLength, outlinePoints);

        lr.positionCount = points.Length;
        lr.SetPositions(points);

        // Hold 단계 (완전 불투명)
        SetLineAlpha(lr, 1f);
        yield return new WaitForSeconds(holdDuration);

        // FadeOut 단계
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            SetLineAlpha(lr, alpha);
            yield return null;
        }

        // 정리
        activeRenderers.Remove(lr);
        if (lr != null) Destroy(lr.gameObject);
    }

    /// <summary>
    /// 콜라이더 외곽선 위에서 hitPoint 근처 구간을 샘플링합니다.
    /// PolygonCollider2D, CompositeCollider2D, CircleCollider2D, BoxCollider2D 지원.
    /// </summary>
    private Vector3[] SampleOutlinePoints(Collider2D col, Vector2 hitPoint, Vector2 tangent, float length, int count)
    {
        List<Vector2> boundary = GetBoundaryPoints(col);

        if (boundary == null || boundary.Count < 2)
            return FallbackLine(hitPoint, tangent, length, count);

        // hitPoint가 속한 세그먼트 찾기
        int bestSeg = 0;
        float bestDist = float.MaxValue;
        Vector2 bestClosest = hitPoint;

        for (int i = 0; i < boundary.Count - 1; i++)
        {
            Vector2 closest = ClosestPointOnSegment(boundary[i], boundary[i + 1], hitPoint);
            float d = Vector2.Distance(closest, hitPoint);
            if (d < bestDist)
            {
                bestDist = d;
                bestSeg = i;
                bestClosest = closest;
            }
        }

        // 해당 세그먼트의 방향벡터
        Vector2 segDir = (boundary[bestSeg + 1] - boundary[bestSeg]).normalized;
        float segLen = Vector2.Distance(boundary[bestSeg], boundary[bestSeg + 1]);

        // bestClosest 기준으로 세그먼트 안에서만 halfLen 범위 클램프
        float hitT = Vector2.Dot(bestClosest - boundary[bestSeg], segDir);
        float startT = Mathf.Clamp(hitT - length * 0.5f, 0f, segLen);
        float endT = Mathf.Clamp(hitT + length * 0.5f, 0f, segLen);

        Vector3[] result = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            float t = Mathf.Lerp(startT, endT, (float)i / (count - 1));
            Vector2 pos = boundary[bestSeg] + segDir * t;
            result[i] = new Vector3(pos.x, pos.y, -1f);
        }

        return result;
    }

    private List<Vector2> GetBoundaryPoints(Collider2D col)
    {
        Debug.Log($"Collider type: {col.GetType().Name}"); // 추가
        List<Vector2> pts = new List<Vector2>();

        if (col is PolygonCollider2D poly)
        {
            // 각 경로를 월드 좌표로 변환 (닫힌 루프 포함)
            for (int pi = 0; pi < poly.pathCount; pi++)
            {
                Vector2[] path = poly.GetPath(pi);
                for (int i = 0; i < path.Length; i++)
                    pts.Add(col.transform.TransformPoint(path[i]));
                // 닫기
                if (path.Length > 0)
                    pts.Add(col.transform.TransformPoint(path[0]));
            }
        }
        else if (col is BoxCollider2D box)
        {
            Vector2 c = (Vector2)box.transform.TransformPoint(box.offset);
            Vector2 s = box.size * 0.5f;
            // 로컬 코너 → 월드
            Vector2[] corners = new Vector2[]
            {
                box.transform.TransformPoint(box.offset + new Vector2(-s.x, -s.y)),
                box.transform.TransformPoint(box.offset + new Vector2( s.x, -s.y)),
                box.transform.TransformPoint(box.offset + new Vector2( s.x,  s.y)),
                box.transform.TransformPoint(box.offset + new Vector2(-s.x,  s.y)),
            };
            pts.AddRange(corners);
            pts.Add(corners[0]); // 닫기
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
            return null; // 지원 안 하는 타입 → 폴백
        }

        return pts;
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

    private Vector3[] FallbackLine(Vector2 center, Vector2 tangent, float length, int count)
    {
        Vector3[] pts = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            float t = Mathf.Lerp(-length * 0.5f, length * 0.5f, (float)i / (count - 1));
            Vector2 pos = center + tangent * t;
            pts[i] = new Vector3(pos.x, pos.y, -1f);
        }
        return pts;
    }

    private void SetLineAlpha(LineRenderer lr, float alpha)
    {
        if (lr == null) return;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(outlineColor, 0f),
                new GradientColorKey(outlineColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(alpha, 0f),
                new GradientAlphaKey(alpha, 1f)
            }
        );
        lr.colorGradient = g;
    }

    private LineRenderer CreateLineRenderer()
    {
        GameObject obj = new GameObject("EchoOutline");
        obj.transform.SetParent(this.transform);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = outlineWidth;
        lr.endWidth = outlineWidth;
        lr.useWorldSpace = true;
        lr.sortingOrder = 11;

        return lr;
    }

    private void OnDestroy()
    {
        foreach (var lr in activeRenderers)
            if (lr != null) Destroy(lr.gameObject);
        activeRenderers.Clear();
    }
}