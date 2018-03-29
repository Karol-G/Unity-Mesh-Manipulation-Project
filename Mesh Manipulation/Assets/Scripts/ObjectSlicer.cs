using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System;
using System.Linq;

public class ObjectSlicer : MonoBehaviour {

    private SlicerAngle slicerAngle;

    // Use this for initialization
    void Start() {
        slicerAngle = GetComponent<SlicerAngle>();
    }

    // Update is called once per frame
    void Update() {

    }

    // Currently bugged
    public GameObject[] sliceSelectedGameObjectNonCombined(GameObject selectedGameObject, Vector3 position, float rotationAngle) {
        GameObject root = sliceSelectedGameObjectCombined(selectedGameObject, position, rotationAngle, true, false);
        GameObject leftHull = root.transform.GetChild(0).gameObject;
        GameObject rightHull = root.transform.GetChild(1).gameObject;
        addRigidbodyToGameObjectDelayed(leftHull);
        addRigidbodyToGameObjectDelayed(rightHull);
        root.transform.DetachChildren();
        Destroy(root);

        return new GameObject[] { leftHull, rightHull };
    }

    public GameObject sliceSelectedGameObjectCombined(GameObject selectedGameObject, Vector3 position, float rotationAngle, bool addRigidbody = true, bool combined = true) {
        //Debug.ClearDeveloperConsole();
        GameObject[] hull;
        List<GameObject> selectedGameObjectChildren = getAllLeafChildren(selectedGameObject);
        List<GameObject> newGameObjectChildren = new List<GameObject>(selectedGameObjectChildren);
        GameObject slicerPlane = createSlicerPlane(position, rotationAngle);

        foreach (GameObject child in selectedGameObjectChildren) {
            child.transform.parent = null;
            addGameObjectAttributes(child);
            hull = sliceGameObject(slicerPlane, child);
            if (hull != null) {
                copyGameObjectAttributes(child, hull);
                newGameObjectChildren.Remove(child);
                PivotPointManager.centerPivotPointOfGameObject(hull[0]);
                PivotPointManager.centerPivotPointOfGameObject(hull[1]);
                updateConnectedGameObjects(hull[0], hull[1], child, combined);
                Destroy(child);
                addMeshColliderToGameObjectDelayed(hull[0]);
                addMeshColliderToGameObjectDelayed(hull[1]);
                newGameObjectChildren.Add(hull[0]);
                newGameObjectChildren.Add(hull[1]);
            }
        }

        GameObject[][] leftRightHull = calculateLeftRightHull(slicerPlane, newGameObjectChildren);
        Destroy(slicerPlane);
        GameObject newRoot = createHullTreeHierarchy(selectedGameObject, leftRightHull);
        calculateConnectedGameObjects(newGameObjectChildren);

        if (addRigidbody) {
            addRigidbodyToGameObjectDelayed(newRoot);
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

    private GameObject[][] calculateLeftRightHull(GameObject slicerPlane, List<GameObject> gameObjects) {
        UnityEngine.Plane mathPlane = createMathPlane(slicerPlane);
        List<GameObject>[] sortedGameObjects = new List<GameObject>[2];
        sortedGameObjects[0] = new List<GameObject>();
        sortedGameObjects[1] = new List<GameObject>();

        foreach (GameObject gameObject in gameObjects) {
            //createTestPoint(mathPlane, gameObject);
            if (isLeftHull(mathPlane, gameObject)) {
                // Left
                sortedGameObjects[0].Add(gameObject);
                gameObject.GetComponent<GameObjectAttributes>().addTag("Left");
            }
            else {
                // Right
                sortedGameObjects[1].Add(gameObject);
                gameObject.GetComponent<GameObjectAttributes>().addTag("Right");
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

    private void updateConnectedGameObjects(GameObject gameObject1, GameObject gameObject2, GameObject original, bool combined) {
        GameObjectAttributes originalAttributes = original.GetComponent<GameObjectAttributes>();
        GameObjectAttributes connectedGameObjectAttributes;
        if (combined) {
            gameObject1.GetComponent<GameObjectAttributes>().addConnectedGameObject(gameObject2);
            gameObject2.GetComponent<GameObjectAttributes>().addConnectedGameObject(gameObject1);
        }

        foreach (GameObject connectedGameObject in originalAttributes.getConnectedGameObjectList().Concat(originalAttributes.getPotentialyConnectedGameObjectList())) {
            //print("Original: " + original.name + " other: " + connectedGameObject.name);
            connectedGameObjectAttributes = connectedGameObject.GetComponent<GameObjectAttributes>();
            connectedGameObjectAttributes.removeConnectedGameObject(original);
            connectedGameObjectAttributes.removePotentialyConnectedGameObject(original);
            connectedGameObjectAttributes.addPotentialyConnectedGameObject(gameObject1);
            connectedGameObjectAttributes.addPotentialyConnectedGameObject(gameObject2);
        }

        gameObject1.GetComponent<GameObjectAttributes>().addPotentialyConnectedGameObjectList(originalAttributes.getPotentialyConnectedGameObjectList());
        gameObject2.GetComponent<GameObjectAttributes>().addPotentialyConnectedGameObjectList(originalAttributes.getPotentialyConnectedGameObjectList());
    }

    private void calculateConnectedGameObjects(List<GameObject> gameObjectList) {
        foreach (GameObject gameObject in gameObjectList) {
            calculateConnectedGameObjectsForSingle(gameObject);
        }
    }

    private void calculateConnectedGameObjectsForSingle(GameObject gameObject) {
        GameObjectAttributes attributes = gameObject.GetComponent<GameObjectAttributes>();
        Vector3[] gameObjectVertices1 = convertToWorldCoordinates(gameObject);
        GameObjectAttributes otherAttributes;

        foreach (GameObject other in attributes.getPotentialyConnectedGameObjectList()) {
            Vector3[] otherVertices = convertToWorldCoordinates(other);
            //print("Potentialy connected: " + gameObject.name + " with " + other.name);
            if (isGameObjectConnected(gameObjectVertices1, otherVertices)) {
                //print("Connected: " + gameObject.name + " with " + other.name);
                attributes.addConnectedGameObject(other);
                otherAttributes = other.GetComponent<GameObjectAttributes>();
                otherAttributes.removePotentialyConnectedGameObject(gameObject);
                otherAttributes.addConnectedGameObject(gameObject);
            }
        }

        attributes.clearPotentialyConnectedGameObject();
    }

    private Vector3[] convertToWorldCoordinates(GameObject gameObject) {
        HashSet<Vector3> localVertices = new HashSet<Vector3>(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices);
        Vector3[] globalVertices = new Vector3[localVertices.Count];
        Transform gameObjectTransform = gameObject.transform;

        int index = 0;
        foreach (Vector3 vertex in localVertices) {
            globalVertices[index] = gameObjectTransform.TransformPoint(vertex);
            //print("VERTEX: " + globalVertices[index]); 
            index++;
        }

        return globalVertices;
    }

    private bool isGameObjectConnected(Vector3[] gameObjectVertices, Vector3[] otherVertices) {
        foreach (Vector3 vertex1 in gameObjectVertices) {
            foreach (Vector3 vertex2 in otherVertices) {
                //print("Vertex1: " + vertex1 + " Vertex2: " + vertex2);
                if (vertex1 == vertex2) {
                    //print("Vertex: " + vertex1);
                    return true;
                }
            }
        }

        return false;
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

    private GameObject[] sliceGameObject(GameObject slicerPlane, GameObject gameObjectToSlice) {
        SlicedHull slicedHull = slicerPlane.GetComponent<PlaneUsageExample>().SliceObject(gameObjectToSlice);
        if (slicedHull != null && slicedHull.upperHull != null && slicedHull.lowerHull != null) {
            GameObject upperHull = slicedHull.CreateUpperHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
            GameObject lowerHull = slicedHull.CreateLowerHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
            return new GameObject[] { upperHull, lowerHull };
        }

        return null;
    }

    public void addMeshColliderToGameObject(GameObject gameObject, bool convex = true) {
        Destroy(gameObject.GetComponent<Collider>());
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
        meshCollider.convex = convex;
    }

    public void addMeshColliderToGameObjectDelayed(GameObject gameObject, bool convex = true) {
        Destroy(gameObject.GetComponent<Collider>());
        StartCoroutine(meshColliderCoroutine(gameObject));
    }

    IEnumerator meshColliderCoroutine(GameObject gameObject) {
        yield return new WaitForSeconds(0.01F);
        if (gameObject != null) {
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
            meshCollider.convex = true;
        }
    }

    public void addRigidbodyToGameObject(GameObject gameObject) {
        Destroy(gameObject.GetComponent<Rigidbody>());
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>() as Rigidbody;
        rigidbody.useGravity = true;
    }

    public void addRigidbodyToGameObjectDelayed(GameObject gameObject) {
        Destroy(gameObject.GetComponent<Rigidbody>());
        StartCoroutine(rigidbodyCoroutine(gameObject));
    }

    IEnumerator rigidbodyCoroutine(GameObject gameObject) {
        yield return new WaitForSeconds(0.01F);
        if (gameObject != null) {
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>() as Rigidbody;
            rigidbody.useGravity = true;
        }
    }
}
