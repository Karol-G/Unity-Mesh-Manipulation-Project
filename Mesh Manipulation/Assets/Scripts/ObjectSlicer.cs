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

    private GameObject[][] sliceSelectedGameObjectCombined(GameObject selectedGameObject, Vector3 position, float rotationAngle)
    {
        GameObject parent;
        GameObject[] hull;
        List<GameObject> selectedGameObjectChildren = getAllLeafChildren(selectedGameObject);
        GameObject slicerPlane = createSlicerPlane(position, rotationAngle);
        foreach (GameObject child in selectedGameObjectChildren) {
            print("selectedGameObject: " + child);           
            hull = sliceGameObject(slicerPlane, child, position, rotationAngle);
            if (hull != null) {
                parent = new GameObject(child.name);
                parent.transform.position = child.transform.position;
                parent.transform.parent = child.transform.parent;
                Destroy(child);
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

        GameObject[][] sortedChildren = checkPlaneSideOfGameobjects(slicerPlane, getAllLeafChildren(selectedGameObject));
        Destroy(slicerPlane);

        return sortedChildren;
    }

    private GameObject createSlicerPlane(Vector3 position, float rotationAngle) {
        GameObject slicerPlane = Instantiate(Resources.Load("DebugSlicerPlanePrefab"), position, Quaternion.identity) as GameObject;
        slicerPlane.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, 0, slicerAngle.getSlicerAngle());
        slicerPlane.transform.Rotate(new Vector3(rotationAngle, 0, 0), Space.Self);

        return slicerPlane;
    }

    private List<GameObject> getAllLeafChildren(GameObject root) {
        //GameObject root = firstHit.transform.root.gameObject;
        List<GameObject> children = new List<GameObject>();
        foreach (Collider child in root.GetComponentsInChildren<Collider>())
        {
            children.Add(child.gameObject);
        }

        return children;
    }

    private GameObject[][] checkPlaneSideOfGameobjects(GameObject slicerPlane, List<GameObject> gameObjects) {
        UnityEngine.Plane mathPlane = createMathPlane(slicerPlane);
        List<GameObject>[] sortedGameObjects = new List<GameObject>[2];
        sortedGameObjects[0] = new List<GameObject>();
        sortedGameObjects[1] = new List<GameObject>();

        foreach (GameObject gameObject in gameObjects) {
            if (checkPlaneSideOfGameobject(mathPlane, gameObject))
            {
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
        foreach (Vector3 vertex in gameObject.GetComponent<MeshFilter>().mesh.vertices) {
            float distance = mathPlane.GetDistanceToPoint(vertex);
            if (distance != 0) {
                if (distance > 0)
                {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        Debug.LogError("An error occured in method checkPlaneSideOfGameobject");
        return true;
    }

    public GameObject[] sliceGameObject(GameObject slicerPlane, GameObject gameObjectToSlice, Vector3 position, float rotationAngle) {
        //GameObject slicerPlane = Instantiate(Resources.Load("DebugSlicerPlanePrefab"), position, Quaternion.identity) as GameObject;        
        //slicerPlane.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, 0, slicerAngle.getSlicerAngle());
        //slicerPlane.transform.Rotate(new Vector3(rotationAngle, 0, 0), Space.Self);
        /*Vector3[] vertices = slicerPlane.GetComponent<MeshFilter>().mesh.vertices;
        foreach (Vector3 vertex in vertices) {
            print("Plane vertex: " + vertex);
        }*/
        SlicedHull slicedHull = slicerPlane.GetComponent<PlaneUsageExample>().SliceObject(gameObjectToSlice);
        if (slicedHull != null) {
            GameObject upperHull = slicedHull.CreateUpperHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
            GameObject lowerHull = slicedHull.CreateLowerHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
            return new GameObject[] { upperHull, lowerHull };
        }
        //Destroy(slicerPlane);

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
