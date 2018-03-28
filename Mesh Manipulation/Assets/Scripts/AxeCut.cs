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

        foreach (GameObject leafChild in leafChildren) {
            if (isGameObjectWaste(leafChild)) {
                Destroy(leafChild);
            }
        }

        //handleUnconnectedGameObjects(leafChildren);
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
        List<Vector3[]> gameObjectVerticesList = getAllVertices(gameObjectList);

        for (int i = 0; i < gameObjectVerticesList.Count; i++) {
            if (isGameObjectUnconnected(i, gameObjectVerticesList)) {
                // Es muss noch geprüft werden welche untereinander abhängen
                //gameObjectList[i].transform.parent = null;
            }
        }
    }

    private List<Vector3[]> getAllVertices(List<GameObject> gameObjectList) {
        List<Vector3[]> verticesList = new List<Vector3[]>();

        foreach (GameObject gameObject in gameObjectList) {
            verticesList.Add(convertToWorldCoordinates(gameObject));
        }

        return verticesList;
    }

    private Vector3[] convertToWorldCoordinates(GameObject gameObject) {
        Vector3[] localVertices = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
        Vector3[] globalVertices = new Vector3[localVertices.Length];
        Transform gameObjectTransform = gameObject.transform;

        for (int i = 0; i < localVertices.Length; i++) {
            globalVertices[i] = gameObjectTransform.TransformPoint(localVertices[i]);
        }

        return globalVertices;
    }

    private bool isGameObjectUnconnected(int gameObjectIndex, List<Vector3[]> gameObjectVerticesList) {


        return false;
    }

    private List<GameObject> getAllLeafChildren(GameObject root) {
        List<GameObject> children = new List<GameObject>();
        foreach (MeshFilter child in root.GetComponentsInChildren<MeshFilter>()) {
            children.Add(child.gameObject);
        }

        return children;
    }
}
