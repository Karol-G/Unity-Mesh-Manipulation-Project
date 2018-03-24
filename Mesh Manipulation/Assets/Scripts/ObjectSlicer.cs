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
                sliceSelectedGameObjectCombined(selectedGameObjects);
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

    private void sliceSelectedGameObjectCombined(List<GameObject> selectedGameObjectList)
    {
        GameObject parent;
        GameObject[] hull;
        foreach (GameObject selectedGameObject in selectedGameObjectList) {
            print("selectedGameObject: " + selectedGameObject);           
            hull = sliceGameObject(selectedGameObject, this.transform.position, 0);
            if (hull != null) {
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
    }

    private void sliceSelectedGameObjectNonCombined(List<GameObject> selectedGameObjectList)
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

    public GameObject[] sliceGameObject(GameObject gameObjectToSlice, Vector3 position, float rotationAngle) {
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
