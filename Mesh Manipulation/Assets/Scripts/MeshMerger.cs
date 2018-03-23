using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMerger : MonoBehaviour {

    public GameObject cube1;
    public GameObject cube2;

	// Use this for initialization
	void Start () {
        //mergeGameObjects(cube1, cube2);
        List<GameObject> meshObjectList = new List<GameObject>();
        meshObjectList.Add(cube1);
        meshObjectList.Add(cube2);
        combineMeshes(meshObjectList);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void combineMeshes(List<GameObject> meshObjectList)
    {

        // combine meshes
        CombineInstance[] combine = new CombineInstance[meshObjectList.Count];
        int i = 0;
        while (i < meshObjectList.Count)
        {
            MeshFilter meshFilter = meshObjectList[i].gameObject.GetComponent<MeshFilter>();
            combine[i].mesh = meshFilter.sharedMesh;
            combine[i].transform = meshFilter.transform.localToWorldMatrix;
            i++;
        }

        meshObjectList[0].GetComponent<MeshFilter>().mesh = new Mesh();
        meshObjectList[0].GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    }

    private void mergeGameObjects(GameObject gameObject1, GameObject gameObject2) {
        Mesh mesh1 = gameObject1.GetComponent<MeshFilter>().mesh;
        Mesh mesh2 = gameObject2.GetComponent<MeshFilter>().mesh;
        mesh1.vertices = addArrays(mesh1.vertices, mesh2.vertices);
        mesh1.triangles = addArrays(mesh1.triangles, mesh2.triangles, mesh1.vertices.Length);
        mesh1.RecalculateBounds();
        mesh1.RecalculateNormals();
        mesh1.RecalculateTangents();
        //mesh1 = autoWeld(mesh1, 0.3F, 1);
        print("Done");
    }

    private Vector3[] addArrays(Vector3[] array1, Vector3[] array2) {
        Vector3[] newArray = new Vector3[array1.Length + array2.Length];
        array1.CopyTo(newArray, 0);
        array2.CopyTo(newArray, array1.Length);

        return newArray;
    }

    private int[] addArrays(int[] array1, int[] array2, int offset)
    {
        for (int i = 0; i < array2.Length; i++) {
            array2[i] += offset;
        }
        int[] newArray = new int[array1.Length + array2.Length];
        array1.CopyTo(newArray, 0);
        array2.CopyTo(newArray, array1.Length);

        return newArray;
    }

    public static Mesh autoWeld(Mesh mesh, float threshold, float bucketStep)
    {
        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

        skip:;
        }

        // Make new triangles
        int[] oldTris = mesh.triangles;
        int[] newTris = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            newTris[i] = old2new[oldTris[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
            finalVertices[i] = newVertices[i];

        mesh.Clear();
        mesh.vertices = finalVertices;
        mesh.triangles = newTris;
        mesh.RecalculateBounds();        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }

    private GameObject CreateGameObjectFromMesh(GameObject original, Material crossSectionMat, Mesh mesh)
    {
        GameObject newObject = CreateEmptyObject("New GameObject", mesh);

        if (newObject != null)
        {
            newObject.transform.localPosition = original.transform.localPosition;
            newObject.transform.localRotation = original.transform.localRotation;
            newObject.transform.localScale = original.transform.localScale;

            Material[] shared = original.GetComponent<MeshRenderer>().sharedMaterials;

            Material[] newShared = new Material[shared.Length + 1];

            // copy our material arrays across using native copy (should be faster than loop)
            System.Array.Copy(shared, newShared, shared.Length);
            newShared[shared.Length] = crossSectionMat;

            // the the material information
            newObject.GetComponent<Renderer>().sharedMaterials = newShared;
        }

        return newObject;
    }

    private static GameObject CreateEmptyObject(string name, Mesh mesh)
    {
        if (mesh == null)
        {
            return null;
        }

        GameObject newObject = new GameObject(name);

        newObject.AddComponent<MeshRenderer>();
        MeshFilter filter = newObject.AddComponent<MeshFilter>();

        filter.mesh = mesh;

        return newObject;
    }
}
