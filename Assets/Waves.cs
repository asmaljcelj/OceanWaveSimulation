using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves : MonoBehaviour
{
    public int Dimensions = 10;
    protected MeshFilter meshFilter;
    protected Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.name = gameObject.name;

        mesh.vertices = GenerateVerts();
        mesh.triangles = GenerateTries();
        mesh.RecalculateBounds();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    private Vector3[] GenerateVerts() {
        var verts = new Vector3[(Dimensions + 1) * (Dimensions + 1)];

        for (int x = 0; x <= Dimensions; x++) {
            for (int z = 0; z <= Dimensions; z++) {
                verts[index(x, z)] = new Vector3(x, 0, z);
            }
        }
        return verts;
    }

    private int index(int x, int z) {
        return x * (Dimensions + 1) + z;
    }

    private int[] GenerateTries() {
        var tries = new int[mesh.vertices.Length * 6];

        for (int x = 0; x < Dimensions; x++) {
            for (int z = 0; z < Dimensions; z++) {
                tries[index(x, z) * 6 + 0] = index(x, z);
                tries[index(x, z) * 6 + 1] = index(x + 1, z + 1);
                tries[index(x, z) * 6 + 2] = index(x + 1, z);
                tries[index(x, z) * 6 + 3] = index(x, z);
                tries[index(x, z) * 6 + 4] = index(x, z + 1);
                tries[index(x, z) * 6 + 5] = index(x + 1, z + 1);
            }
        }
        return tries;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
