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
    float sphereRadius;

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
    private List<Vector2> pathPoints;
    private List<Vector3> archedAnchorPoints;
    private List<Vector3> archedControlPoints;
    private List<Vector3> archedPathPoints;
    private int[] multipliers;
    private bool morphing;
    private float elapsedMorphingTime;
    private float translation;
    private Vector3 sphereCenter;

    void Start()
    {
        sphereCenter = new Vector3(0.0f, sphereRadius - perspectiveOffset, 0.0f);
        Random.InitState(seed);
        GeneratePath(seed);
        GenerateMultipliers();
        ProgressPath();
        CreateBezierPath();
        ProjectPathToCylinderSurface();
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
            TranslateControlPoints(new Vector2(translation, 0.0f));
        }
        else if(morphing && elapsedMorphingTime > morphingTime)
        {
            elapsedMorphingTime = 0.0f;
            morphing = false;
        }
        ProgressPath();
        CreateBezierPath();
        ProjectPathToCylinderSurface();
        GetComponent<MeshFilter>().mesh = GenerateMesh();
    }

    void GeneratePath(int seed)
    {
        anchorPoints = new List<Vector2>();
        controlPoints = new List<Vector2>();
        pointOffset = new Vector2(0.0f, 0.0f);

        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector2 anchorPoint = i == 0 ? pointOffset : pointOffset + new Vector2(Random.Range(-stepSize / 4, stepSize / 4), Random.Range(-stepSize / 4, stepSize / 4));            
            anchorPoints.Add(anchorPoint);
            //Instantiate(anchorPrefab, new Vector3(anchorPoint.x, anchorPoint.y, 0), Quaternion.identity);
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

    void TranslateControlPoints(Vector2 translation)
    {
        for (int i = 0; i < controlPoints.Count - 1; i += 2)
        {
            controlPoints[i] = controlPoints[i] + multipliers[i / 2] * translation;
            controlPoints[i + 1] = 2 * anchorPoints[ i / 2] - controlPoints[i];
        }
    }

    void ProgressPath()
    {
        float movement = -sidewaysSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
        Vector2 progression = new Vector2(movement, -progressionSpeed * Time.deltaTime);
        float rotationRadians = rotationSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;

        archedAnchorPoints = new List<Vector3>(anchorPoints.Count);
        archedControlPoints = new List<Vector3>(controlPoints.Count);

        for (int i = 0; i < anchorPoints.Count; i++)
        {
            anchorPoints[i] += progression;
            float rotatedX = anchorPoints[i].x * Mathf.Cos(rotationRadians) - anchorPoints[i].y * Mathf.Sin(rotationRadians);
            float rotatedY = anchorPoints[i].x * Mathf.Sin(rotationRadians) + anchorPoints[i].y * Mathf.Cos(rotationRadians);
            anchorPoints[i] = new Vector2(rotatedX, rotatedY);

            // float theta = rotatedY / sphereRadius;
            
            // float z = sphereRadius * Mathf.Sin(theta);
            // float y = -sphereRadius * Mathf.Cos(theta);
            // archedAnchorPoints.Add(sphereCenter + new Vector3(rotatedX, y, z));
        }
        for (int i = 0; i < controlPoints.Count; i++)
        {
            controlPoints[i] += progression;
            float rotatedX = controlPoints[i].x * Mathf.Cos(rotationRadians) - controlPoints[i].y * Mathf.Sin(rotationRadians);
            float rotatedY = controlPoints[i].x * Mathf.Sin(rotationRadians) + controlPoints[i].y * Mathf.Cos(rotationRadians);
            controlPoints[i] = new Vector2(rotatedX, rotatedY);

            // float theta = rotatedY / sphereRadius;
            
            // float z = sphereRadius * Mathf.Sin(theta);
            // float y = -sphereRadius * Mathf.Cos(theta);
            // archedControlPoints.Add(sphereCenter + new Vector3(rotatedX, y, z));
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

    void Create3DBezierPath()
    {
        archedPathPoints = new List<Vector3>();

        for (int i = 0; i < archedAnchorPoints.Count - 1; i++)
        {
            Vector3 p0 = archedAnchorPoints[i];
            Vector3 p1 = archedControlPoints[2 * i + 1];
            Vector3 p2 = archedControlPoints[2 * i + 2];
            Vector3 p3 = archedAnchorPoints[i + 1];

            for (int j = 0; j <= curveResolution; j++)
            {
                float t = j / (float)curveResolution;
                Vector3 curvePoint = CalculateCubicBezierPoint(t, p0, p1, p2, p3);
                archedPathPoints.Add(curvePoint);
            }
        }
    }

    Vector2 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // (1-t)3p0 + 3(1-t)2tp1 + 3(1-t)t2p2 + t3p2

        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    void ProjectPathToCylinderSurface()
    {
        archedPathPoints = new List<Vector3>();

        foreach (var pathPoint in pathPoints)
        {
            float theta = pathPoint.y / sphereRadius;
            
            float z = sphereRadius * Mathf.Sin(theta);
            float y = -sphereRadius * Mathf.Cos(theta);

            Vector3 archedPathPoint = sphereCenter + new Vector3(pathPoint.x, y, z);
            //Debug.Log("The 2D representation is: " + pathPoint + "and the Vector3 projected to the sphere is: " + archedPathPoint);
            archedPathPoints.Add(archedPathPoint);
        }
    }

    Mesh GenerateMesh()
    {
        int vertIndex = 0;
        int triIndex = 0;

        int[] triangles = new int[2 * (archedPathPoints.Count - 1) * 3];
        Vector3[] vertices = new Vector3[2 * archedPathPoints.Count];
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
            direction.Normalize();
            Vector3 left = Vector3.Cross(direction, Vector3.up);
            //Vector3 left = new Vector3(-direction.z, 0, direction.x);
            vertices[vertIndex] = archedPathPoints[i] + pathWidth * 0.5f * left;
            vertices[vertIndex + 1] = archedPathPoints[i] - pathWidth * 0.5f * left;

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
            triangles = triangles
        };

        return mesh;
    }
}