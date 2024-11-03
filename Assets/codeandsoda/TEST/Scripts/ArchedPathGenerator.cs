using UnityEngine;
using System.Collections.Generic;

public class ArchedPathGenerator : MonoBehaviour
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
    float cylinderRadius;

    [SerializeField]
    float perspectiveOffset;

    [SerializeField]
    float pathWidth;

    [SerializeField]
    float progressionSpeed;

    [SerializeField]
    float sidewaysSpeed;

    [SerializeField]
    float rotationSpeed;

    private Vector2 pointOffset;
    private List<Vector2> anchorPoints;
    private List<Vector2> controlPoints;
    private List<Vector3> archedPathPoints;
    private int[] multipliers;
    private bool morphing;
    private float elapsedMorphingTime;
    private float translation;
    private Vector3 cylinderCenter;

    void Start()
    {
        cylinderCenter = new Vector3(0.0f, cylinderRadius - perspectiveOffset, 0.0f);
        Random.InitState(seed);
        GeneratePath();
        GenerateMultipliers();
        ProgressPath();
        CreateBezierPath();
        GetComponent<MeshFilter>().mesh = GenerateMesh();
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
            TranslateControlPoints();
        }
        else if(morphing && elapsedMorphingTime > morphingTime)
        {
            elapsedMorphingTime = 0.0f;
            morphing = false;
        }
        ProgressPath();
        CreateBezierPath();
        GetComponent<MeshFilter>().sharedMesh = GenerateMesh();
    }

    void GeneratePath()
    {
        anchorPoints = new List<Vector2>();
        controlPoints = new List<Vector2>();
        pointOffset = new Vector2(0.0f, 0.0f);

        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector2 anchorPoint = i == 0 ? pointOffset : pointOffset + new Vector2(Random.Range(-stepSize / 4, stepSize / 4), Random.Range(-stepSize / 4, stepSize / 4));            
            anchorPoints.Add(anchorPoint);
            int multiplier = Random.Range(0.0f, 1.0f) < 0.5f ? 1 : -1;
            Vector2 controlPoint = anchorPoint + new Vector2(Random.Range(-multiplier * stepSize / 3, -multiplier * stepSize), Random.Range(-stepSize / 3, -stepSize / 4));
            controlPoints.Add(controlPoint);
            controlPoints.Add(2 * anchorPoint - controlPoint);
            pointOffset += new Vector2(multiplier * stepSize / 2, stepSize);
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

    void TranslateControlPoints()
    {
        for (int i = 0; i < controlPoints.Count - 1; i += 2)
        {
            if(controlPoints[i].y > 0.0f)
            {
                // Vector2 direction = anchorPoints [i / 2] - controlPoints[i].normalized;
                //direction = translation * new Vector2(-direction.y, direction.x);
                // controlPoints[i] = controlPoints[i] + multipliers[i / 2] * direction;

                Vector2 direction = controlPoints[i] - anchorPoints [i / 2];
                float rotatedX = direction.x * Mathf.Cos(translation) - direction.y * Mathf.Sin(translation);
                float rotatedY = direction.x * Mathf.Sin(translation) + direction.y * Mathf.Cos(translation);
                direction = multipliers[i / 2] * new Vector2(rotatedX, rotatedY);

                controlPoints[i] = anchorPoints [i / 2] + direction;
                controlPoints[i + 1] = 2 * anchorPoints[ i / 2] - controlPoints[i];
            }
        }
    }

    void ProgressPath()
    {
        float movement = -sidewaysSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
        Vector2 progression = new Vector2(movement, -progressionSpeed * Time.deltaTime);
        float rotationRadians = rotationSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;

        for (int i = 0; i < anchorPoints.Count; i++)
        {
            anchorPoints[i] += progression;
            float rotatedX = anchorPoints[i].x * Mathf.Cos(rotationRadians) - anchorPoints[i].y * Mathf.Sin(rotationRadians);
            float rotatedY = anchorPoints[i].x * Mathf.Sin(rotationRadians) + anchorPoints[i].y * Mathf.Cos(rotationRadians);
            anchorPoints[i] = new Vector2(rotatedX, rotatedY);
        }
        for (int i = 0; i < controlPoints.Count; i++)
        {
            controlPoints[i] += progression;
            float rotatedX = controlPoints[i].x * Mathf.Cos(rotationRadians) - controlPoints[i].y * Mathf.Sin(rotationRadians);
            float rotatedY = controlPoints[i].x * Mathf.Sin(rotationRadians) + controlPoints[i].y * Mathf.Cos(rotationRadians);
            controlPoints[i] = new Vector2(rotatedX, rotatedY);
        }
    }

    void CreateBezierPath()
    {
        archedPathPoints = new List<Vector3>();

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
                archedPathPoints.Add(ProjectPointToCylinderSurface(curvePoint));
            }
        }
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

    Vector3 ProjectPointToCylinderSurface(Vector2 pathPoint)
    {
        float theta = pathPoint.y / cylinderRadius;
            
        float z = cylinderRadius * Mathf.Sin(theta);
        float y = -cylinderRadius * Mathf.Cos(theta);

        return cylinderCenter + new Vector3(pathPoint.x, y, z);
    }

    Mesh GenerateMesh()
    {
        int vertIndex = 0;
        int triIndex = 0;

        int[] triangles = new int[2 * (archedPathPoints.Count - 1) * 3];
        Vector3[] vertices = new Vector3[2 * archedPathPoints.Count];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < archedPathPoints.Count; i++)
        {
            Vector3 direction = Vector3.zero;
            if (i < archedPathPoints.Count - 1)
            {
                direction += archedPathPoints[i + 1] - archedPathPoints[i];
            }
            if (i > 0)
            {
                direction += archedPathPoints[i] - archedPathPoints[i-1];
            }
            float tilt = direction.x / 5;
            direction.Normalize();
            Vector3 left = Vector3.Cross(direction, Vector3.up);
            left.y += tilt;
            vertices[vertIndex] = archedPathPoints[i] + pathWidth * 0.5f * left;
            vertices[vertIndex + 1] = archedPathPoints[i] - pathWidth * 0.5f * left;

            float completionPercent = i / (float)(archedPathPoints.Count - 1);
            uvs[vertIndex] = new Vector2(0, completionPercent);
            uvs[vertIndex + 1] = new Vector2(1, completionPercent);

            if (i < archedPathPoints.Count - 1)
            {
                triangles[triIndex] = vertIndex;
                triangles[triIndex + 1] = vertIndex + 2;
                triangles[triIndex + 2] = vertIndex + 1;

                triangles[triIndex + 3] = vertIndex + 1;
                triangles[triIndex + 4] = vertIndex + 2;
                triangles[triIndex + 5] = vertIndex + 3;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };

        return mesh;
    }
}