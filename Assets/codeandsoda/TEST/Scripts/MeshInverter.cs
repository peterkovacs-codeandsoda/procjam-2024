using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshInverter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var triss = meshFilter.sharedMesh.triangles;
        var normals=meshFilter.sharedMesh.normals;
        for (int i=0;i<normals.Length;i++)
            normals[i]=-normals[i];
        for (int i = 0; i < triss.Length / 3; i++)
        {
            int temp = triss[i * 3 + 1];
            triss[i * 3 + 1] = triss[i * 3];
            triss[i * 3] = temp;
        }
        Mesh mesh=Instantiate(meshFilter.sharedMesh);
        mesh.triangles=triss;
        mesh.normals=normals;
        meshFilter.mesh=mesh;
    }

    void FixedUpdate()
    {
        transform.Rotate(0.0f, -0.10f, 0.0f);
    }
}
