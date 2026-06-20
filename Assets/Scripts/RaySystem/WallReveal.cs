using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallReveal : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] private KeyCode activateKey = KeyCode.F;

    [Header("Path A Walls")]
    [SerializeField] private Tilemap[] pathAWalls;
    [SerializeField] private Transform pathAStartPoint;
    [SerializeField] private Color pathAColor = Color.cyan;

    [Header("Path B Walls")]
    [SerializeField] private Tilemap[] pathBWalls;
    [SerializeField] private Transform pathBStartPoint;
    [SerializeField] private Color pathBColor = new Color(1f, 0.6f, 0.1f);
    [Header("Obstacle Colliders")]
    [SerializeField] private PolygonCollider2D[] obstacleColliders;

    [SerializeField] private Color obstacleColor = Color.red;

    [Header("Visual")]
    [SerializeField] private float lineWidth = 0.08f;
    [SerializeField] private float revealSpeed = 15f;

    [Header("Timing")]
    [SerializeField] private float holdDuration = 3f;
    [SerializeField] private float fadeDuration = 1f;

    private bool playerInRange;

    private readonly List<GameObject> spawnedLines = new();
    private readonly List<WallEdge> edges = new();

    private class WallEdge
    {
        public Vector3 start;
        public Vector3 end;
        public float distance;
        public bool spawned;
        public Color color;
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (InputManager.getPressed)
        {
            StopAllCoroutines();
            ClearLines();
            StartCoroutine(RevealRoutine());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private IEnumerator RevealRoutine()
    {
        edges.Clear();

        BuildEdges(pathAWalls, pathAStartPoint, pathAColor);
        BuildEdges(pathBWalls, pathBStartPoint, pathBColor);
        BuildObstacleEdges(
    obstacleColliders,
    pathBStartPoint,
    obstacleColor);
        float maxDistance = 0f;

        foreach (var edge in edges)
        {
            edge.spawned = false;

            if (edge.distance > maxDistance)
                maxDistance = edge.distance;
        }

        float radius = 0f;

        while (radius < maxDistance + 1f)
        {
            radius += revealSpeed * Time.deltaTime;

            foreach (var edge in edges)
            {
                if (edge.spawned)
                    continue;

                if (edge.distance <= radius)
                {
                    edge.spawned = true;
                    CreateLine(edge.start, edge.end, edge.color);
                }
            }

            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        yield return StartCoroutine(FadeOutRoutine());
    }
    private void BuildObstacleEdges(
    PolygonCollider2D[] colliders,
    Transform startPoint,
    Color color)
    {
        if (colliders == null || startPoint == null)
            return;

        foreach (var poly in colliders)
        {
            if (poly == null)
                continue;

            for (int pathIndex = 0; pathIndex < poly.pathCount; pathIndex++)
            {
                Vector2[] path = poly.GetPath(pathIndex);

                for (int i = 0; i < path.Length; i++)
                {
                    Vector3 start =
                        poly.transform.TransformPoint(path[i]);

                    Vector3 end =
                        poly.transform.TransformPoint(
                            path[(i + 1) % path.Length]);

                    Vector3 mid = (start + end) * 0.5f;

                    edges.Add(new WallEdge
                    {
                        start = start,
                        end = end,
                        distance = Vector3.Distance(
                            startPoint.position,
                            mid),
                        spawned = false,
                        color = color
                    });
                }
            }
        }
    }
    private void BuildEdges(
        Tilemap[] tilemaps,
        Transform startPoint,
        Color color)
    {
        if (tilemaps == null || startPoint == null)
            return;

        foreach (var tilemap in tilemaps)
        {
            if (tilemap == null)
                continue;

            BoundsInt bounds = tilemap.cellBounds;

            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(cell))
                    continue;

                Vector3 center = tilemap.GetCellCenterWorld(cell);

                float hx = tilemap.cellSize.x * 0.5f;
                float hy = tilemap.cellSize.y * 0.5f;

                Vector3 bl = center + new Vector3(-hx, -hy);
                Vector3 br = center + new Vector3(hx, -hy);
                Vector3 tl = center + new Vector3(-hx, hy);
                Vector3 tr = center + new Vector3(hx, hy);

                AddEdge(tilemap, cell, Vector3Int.left, bl, tl, startPoint.position, color);
                AddEdge(tilemap, cell, Vector3Int.right, br, tr, startPoint.position, color);
                AddEdge(tilemap, cell, Vector3Int.up, tl, tr, startPoint.position, color);
                AddEdge(tilemap, cell, Vector3Int.down, bl, br, startPoint.position, color);
            }
        }
    }

    private void AddEdge(
        Tilemap tilemap,
        Vector3Int cell,
        Vector3Int direction,
        Vector3 start,
        Vector3 end,
        Vector3 origin,
        Color color)
    {
        if (tilemap.HasTile(cell + direction))
            return;

        Vector3 mid = (start + end) * 0.5f;

        edges.Add(new WallEdge
        {
            start = start,
            end = end,
            distance = Vector3.Distance(origin, mid),
            spawned = false,
            color = color
        });
    }

    private void CreateLine(
        Vector3 start,
        Vector3 end,
        Color color)
    {
        GameObject obj = new GameObject("WallEdge");
        obj.transform.SetParent(transform);

        LineRenderer lr = obj.AddComponent<LineRenderer>();

        lr.positionCount = 2;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.material = new Material(Shader.Find("Sprites/Default"));

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lr.startColor = color;
        lr.endColor = color;

        lr.useWorldSpace = true;
        lr.sortingOrder = 100;

        spawnedLines.Add(obj);
    }

    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;

        List<LineRenderer> renderers = new();

        foreach (var obj in spawnedLines)
        {
            if (obj == null)
                continue;

            LineRenderer lr = obj.GetComponent<LineRenderer>();

            if (lr != null)
                renderers.Add(lr);
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            foreach (var lr in renderers)
            {
                Color c = lr.startColor;
                c.a = alpha;

                lr.startColor = c;
                lr.endColor = c;
            }

            yield return null;
        }

        ClearLines();
    }

    private void ClearLines()
    {
        foreach (var obj in spawnedLines)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedLines.Clear();
    }
}