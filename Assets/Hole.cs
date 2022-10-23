using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour {
    public float radius;
    public int circlePoints;
    public Vector3 center;
    public Material holeMaterial;
    public Transform holeRendered;

    public Vector3[] vertices;

    MeshFilter meshFilter;

    int NextCircleVertex(Vector3[] vertices, int index) {
        if (index < vertices.Length - 1)
            return index + 1;
        else {
            return 4;
        }
    }
    int ClosestCircleCorner(Vector3[] vertices, int index1, int index2) {
        float closestTotalDistance = Mathf.Infinity;
        int closestCorner = 0;
        for (int i = 0; i < 4; i++) {
            float totalDistance = Vector3.Distance(vertices[index1], vertices[i]) + Vector3.Distance(vertices[index2], vertices[i]) -
                2 * Vector3.Distance(center, vertices[i]);
            if (totalDistance < closestTotalDistance) {
                closestTotalDistance = totalDistance;
                closestCorner = i;
            }
        }
        return closestCorner;
    }

    public void Start() {
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = holeMaterial;

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
    }
    public void Update() {
        center = holeRendered.position;

        vertices = new Vector3[circlePoints + 4]; //first 4 are rectangle edges around the circle
        vertices[0] = new Vector3(-10, 0, -10);
        vertices[1] = new Vector3(10, 0, -10);
        vertices[2] = new Vector3(-10, 0, 10);
        vertices[3] = new Vector3(10, 0, 10);

        for (int i = 4; i < vertices.Length; i++) {
            float theta = Mathf.PI * 2 / circlePoints * i;
            float x = center.x + radius * Mathf.Cos(theta), z = center.z + radius * Mathf.Sin(theta);
            vertices[i] = new Vector3(x, 0, z);
        }
        meshFilter.mesh.vertices = vertices;

        List<int> tris = new List<int>(vertices.Length * 3);

        //add the edge case triangles later
        for (int i = 4; i < vertices.Length; i++) {
            //triangle of current vertex, next vertex, and closest corner the vertices see
            int closestCorner = ClosestCircleCorner(vertices, i, NextCircleVertex(vertices, i));
            int nextCircleVertex = NextCircleVertex(vertices, i);

            int[] triangle = { i, nextCircleVertex, closestCorner };
            for (int j = 0; j < 3; j++)
                tris.Add(triangle[j]);

            //check if it is an edge case triangle (between multiple edges)
            int nextClosestCorner = ClosestCircleCorner(vertices, nextCircleVertex, NextCircleVertex(vertices, nextCircleVertex));

            if (closestCorner != nextClosestCorner) {
                //add triangle
                triangle = new int[] { closestCorner, nextCircleVertex, nextClosestCorner };
                for (int j = 0; j < 3; j++)
                    tris.Add(triangle[j]);
            }
        }
        meshFilter.mesh.triangles = tris.ToArray();

        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++) {
            normals[i] = -Vector3.forward;
        }
        try {
            meshFilter.mesh.normals = normals;
        } catch { }
    }
}