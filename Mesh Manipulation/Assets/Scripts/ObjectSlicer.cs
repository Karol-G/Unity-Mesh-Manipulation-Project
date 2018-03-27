using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using System;

public class ObjectSlicer : MonoBehaviour {

    private SlicerAngle slicerAngle;
    public Material capMaterial;

	// Use this for initialization
	void Start () {
        slicerAngle = GetComponent<SlicerAngle>();
    }
	
	// Update is called once per frame
	void Update () {
        checkMouseButtonDown();        
    }

    private void checkMouseButtonDown() {
        if (Input.GetMouseButtonDown(0)) {
            GameObject selectedGameObject = SelecterRaycast.getSelectedGameObject(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)));
            //GameObject selectedGameObject = getSelectedObject();

            if (selectedGameObject != null) {
                sliceSelectedGameObjectCombined(selectedGameObject, this.transform.position, 0);
                //sliceSelectedGameObjectConvex(selectedGameObject);
            }
        }
    }   

	private GameObject getSelectedObject() {		 
   		RaycastHit hit;        
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));        
        if (Physics.Raycast (ray,out hit,100.0f)) {		        
		    return hit.collider.gameObject;
		}		 

		 return null;
	}

    private GameObject sliceSelectedGameObjectCombined(GameObject selectedGameObject, Vector3 position, float rotationAngle)
    {
        Debug.ClearDeveloperConsole();
        GameObject[] hull;
        List<GameObject> selectedGameObjectChildren = getAllLeafChildren(selectedGameObject);
        List<GameObject> newGameObjectChildren = new List<GameObject>(selectedGameObjectChildren);
        GameObject slicerPlane = createSlicerPlane(position, rotationAngle);
        //Vector3 worldPositionOffset = calculateWorldOffsetPosition(selectedGameObject);
        //Vector3 worldRotation = selectedGameObject.transform.eulerAngles;

        foreach (GameObject child in selectedGameObjectChildren) {
            child.transform.parent = null;
            hull = sliceGameObject(slicerPlane, child);
            if (hull != null)
            {                                
                newGameObjectChildren.Remove(child);
                Destroy(child);
                //hull[0].transform.position += worldPositionOffset;
                //hull[1].transform.position += worldPositionOffset;
                PivotPointManager.centerPivotPointOfGameObject(hull[0]/*, worldRotation*/);
                PivotPointManager.centerPivotPointOfGameObject(hull[1]/*, worldRotation*/);
                addMeshColliderToGameObjectDelayed(hull[0]);
                addMeshColliderToGameObjectDelayed(hull[1]);
                newGameObjectChildren.Add(hull[0]);
                newGameObjectChildren.Add(hull[1]);
            }
        }

        GameObject[][] sortedChildren = checkPlaneSideOfGameobjects(slicerPlane, newGameObjectChildren);
        Destroy(slicerPlane);

        return reorderChildren(selectedGameObject, sortedChildren);
        //return null;
    }

    private Vector3 calculateWorldOffsetPosition(GameObject gameObject) {
        if (gameObject.transform.childCount > 0) {
            return gameObject.transform.position;
        }

        return Vector3.zero;
    }

    private GameObject createSlicerPlane(Vector3 position, float rotationAngle) {
        GameObject slicerPlane = Instantiate(Resources.Load("SlicerPlanePrefab"), position, Quaternion.identity) as GameObject;
        slicerPlane.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, 0, slicerAngle.getSlicerAngle());
        slicerPlane.transform.Rotate(new Vector3(rotationAngle, 0, 0), Space.Self);

        return slicerPlane;
    }

    private List<GameObject> getAllLeafChildren(GameObject root) {
        List<GameObject> children = new List<GameObject>();
        foreach (MeshFilter child in root.GetComponentsInChildren<MeshFilter>())
        {
            children.Add(child.gameObject);
        }

        return children;
    }

    private void printGameObjectList(List<GameObject> gameObjectList) {
        print("List size: " + gameObjectList.Count);
        foreach (GameObject gameObject in gameObjectList) {
            print("GameObject: " + gameObject);
        }
    }

    private void printGameObjectList(GameObject[] gameObjectList) {
        print("List size: " + gameObjectList.Length);
        foreach (GameObject gameObject in gameObjectList) {
            print("GameObject: " + gameObject);
        }
    }

    private GameObject[][] checkPlaneSideOfGameobjects(GameObject slicerPlane, List<GameObject> gameObjects) {
        UnityEngine.Plane mathPlane = createMathPlane(slicerPlane);
        List<GameObject>[] sortedGameObjects = new List<GameObject>[2];
        sortedGameObjects[0] = new List<GameObject>();
        sortedGameObjects[1] = new List<GameObject>();

        foreach (GameObject gameObject in gameObjects) {
            createTestPoint(mathPlane, gameObject);
            if (checkPlaneSideOfGameobject(mathPlane, gameObject)) {
                sortedGameObjects[0].Add(gameObject);
            }
            else {
                sortedGameObjects[1].Add(gameObject);
            }
        }

        return new GameObject[][] {sortedGameObjects[0].ToArray(), sortedGameObjects[1].ToArray()};
    }

    private UnityEngine.Plane createMathPlane(GameObject slicerPlane) {
        Vector3 point1 = slicerPlane.transform.TransformPoint(new Vector3(5, 0, 5));
        Vector3 point2 = slicerPlane.transform.TransformPoint(new Vector3(4, 0, 5));
        Vector3 point3 = slicerPlane.transform.TransformPoint(new Vector3(5, 0, 4));
        return new UnityEngine.Plane(point1, point2, point3);
    }

    private bool checkPlaneSideOfGameobject(UnityEngine.Plane mathPlane, GameObject gameObject) {
        return !mathPlane.GetSide(gameObject.transform.position);
    }

    private void createTestPoint(UnityEngine.Plane mathPlane, GameObject gameObject) {
        if (checkPlaneSideOfGameobject(mathPlane, gameObject))
        {
            Instantiate(Resources.Load("TestPointLeftPrefab"), gameObject.transform.position, Quaternion.identity);
        }
        else {
            Instantiate(Resources.Load("TestPointRightPrefab"), gameObject.transform.position, Quaternion.identity);
        }
    }

    private GameObject reorderChildren(GameObject selectedGameObject, GameObject[][] sortedChildren)
    {
        GameObject root = new GameObject(selectedGameObject.name);
        GameObject leftHull = new GameObject("Left Hull");
        GameObject rightHull = new GameObject("Right Hull");
        Quaternion rotation = sortedChildren[0][0].transform.rotation;
        root.transform.position = selectedGameObject.transform.position;        
        leftHull.transform.position = root.transform.position;
        rightHull.transform.position = root.transform.position;
        leftHull.transform.parent = root.transform;
        rightHull.transform.parent = root.transform;
        root.transform.rotation = rotation;
        //print("Reorder Children rotation: " + sortedChildren[0][0].transform.eulerAngles);
        for (int i = 0; i < sortedChildren[0].Length; i++) {
            sortedChildren[0][i].transform.parent = leftHull.transform;
            //sortedChildren[0][i].transform.rotation = Quaternion.identity;
        }

        for (int i = 0; i < sortedChildren[1].Length; i++) {
            sortedChildren[1][i].transform.parent = rightHull.transform;
            //sortedChildren[1][i].transform.rotation = Quaternion.identity;
        }

        //root.transform.rotation = rotation;
        //addRigidbodyToGameObjectDelayed(root);
        Destroy(selectedGameObject);

        return root;
    }

    public GameObject[] sliceGameObject(GameObject slicerPlane, GameObject gameObjectToSlice) {
        SlicedHull slicedHull = slicerPlane.GetComponent<PlaneUsageExample>().SliceObject(gameObjectToSlice);
        if (slicedHull != null && slicedHull.upperHull != null && slicedHull.lowerHull != null) {
            GameObject upperHull = slicedHull.CreateUpperHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
            GameObject lowerHull = slicedHull.CreateLowerHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
            return new GameObject[] { upperHull, lowerHull };
        }

        return null;
    }

    /*private void sliceSelectedGameObjectNonCombined(List<GameObject> selectedGameObjectList)
    {
        // Es müssen zwei neue roots erstellt werden, der alte muss gelöscht werden (Es wird kompliziert mit den verschiedenen Children die einem neuen root zuzuordnen)
        GameObject parent;
        GameObject[] hull;
        foreach (GameObject selectedGameObject in selectedGameObjectList)
        {
            print("selectedGameObject: " + selectedGameObject);
            hull = sliceGameObject(selectedGameObject, position, rotationAngle);
            if (hull != null)
            {
                parent = new GameObject(selectedGameObject.name);
                parent.transform.position = selectedGameObject.transform.position;
                parent.transform.parent = selectedGameObject.transform.parent;
                Destroy(selectedGameObject);
                addMeshColliderToGameObjectDelayed(hull[0]);
                addMeshColliderToGameObjectDelayed(hull[1]);
                hull[0].transform.parent = parent.transform;
                hull[1].transform.parent = parent.transform;
                hull[0].transform.localPosition = Vector3.zero;
                hull[1].transform.localPosition = Vector3.zero;
                // Only root should have a rigidbody
                if (parent.transform.parent == null)
                {
                    addRigidbodyToGameObjectDelayed(parent);
                }
            }
        }
    }*/

    public void addMeshColliderToGameObject(GameObject gameObject, bool convex = true)
    {
        Destroy(gameObject.GetComponent<Collider>());
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
        meshCollider.convex = convex;
    }

    public void addMeshColliderToGameObjectDelayed(GameObject gameObject, bool convex = true)
    {
        Destroy(gameObject.GetComponent<Collider>());
        StartCoroutine(meshColliderCoroutine(gameObject, convex));
    }

    IEnumerator meshColliderCoroutine(GameObject gameObject, bool convex)
    {
        yield return new WaitForSeconds(0.01F);        
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
        meshCollider.convex = convex;
    }

    public void addRigidbodyToGameObject(GameObject gameObject)
    {
        Destroy(gameObject.GetComponent<Rigidbody>());
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>() as Rigidbody;
        rigidbody.useGravity = true;
    }

    public void addRigidbodyToGameObjectDelayed(GameObject gameObject)
    {
        Destroy(gameObject.GetComponent<Rigidbody>());
        StartCoroutine(rigidbodyCoroutine(gameObject));
    }

    IEnumerator rigidbodyCoroutine(GameObject gameObject)
    {       
        yield return new WaitForSeconds(0.01F);
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>() as Rigidbody;
        rigidbody.useGravity = true;
    }
}
