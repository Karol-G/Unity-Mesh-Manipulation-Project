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
            List<GameObject> selectedGameObjects = SelecterRaycast.getSelectedGameObjects(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)));
            //GameObject selectedGameObject = getSelectedObject();

            if (selectedGameObjects != null) {
                sliceSelectedGameObject1(selectedGameObjects);
                //sliceSelectedGameObjectConvex(selectedGameObject);
            }
        }
        if (Input.GetMouseButtonDown(2))
        {
            GameObject selectedGameObject = getSelectedObject();

            if (selectedGameObject != null)
            {
                sliceSelectedGameObjectConvex(selectedGameObject);
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

    public void sliceSelectedGameObject(GameObject selectedGameObject, bool convexSlicer = true, bool childSlicer = false) {
        if (convexSlicer) {
            sliceSelectedGameObjectConvex(selectedGameObject);
        }
        else {
            sliceSelectedGameObjectNonConvex(selectedGameObject);
        }
    }

    private void sliceSelectedGameObjectConvex(GameObject selectedGameObject) {
        GameObject[] hull = sliceGameObject(selectedGameObject, this.transform.position, 0);       
        Destroy(selectedGameObject);
        addMeshColliderToGameObjectDelayed(hull[0]);
        addMeshColliderToGameObjectDelayed(hull[1]);
        addRigidbodyToGameObjectDelayed(hull[0]);
        addRigidbodyToGameObjectDelayed(hull[1]);
    }

    private void sliceSelectedGameObject1(List<GameObject> selectedGameObjectList)
    {
        GameObject parent;
        GameObject[] hull;
        foreach (GameObject selectedGameObject in selectedGameObjectList) {
            print("selectedGameObject: " + selectedGameObject);
            parent = new GameObject(selectedGameObject.name);
            parent.transform.position = selectedGameObject.transform.position;
            parent.transform.parent = selectedGameObject.transform.parent;            
            hull = sliceGameObject(selectedGameObject, this.transform.position, 0);
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

    private void sliceSelectedGameObject2(List<GameObject> selectedGameObjectList)
    {
        // Es müssen zwei neue roots erstellt werden, der alte muss gelöscht werden (Es wird kompliziert mit den verschiedenen Children die einem neuen root zuzuordnen)
        GameObject root;
        GameObject[] hull;
        foreach (GameObject selectedGameObject in selectedGameObjectList)
        {
            root = new GameObject(selectedGameObject.name);
            root.transform.position = selectedGameObject.transform.position;
            root.transform.parent = selectedGameObject.transform.parent;
            hull = sliceGameObject(selectedGameObject, this.transform.position, 0);
            Destroy(selectedGameObject);
            addMeshColliderToGameObjectDelayed(hull[0]);
            addMeshColliderToGameObjectDelayed(hull[1]);

            addRigidbodyToGameObjectDelayed(hull[0]);
            addRigidbodyToGameObjectDelayed(hull[1]);
        }
    }

    private void sliceSelectedGameObjectNonConvex(GameObject selectedGameObject) {
        GameObject[] hull = BLINDED_AM_ME.MeshCut.Cut(selectedGameObject, transform.position, slicerAngle.getTransformRight(), capMaterial);
        addMeshColliderToGameObjectDelayed(hull[0]);
        addMeshColliderToGameObjectDelayed(hull[1]);
        addRigidbodyToGameObjectDelayed(hull[0]);
        addRigidbodyToGameObjectDelayed(hull[1]);
    }

    public GameObject[] sliceGameObject(GameObject gameObjectToSlice, Vector3 position, float rotationAngle, bool convexSlicer = true) {
        if (convexSlicer) {
            return sliceGameObjectConvex(gameObjectToSlice, position, rotationAngle);
        }
        else {
            return sliceGameObjectNonConvex(gameObjectToSlice, position, rotationAngle);
        }
    }

    public GameObject[] sliceGameObjectConvex(GameObject gameObjectToSlice, Vector3 position, float rotationAngle) {
        GameObject slicerPlane = Instantiate(Resources.Load("SlicerPlanePrefab"), position, Quaternion.identity) as GameObject;
        slicerPlane.transform.eulerAngles = this.transform.eulerAngles + new Vector3(0, 0, slicerAngle.getSlicerAngle());
        slicerPlane.transform.Rotate(new Vector3(rotationAngle, 0, 0), Space.Self);
        SlicedHull slicedHull = slicerPlane.GetComponent<PlaneUsageExample>().SliceObject(gameObjectToSlice);
        if (slicedHull == null)
        {
            return null;
        }
        GameObject upperHull = slicedHull.CreateUpperHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
        GameObject lowerHull = slicedHull.CreateLowerHull(gameObjectToSlice, gameObjectToSlice.GetComponent<MeshRenderer>().material);
        Destroy(slicerPlane);

        return new GameObject[] {upperHull, lowerHull};
    }

    public GameObject[] sliceGameObjectNonConvex(GameObject gameObjectToSlice, Vector3 position, float rotationAngle) {
        Vector3 normal = Quaternion.Euler(transform.up * rotationAngle) * slicerAngle.getTransformRight();
        GameObject[] hull = BLINDED_AM_ME.MeshCut.Cut(gameObjectToSlice, position, normal, capMaterial);

        return hull;
    }

    /*public void addMeshColliderToGameObject(GameObject gameObject, bool convex = true) {
        if (gameObject.transform.childCount == 0) {
            foreach (GameObject child in getFirstChildren(gameObject)) {
                addMeshColliderToGameObject(child, convex);
            }
        }
        else {
            Destroy(gameObject.GetComponent<Collider>());
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
            meshCollider.convex = convex;
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

    /*private GameObject[] getFirstChildren(GameObject parent)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>();
        GameObject[] firstChildren = new GameObject[parent.transform.childCount];
        int index = 0;
        foreach (Transform child in children)
        {
            if (child.parent == parent)
            {
                firstChildren[index] = child.gameObject;
                index++;
            }
        }
        return firstChildren;
    }*/
}
