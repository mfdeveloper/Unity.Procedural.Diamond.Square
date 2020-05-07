using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;

    private Mesh mesh;
    private MeshFilter meshFilter;

    void Awake()
    {
        vertices = new Vector3[4] {
            new Vector3(-1.0f, 0.0f, 1.0f),
            new Vector3(1.0f, 0.0f, 1.0f),
            new Vector3(-1.0f, 0.0f, -1.0f),
            new Vector3(1.0f, 0.0f, -1.0f)
        };

        uvs = new Vector2[4] {
            new Vector3(0.0f, 0.0f),
            new Vector3(1.0f, 0.0f),
            new Vector3(0.0f, 1.0f),
            new Vector3(1.0f, 1.0f)
        };

        triangles = new int[6];

        //Triangle 1
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        //Triangle 2
        triangles[3] = 0;
        triangles[4] = 3;
        triangles[5] = 2;

        meshFilter = GetComponent<MeshFilter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
