using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Combine GameObjects with different materials into one mesh
public class GameObjectCombiner : MonoBehaviour
{
    //Array with GameObjects we are going to combine
    public GameObject[] publicGameObjectArray;

    void Start() {
        //combineGameObjects(publicGameObjectArray, true);
    }    

    public GameObject combineGameObjects(GameObject[] gameObjectArray, bool removeHiddenTrianglesBool = false) {
        if (removeHiddenTrianglesBool) {
            removeHiddenTriangles(gameObjectArray);
        }

        Vector3 position = getCenterOfGameObjects(gameObjectArray);
        GameObject combinedGameObject = new GameObject();
        combinedGameObject.transform.rotation = gameObjectArray[0].transform.rotation;
        combinedGameObject.transform.parent = gameObjectArray[0].transform.root;
        combinedGameObject.AddComponent<MeshFilter>();
        combinedGameObject.AddComponent<MeshRenderer>();
        combinedGameObject.transform.position = position;
        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();
        Matrix4x4 myTransform = combinedGameObject.transform.worldToLocalMatrix;        
        MeshFilter[] meshFilters = new MeshFilter[gameObjectArray.Length];
        for (int i = 0; i < gameObjectArray.Length; i++) {
            meshFilters[i] = gameObjectArray[i].GetComponent<MeshFilter>();
        }


        foreach (MeshFilter meshFilter in meshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
            {
                int materialArrayIndex = 0;
                ///////////////////////////////////////////////// WHAT DOES THIS DO???
                for (materialArrayIndex = 0; materialArrayIndex < materials.Count; materialArrayIndex++)
                {
                    if (materials[materialArrayIndex] == meshRenderer.sharedMaterials[s])
                        break;
                }
                /////////////////////////////////////////////////
                if (materialArrayIndex == materials.Count)
                {
                    materials.Add(meshRenderer.sharedMaterials[s]);
                    combineInstanceArrays.Add(new ArrayList());
                }

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.transform = myTransform * meshRenderer.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = s;
                combineInstance.mesh = meshFilter.sharedMesh;
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
            }
        }

        // For MeshFilter        
        // Get / Create mesh filter
        MeshFilter meshFilterCombine = combinedGameObject.GetComponent<MeshFilter>();
        
        // Combine by material index into per-material meshes
        // also, Create CombineInstance array for next step
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for (int m = 0; m < materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        // Combine into one
        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);
        // Destroy other meshes
        foreach (Mesh mesh in meshes)
        {
            mesh.Clear();
            DestroyImmediate(mesh);
        }
        //}

        // For MeshRenderer        
        // Get / Create mesh renderer
        MeshRenderer meshRendererCombine = combinedGameObject.GetComponent<MeshRenderer>();
        
        // Assign materials
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;
        //}

        /*MeshCollider meshCollider = combinedGameObject.AddComponent<MeshCollider>() as MeshCollider;
        meshCollider.convex = true;
        Rigidbody rigidbody = combinedGameObject.AddComponent<Rigidbody>() as Rigidbody;
        rigidbody.useGravity = true;*/       
        destroyGameObjectArray(gameObjectArray);

        return combinedGameObject;
    }

    private Vector3 getCenterOfGameObjects(GameObject[] gameObjectArray) {
        Vector3 center = Vector3.zero;
        float count = 0;

        foreach (GameObject gameObject in gameObjectArray) {
            center += gameObject.transform.position;
            count++;
        }
    
        return center / count;
    }

    private void destroyGameObjectArray(GameObject[] gameObjectArray) {
        foreach (GameObject gameObject in gameObjectArray) {
            Destroy(gameObject);
        }
    }

    /*private string removeOccurrenceFromString(string theString, string occurrence) {
        return theString.Replace(occurrence, "");
    }*/

    public void removeHiddenTriangles(GameObject[] gameObjectArray) {
        for (int i = 0; i < gameObjectArray.Length; i++) {
            for (int j = i+1; j < gameObjectArray.Length; j++) {
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
        for (int i = 0;  i < triangles.Length; i += 3) {
            if (isTriangleHidden(setOfVertices, vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]]))
            {
                newTriangles.RemoveAt(index+2);
                newTriangles.RemoveAt(index+1);
                newTriangles.RemoveAt(index);                
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

    private void printList(List<Vector3> list) {
        foreach (Vector3 point in list) {
            print(point);
        }
    }
}