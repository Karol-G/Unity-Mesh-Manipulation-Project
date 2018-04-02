using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System;
using System.Linq;

public class ObjectSlicer : MonoBehaviour {

    private SlicerAngle slicerAngle;
    private ConnectedGameObjectHandler connectedGameObjectHandler = new ConnectedGameObjectHandler();
    private ComponentHandler componentHandler;

    // Use this for initialization
    void Start() {
        slicerAngle = GetComponent<SlicerAngle>();
        componentHandler = GameObject.Find("Controller").GetComponent<ComponentHandler>();
    }

    // Update is called once per frame
    void Update() {

    }

    // Currently bugged
    public GameObject[] sliceSelectedGameObjectNonCombined(GameObject selectedGameObject, Vector3 position, float rotationAngle) {
        GameObject root = sliceSelectedGameObjectCombined(selectedGameObject, position, rotationAngle, "Left", "Right", true, false);
        GameObject leftHull = root.transform.GetChild(0).gameObject;
        GameObject rightHull = root.transform.GetChild(1).gameObject;
        componentHandler.addRigidbodyToGameObjectDelayed(leftHull);
        componentHandler.addRigidbodyToGameObjectDelayed(rightHull);
        root.transform.DetachChildren();
        Destroy(root);

        return new GameObject[] { leftHull, rightHull };
    }

    public GameObject sliceSelectedGameObjectCombined(GameObject root, Vector3 position, float rotationAngle, string leftTag, string rightTag, bool genCrossSection, bool addRigidbody = true, bool combined = true) {
        List<GameObject> selectedGameObjectChildren = getAllLeafChildren(root);

        return sliceSelectedGameObjectCombined(selectedGameObjectChildren, position, rotationAngle, leftTag, rightTag, genCrossSection, addRigidbody, combined);
    }

    public GameObject sliceSelectedGameObjectCombined(List<GameObject> gameObjectList, Vector3 position, float rotationAngle, string leftTag, string rightTag, bool genCrossSection, bool addRigidbody = true, bool combined = true) {
        gameObjectList = sortGameObjectsByDistance(gameObjectList, position);
        GameObject root = gameObjectList[0].transform.root.gameObject;
        List<GameObject> newGameObjectChildren = new List<GameObject>(getAllLeafChildren(root));
        GameObject slicerPlane = createSlicerPlane(position, rotationAngle);
        GameObject[] hull;

        foreach (GameObject gameObject in gameObjectList) {
            gameObject.transform.parent = null;
            addGameObjectAttributes(gameObject);
            hull = sliceGameObject(slicerPlane, gameObject, genCrossSection);
            if (hull != null) {
                copyGameObjectAttributes(gameObject, hull);
                newGameObjectChildren.Remove(gameObject);
                PivotPointManager.centerPivotPointOfGameObject(hull[0]);
                PivotPointManager.centerPivotPointOfGameObject(hull[1]);
                connectedGameObjectHandler.updateConnectedGameObjects(hull[0], hull[1], gameObject, combined);
                Destroy(gameObject);
                componentHandler.addMeshColliderToGameObjectDelayed(hull[0]);
                componentHandler.addMeshColliderToGameObjectDelayed(hull[1]);
                newGameObjectChildren.Add(hull[0]);
                newGameObjectChildren.Add(hull[1]);
            }
        }

        GameObject[][] leftRightHull = calculateLeftRightHull(slicerPlane, newGameObjectChildren, leftTag, rightTag);
        Destroy(slicerPlane);
        GameObject newRoot = createHullTreeHierarchy(root, leftRightHull);
        connectedGameObjectHandler.calculateConnectedGameObjects(newGameObjectChildren);

        if (addRigidbody) {
            componentHandler.addRigidbodyToGameObjectDelayed(newRoot);
        }

        return newRoot;
    }

    private GameObject createSlicerPlane(Vector3 position, float rotationAngle) {
        GameObject slicerPlane = Instantiate(Resources.Load("SlicerPlanePrefab"), position, Quaternion.identity) as GameObject;
        slicerPlane.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, 0, slicerAngle.getSlicerAngle());
        slicerPlane.transform.Rotate(new Vector3(rotationAngle, 0, 0), Space.Self);

        return slicerPlane;
    }

    private GameObject createDebugSlicerPlane(Vector3 position, float rotationAngle) {
        GameObject slicerPlane1 = Instantiate(Resources.Load("DebugSlicerPlanePrefab"), position, Quaternion.identity) as GameObject;
        slicerPlane1.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, 0, slicerAngle.getSlicerAngle());
        slicerPlane1.transform.Rotate(new Vector3(rotationAngle, 0, 0), Space.Self);

        GameObject slicerPlane2 = Instantiate(Resources.Load("DebugSlicerPlanePrefab"), position, Quaternion.identity) as GameObject;
        slicerPlane2.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, 0, slicerAngle.getSlicerAngle());
        slicerPlane2.transform.Rotate(new Vector3(rotationAngle, 0, 0), Space.Self);
        slicerPlane2.transform.Rotate(new Vector3(180, 0, 0), Space.Self);

        return createSlicerPlane(position, rotationAngle);
    }

    private List<GameObject> getAllLeafChildren(GameObject root) {
        List<GameObject> children = new List<GameObject>();
        foreach (MeshFilter child in root.GetComponentsInChildren<MeshFilter>()) {
            children.Add(child.gameObject);
        }

        return children;
    }

    private List<GameObject> sortGameObjectsByDistance(List<GameObject> gameObjectList, Vector3 position) {
        return gameObjectList = gameObjectList.OrderBy(x => Vector2.Distance(position, x.transform.position)).ToList();
    }

    private GameObject[][] calculateLeftRightHull(GameObject slicerPlane, List<GameObject> gameObjects, string leftTag, string rightTag) {
        UnityEngine.Plane mathPlane = createMathPlane(slicerPlane);
        List<GameObject>[] sortedGameObjects = new List<GameObject>[2];
        sortedGameObjects[0] = new List<GameObject>();
        sortedGameObjects[1] = new List<GameObject>();

        foreach (GameObject gameObject in gameObjects) {
            //createTestPoint(mathPlane, gameObject);
            if (isLeftHull(mathPlane, gameObject)) {
                // Left
                sortedGameObjects[0].Add(gameObject);
                gameObject.GetComponent<GameObjectAttributes>().addTag(leftTag);
            }
            else {
                // Right
                sortedGameObjects[1].Add(gameObject);
                gameObject.GetComponent<GameObjectAttributes>().addTag(rightTag);
            }
        }

        return new GameObject[][] { sortedGameObjects[0].ToArray(), sortedGameObjects[1].ToArray() };
    }

    private void addGameObjectAttributes(GameObject gameObject) {
        if (gameObject.GetComponent<GameObjectAttributes>() == null) {
            gameObject.AddComponent<GameObjectAttributes>();
        }
    }

    private void copyGameObjectAttributes(GameObject original, GameObject[] hull) {
        GameObjectAttributes attributes = original.GetComponent<GameObjectAttributes>();
        hull[0].AddComponent<GameObjectAttributes>().setTagList(attributes.getTagList());
        hull[1].AddComponent<GameObjectAttributes>().setTagList(attributes.getTagList());
    }    

    private UnityEngine.Plane createMathPlane(GameObject slicerPlane) {
        Vector3 point1 = slicerPlane.transform.TransformPoint(new Vector3(5, 0, 5));
        Vector3 point2 = slicerPlane.transform.TransformPoint(new Vector3(4, 0, 5));
        Vector3 point3 = slicerPlane.transform.TransformPoint(new Vector3(5, 0, 4));
        return new UnityEngine.Plane(point1, point2, point3);
    }

    private bool isLeftHull(UnityEngine.Plane mathPlane, GameObject gameObject) {
        return !mathPlane.GetSide(gameObject.transform.position);
    }

    private void createTestPoint(UnityEngine.Plane mathPlane, GameObject gameObject) {
        if (isLeftHull(mathPlane, gameObject)) {
            Instantiate(Resources.Load("TestPointLeftPrefab"), gameObject.transform.position, Quaternion.identity);
        }
        else {
            Instantiate(Resources.Load("TestPointRightPrefab"), gameObject.transform.position, Quaternion.identity);
        }
    }

    private GameObject createHullTreeHierarchy(GameObject selectedGameObject, GameObject[][] sortedChildren) {
        GameObject root = new GameObject(selectedGameObject.name);
        GameObject leftHull = new GameObject(selectedGameObject.name + " LH");
        GameObject rightHull = new GameObject(selectedGameObject.name + " RH");
        Quaternion rotation = sortedChildren[0][0].transform.rotation;
        root.transform.position = selectedGameObject.transform.position;
        leftHull.transform.position = root.transform.position;
        rightHull.transform.position = root.transform.position;
        leftHull.transform.parent = root.transform;
        rightHull.transform.parent = root.transform;
        root.transform.rotation = rotation;

        for (int i = 0; i < sortedChildren[0].Length; i++) {
            sortedChildren[0][i].transform.parent = leftHull.transform;
        }

        for (int i = 0; i < sortedChildren[1].Length; i++) {
            sortedChildren[1][i].transform.parent = rightHull.transform;
        }

        Destroy(selectedGameObject);

        return root;
    }

    private GameObject[] sliceGameObject(GameObject slicerPlane, GameObject gameObjectToSlice, bool genCrossSection = true) {
        SlicedHull slicedHull = slicerPlane.GetComponent<PlaneUsageExample>().SliceObject(gameObjectToSlice, genCrossSection);
        if (slicedHull != null && slicedHull.upperHull != null && slicedHull.lowerHull != null) {
            GameObject upperHull = slicedHull.CreateUpperHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
            GameObject lowerHull = slicedHull.CreateLowerHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
            return new GameObject[] { upperHull, lowerHull };
        }

        return null;
    }

}
