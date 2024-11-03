using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{

    [SerializeField]
    bool leftSide;

    private LineRenderer lineRenderer;
    private MeshFilter meshFilter;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        meshFilter = GetComponentInParent<MeshFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        VisualizePath();
    }

    void VisualizePath()
    {
        Vector3[] vertices =  meshFilter.sharedMesh.vertices;
        lineRenderer.positionCount = vertices.Length / 2;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, leftSide ? vertices[i * 2] : vertices[i * 2 + 1]);
        }
    }
}
