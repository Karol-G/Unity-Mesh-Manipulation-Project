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

    /*public void combineGameObjectsBackup(GameObject[] gameObjectArray)
    {
        Dictionary<string, List<CombineInstance>> combineInstanceDictionary = new Dictionary<string, List<CombineInstance>>();
        Dictionary<string, Material> materialDictionary = new Dictionary<string, Material>();
        Vector3 position = getCenterOfGameObjects(gameObjectArray);
        GameObject combinedGameObject = new GameObject();
        combinedGameObject.AddComponent<MeshFilter>();
        combinedGameObject.AddComponent<MeshRenderer>();

        //Loop through the array with GameObjects
        for (int i = 0; i < gameObjectArray.Length; i++)
        {
            GameObject currentGameObject = gameObjectArray[i];
            Vector3 oldPosition = currentGameObject.transform.position;
            currentGameObject.transform.position = Vector3.zero;

            //Deactivate the tree 
            currentGameObject.SetActive(false);

            //Get all meshfilters from this GameObject, true to also find deactivated children
            MeshFilter[] meshFilters = currentGameObject.GetComponentsInChildren<MeshFilter>(true);

            //Loop through all children
            for (int j = 0; j < meshFilters.Length; j++)
            {
                MeshFilter meshFilter = meshFilters[j];
                CombineInstance combine = new CombineInstance();
                MeshRenderer meshRender = meshFilter.GetComponent<MeshRenderer>();

                combine.mesh = meshFilter.mesh;
                combine.transform = meshFilter.transform.localToWorldMatrix;                
                string materialName = removeOccurrenceFromString(meshRender.material.name, " (Instance)");

                if (!combineInstanceDictionary.ContainsKey(materialName)) {
                    combineInstanceDictionary[materialName] = new List<CombineInstance>();
                    materialDictionary[materialName] = meshRender.material;
                }

                combineInstanceDictionary[materialName].Add(combine);
            }

            currentGameObject.transform.position = oldPosition;
        }

        int index = 0;
        //Create the array that will form the combined mesh
        CombineInstance[] totalMesh = new CombineInstance[combineInstanceDictionary.Count];
        Material[] materialArray = new Material[combineInstanceDictionary.Count];
        foreach (KeyValuePair<string, List<CombineInstance>> entry in combineInstanceDictionary)
        {
            Mesh combinedMaterialMesh = new Mesh();
            combinedMaterialMesh.CombineMeshes(combineInstanceDictionary[entry.Key].ToArray());
            //Add the submeshes in the same order as the material is set in the combined mesh
            totalMesh[index].mesh = combinedMaterialMesh;
            totalMesh[index].transform = combinedGameObject.transform.localToWorldMatrix;            
            materialArray[index] = materialDictionary[entry.Key];
            index++;
        }

        //Create the final combined mesh
        Mesh combinedAllMesh = new Mesh();

        //Make sure it's set to false to get separate meshes
        combinedAllMesh.CombineMeshes(totalMesh, false);
        addComponentsToCombinedGameObject(combinedGameObject, position, combinedAllMesh, materialArray);
        destroyGameObjectArray(gameObjectArray);
    }*/

    public GameObject combineGameObjects(GameObject[] gameObjectArray, bool removeHiddenTrianglesBool = false) {
        if (removeHiddenTrianglesBool) {
            removeHiddenTriangles(gameObjectArray);
        }

        Vector3 position = getCenterOfGameObjects(gameObjectArray);
        GameObject combinedGameObject = new GameObject();
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

    /*private void addComponentsToCombinedGameObject(GameObject combinedGameObject, Vector3 position, Mesh combinedAllMesh, Material[] materialArray) {
        combinedGameObject.GetComponent<MeshRenderer>().materials = materialArray;
        combinedGameObject.GetComponent<MeshFilter>().mesh = combinedAllMesh;
        MeshCollider meshCollider = combinedGameObject.AddComponent<MeshCollider>() as MeshCollider;
        meshCollider.convex = true;
        Rigidbody rigidbody = combinedGameObject.AddComponent<Rigidbody>() as Rigidbody;
        rigidbody.useGravity = true;
        combinedGameObject.transform.position = position;
    }*/

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

    /*public void removeHiddenTriangles(Mesh mesh) {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        List<Vector3> triangle1 = new List<Vector3>();
        List<Vector3> triangle2 = new List<Vector3>();
        bool isAtLeastOneEqual = false;

        for (int i = 0; i < triangles.Length; i += 3) {
            triangle1.Clear();
            triangle1.Add(vertices[triangles[i]]);
            triangle1.Add(vertices[triangles[i+1]]);
            triangle1.Add(vertices[triangles[i+2]]);

            for (int j = i+3; j < triangles.Length; j += 3)
            {
                triangle2.Clear();
                triangle2.Add(vertices[triangles[j]]);
                triangle2.Add(vertices[triangles[j + 1]]);
                triangle2.Add(vertices[triangles[j + 2]]);

                if (isEqual<Vector3>(triangle1, triangle2)) {
                    print("EQUAL");
                    isAtLeastOneEqual = true;
                }
                print("triangle1: " + triangle1[0] + " " + triangle1[1] + " " + triangle1[2]);
                print("triangle2: " + triangle2[0] + " " + triangle2[1] + " " + triangle2[2]);
            }
        }

        print("isAtLeastOneEqual: " + isAtLeastOneEqual);
    }*/

    /*public bool isEqual<T>(IEnumerable<T> list1, IEnumerable<T> list2)
    {
        Dictionary<T, int> cnt = new Dictionary<T, int>();
        foreach (T s in list1)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]++;
            }
            else
            {
                cnt.Add(s, 1);
            }
        }
        foreach (T s in list2)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]--;
            }
            else
            {
                return false;
            }
        }
        return cnt.Values.All(c => c == 0);
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