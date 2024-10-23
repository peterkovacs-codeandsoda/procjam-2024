using UnityEngine;
using System.Collections.Generic;

public class PathGenerator : MonoBehaviour
{

    [SerializeField]
    int seed;

    [SerializeField]
    int numberOfPoints;

    [SerializeField]
    float stepSize;

    [SerializeField]
    int curveResolution;

    [SerializeField]
    float morphingSpeed;

    [SerializeField]
    float morphingTime;

    [SerializeField]
    GameObject anchorPrefab;

    [SerializeField]
    GameObject vehiclePrefab;

    private Vector2 pointOffset;
    private List<Vector2> anchorPoints;
    private List<Vector2> controlPoints;
    private List<Vector2> pathPoints;
    private int[] multipliers;
    private LineRenderer lineRenderer;
    private bool morphing;
    private float elapsedMorphingTime;
    private float translation;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        Random.InitState(seed);
        GeneratePath(seed);
        GenerateMultipliers();
        GameObject.Find("Vehicle").transform.position = new Vector3(anchorPoints[0].x, anchorPoints[0].y, 0);
        CreateBezierPath();
        VisualizePath();
    }

    void Update()
    {
        if (!morphing && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            translation = -morphingSpeed * Time.deltaTime;
            morphing = true;
        }
        if (!morphing && (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)))
        {
            translation = morphingSpeed * Time.deltaTime;
            morphing = true;
        }

        if (morphing && elapsedMorphingTime < morphingTime)
        {
            elapsedMorphingTime += Time.deltaTime;
            TranslateControlPoints(new Vector2(translation, 0.0f));
            CreateBezierPath();
            VisualizePath();
        }
        else if(morphing && elapsedMorphingTime > morphingTime)
        {
            elapsedMorphingTime = 0.0f;
            morphing = false;
        }
    }

    void GeneratePath(int seed)
    {
        anchorPoints = new List<Vector2>();
        controlPoints = new List<Vector2>();
        pointOffset = new Vector2(0.0f, 0.0f);

        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector2 anchorPoint = pointOffset + new Vector2(Random.Range(-stepSize / 4, stepSize / 4), Random.Range(-stepSize / 4, stepSize / 4));
            anchorPoints.Add(anchorPoint);
            Instantiate(anchorPrefab, new Vector3(anchorPoint.x, anchorPoint.y, 0), Quaternion.identity);
            int multiplier = Random.Range(0.0f, 1.0f) < 0.5f ? 1 : -1;
            Vector2 controlPoint = anchorPoint + new Vector2(Random.Range(-multiplier * stepSize / 3, -multiplier * stepSize), Random.Range(-stepSize / 3, -stepSize / 4));
            controlPoints.Add(controlPoint);
            controlPoints.Add(2 * anchorPoint - controlPoint);
            pointOffset = pointOffset + new Vector2(multiplier * stepSize / 2, stepSize);
        }
    }

    void GenerateMultipliers()
    {
        multipliers = new int[anchorPoints.Count];
        for (int i = 0; i < multipliers.Length; i++)
        {
            multipliers[i] = Random.Range(0.0f, 1.0f) < 0.5f ? 1 : -1;
        }
    }

    void TranslateControlPoints(Vector2 translation)
    {
        for (int i = 0; i < controlPoints.Count - 1; i += 2)
        {
            controlPoints[i] = controlPoints[i] + multipliers[i / 2] * translation;
            controlPoints[i + 1] = 2 * anchorPoints[ i / 2] - controlPoints[i];
        }
    }

    void CreateBezierPath()
    {
        pathPoints = new List<Vector2>();

        for (int i = 0; i < anchorPoints.Count - 1; i++)
        {
            Vector2 p0 = anchorPoints[i];
            Vector2 p1 = controlPoints[2 * i + 1];
            Vector2 p2 = controlPoints[2 * i + 2];
            Vector2 p3 = anchorPoints[i + 1];

            for (int j = 0; j <= curveResolution; j++)
            {
                float t = j / (float)curveResolution;
                Vector2 curvePoint = CalculateCubicBezierPoint(t, p0, p1, p2, p3);
                pathPoints.Add(curvePoint);
            }
        }
    }

    Vector2 CalculateQuadraticBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        // (1-t)2p0 + 2(1-t)tp1 + t2p2

        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;

        return point;
    }

    Vector2 CalculateCubicBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        // (1-t)3p0 + 3(1-t)2tp1 + 3(1-t)t2p2 + t3p2

        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        Vector2 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    void VisualizePath()
    {
        lineRenderer.positionCount = pathPoints.Count;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(pathPoints[i].x, pathPoints[i].y, 0)); // Z = 0 for 2D visualization
        }
    }
}