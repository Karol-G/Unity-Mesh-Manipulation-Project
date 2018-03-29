using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeCut : MonoBehaviour {

    private ObjectSlicer objectSlicer;
    private SlicerAngle slicerAngle;
    private GameObjectCombiner gameObjectCombiner;
    private float cuttingAngle;
    private float cuttingWidth = 0.05F;
    private float cuttingDepth = 0.2F;

    // Use this for initialization
    void Start() {
        objectSlicer = this.GetComponent<ObjectSlicer>();
        slicerAngle = GetComponent<SlicerAngle>();
        gameObjectCombiner = GameObject.Find("Controller").GetComponent<GameObjectCombiner>();
        cuttingAngle = calculateCuttingAngle(cuttingDepth, cuttingWidth);
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void doAxeCut(GameObject selectedGameObject) {
        GameObject root = objectSlicer.sliceSelectedGameObjectCombined(selectedGameObject, getSlicerPlanePosition(cuttingWidth / 2), -cuttingAngle, false);
        root = objectSlicer.sliceSelectedGameObjectCombined(root, getSlicerPlanePosition(-cuttingWidth / 2), cuttingAngle, false);
        List<GameObject> leafChildren = getAllLeafChildren(root);
        List<GameObject> tempLeafChildren = new List<GameObject>(leafChildren);

        foreach (GameObject leafChild in leafChildren) {
            if (isGameObjectWaste(leafChild)) {
                tempLeafChildren.Remove(leafChild);
                destroyGameObject(leafChild);                
            }
        }

        handleUnconnectedGameObjects(tempLeafChildren);
        //objectSlicer.addRigidbodyToGameObjectDelayed(root);
    }

    private Vector3 getSlicerPlanePosition(float offset) {
        Vector3 origin = transform.TransformPoint(Quaternion.Euler(new Vector3(0, 0, slicerAngle.getSlicerAngle() - 90)) * new Vector3(offset, 0, 0));
        Vector3 direction = transform.TransformPoint(Vector3.zero) - transform.TransformPoint(-Vector3.forward);
        RaycastHit hit;
        Ray ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out hit, 1000.0f)) {
            return hit.point;
        }

        return Vector3.zero;
    }

    private float calculateCuttingAngle(float cuttingDepth, float cuttingWidth) {
        return 90 - Mathf.Atan(cuttingDepth / (cuttingWidth / 2)) * Mathf.Rad2Deg;
    }

    private bool isGameObjectWaste(GameObject gameObject) {
        List<string> tagList = gameObject.GetComponent<GameObjectAttributes>().getTagList();

        return tagList[tagList.Count - 1].Equals("Right") && tagList[tagList.Count - 2].Equals("Left");
    }

    private void handleUnconnectedGameObjects(List<GameObject> gameObjectList) {
        List<List<GameObject>> gameObjectIsland = calculateIslands(gameObjectList);
        disconnectIslands(gameObjectIsland);
    }

    private List<List<GameObject>> calculateIslands(List<GameObject> gameObjectList) {
        List<List<GameObject>> gameObjectIslands = new List<List<GameObject>>();
        List<GameObject> gameObjectQueue = new List<GameObject>();
        GameObject currentGameObject;
        List<GameObject> connectedGameObjects;
        int index = 0;

        while (gameObjectList.Count > 0) {
            gameObjectQueue.Add(gameObjectList[0]);
            gameObjectIslands.Add(new List<GameObject>());

            while (gameObjectQueue.Count > 0) {
                currentGameObject = gameObjectQueue[0];
                gameObjectQueue.Remove(currentGameObject);
                gameObjectList.Remove(currentGameObject);

                if (!gameObjectIslands[index].Contains(currentGameObject)) {
                    gameObjectIslands[index].Add(currentGameObject);
                }

                connectedGameObjects = currentGameObject.GetComponent<GameObjectAttributes>().getConnectedGameObjectList();
                for (int i = 0; i < connectedGameObjects.Count; i++) {
                    if (!gameObjectQueue.Contains(connectedGameObjects[i]) && !gameObjectIslands[index].Contains(connectedGameObjects[i])) {
                        gameObjectQueue.Add(connectedGameObjects[i]);
                    }
                }

            }

            index++;
        }

        //printIslands(gameObjectIslands);

        return gameObjectIslands;
    }

    private void disconnectIslands(List<List<GameObject>> gameObjectIslands) {
        if (gameObjectIslands.Count > 1) {
            GameObject root = gameObjectIslands[0][0].transform.root.gameObject;
            Quaternion rotation = root.transform.rotation;
            string rootName = root.name;
            detachChildren(root);

            int index = 0;
            GameObject newRoot;            
            foreach (List<GameObject> island in gameObjectIslands) {
                newRoot = new GameObject(rootName + " " + index + 1);
                newRoot.transform.position = findCenterPoint(island);
                newRoot.transform.rotation = rotation;

                foreach (GameObject gameObject in island) {
                    gameObject.transform.parent = newRoot.transform;
                }                
            }
        }
    }

    private void detachChildren(GameObject root) {
        root.transform.GetChild(0).DetachChildren();
        root.transform.GetChild(1).DetachChildren();
        Destroy(root);
    }

    private Vector3 findCenterPoint(List<GameObject> gameObjectList) {
        Vector3 center = Vector3.zero;
        float count = 0;

        foreach (GameObject gameObject in gameObjectList) {
            center += gameObject.transform.position;
            count++;
        }

        return -(center / count);
    }

    private void printIslands(List<List<GameObject>> gameObjectIslands) {
        print("Islands: " + gameObjectIslands.Count);
        int index = 0;
        foreach (List<GameObject> list in gameObjectIslands) {
            print("Islands[" + index + "]: " + list.Count);
            index++;
        }
    }

    private List<GameObject> getAllLeafChildren(GameObject root) {
        List<GameObject> children = new List<GameObject>();
        foreach (MeshFilter child in root.GetComponentsInChildren<MeshFilter>()) {
            children.Add(child.gameObject);
        }

        return children;
    }

    private void destroyGameObject(GameObject gameObject) {
        gameObject.GetComponent<GameObjectAttributes>().destroyGameObject();
        Destroy(gameObject);
    }
    
}
