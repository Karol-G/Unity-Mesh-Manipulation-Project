using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenTriangleRemover {

    public void removeHiddenTriangles(GameObject[] gameObjectArray) {
        for (int i = 0; i < gameObjectArray.Length; i++) {
            for (int j = i + 1; j < gameObjectArray.Length; j++) {
                removeHiddenTriangles(gameObjectArray[i], gameObjectArray[j]);
            }
        }
    }

    public void removeHiddenTriangles(GameObject gameObject1, GameObject gameObject2) {
        Mesh mesh1 = gameObject1.GetComponent<MeshFilter>().mesh;
        Mesh mesh2 = gameObject2.GetComponent<MeshFilter>().mesh;
        List<Vector3> setOfVertices1 = new List<Vector3>();
        List<Vector3> setOfVertices2 = new List<Vector3>();

        foreach (Vector3 vertex1 in mesh1.vertices) {
            foreach (Vector3 vertex2 in mesh2.vertices) {
                if (setOfVertices1.Contains(vertex1) && setOfVertices2.Contains(vertex2)) {
                    break;
                }
                else if (gameObject1.transform.TransformPoint(vertex1).Equals(gameObject2.transform.TransformPoint(vertex2))) {
                    setOfVertices1.Add(vertex1);
                    setOfVertices2.Add(vertex2);
                    break;
                }
            }
        }

        mesh1.triangles = checkSingleMeshForHiddenTriangles(setOfVertices1, mesh1.triangles, mesh1.vertices);
        mesh2.triangles = checkSingleMeshForHiddenTriangles(setOfVertices2, mesh2.triangles, mesh2.vertices);
        mesh1.RecalculateNormals();
        mesh1.RecalculateTangents();
        mesh2.RecalculateNormals();
        mesh2.RecalculateTangents();
        UnityEditor.MeshUtility.Optimize(mesh1);
        UnityEditor.MeshUtility.Optimize(mesh2);
    }

    private int[] checkSingleMeshForHiddenTriangles(List<Vector3> setOfVertices, int[] triangles, Vector3[] vertices) {
        List<int> newTriangles = new List<int>(triangles);
        int index = 0;
        for (int i = 0; i < triangles.Length; i += 3) {
            if (isTriangleHidden(setOfVertices, vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]])) {
                newTriangles.RemoveAt(index + 2);
                newTriangles.RemoveAt(index + 1);
                newTriangles.RemoveAt(index);
                Debug.Log("Triangle removed");
            }
            else {
                index += 3;
            }
        }

        return newTriangles.ToArray();
    }

    private bool isTriangleHidden(List<Vector3> setOfVertices, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) {
        return setOfVertices.Contains(vertex1) && setOfVertices.Contains(vertex2) && setOfVertices.Contains(vertex3);
    }
}
