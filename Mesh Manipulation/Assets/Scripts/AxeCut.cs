using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeCut : MonoBehaviour {

    private ObjectSlicer objectSlicer;
    private SlicerAngle slicerAngle;
    private GameObjectCombiner gameObjectCombiner;
    private float cuttingAngle;
    private float cuttingWidth = 0.5F;//0.05F;
    private float cuttingDepth = 0.2F;

    // Use this for initialization
    void Start () {
        objectSlicer = this.GetComponent<ObjectSlicer>();
        slicerAngle = GetComponent<SlicerAngle>();
        gameObjectCombiner = GameObject.Find("Controller").GetComponent<GameObjectCombiner>();
        cuttingAngle = calculateCuttingAngle(cuttingDepth, cuttingWidth);
    }
	
	// Update is called once per frame
	void Update () {
        checkMouseButtonDown();

    }

    private void checkMouseButtonDown()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameObject selectedGameObject = getSelectedObject();

            if (selectedGameObject != null)
            {
                doAxeCut(selectedGameObject);
            }
        }
    }

    private GameObject getSelectedObject()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    private void doAxeCut(GameObject selectedGameObject) {
        bool convexSlicer = false;
        GameObject[] hull1 = objectSlicer.sliceGameObject(selectedGameObject, getSlicerPlanePosition(cuttingWidth/2), -cuttingAngle, convexSlicer);
        Destroy(selectedGameObject);//
        objectSlicer.addMeshColliderToGameObject(hull1[0]);            
        GameObject[] hull2 = objectSlicer.sliceGameObject(hull1[0], getSlicerPlanePosition(-cuttingWidth/2), cuttingAngle, convexSlicer);
        Destroy(hull1[0]);
        hull1[0] = null;
        Destroy(hull2[1]);
        GameObject combinedGameObject = gameObjectCombiner.combineGameObjects(new GameObject[] {hull1[1], hull2[0]}, false);
        objectSlicer.addMeshColliderToGameObject(combinedGameObject, false);
        //objectSlicer.addRigidbodyToGameObject(combinedGameObject);
    }

    private Vector3 getSlicerPlanePosition(float offset)
    {
        Vector3 origin = transform.TransformPoint(Quaternion.Euler(new Vector3(0, 0, slicerAngle.getSlicerAngle() - 90)) * new Vector3(offset, 0, 0));
        Vector3 direction = transform.TransformPoint(Vector3.zero) - transform.TransformPoint(-Vector3.forward);
        RaycastHit hit;
        Ray ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out hit, 1000.0f))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private float calculateCuttingAngle(float cuttingDepth, float cuttingWidth) {        
        return 90 - Mathf.Atan(cuttingDepth / (cuttingWidth / 2)) * Mathf.Rad2Deg;
    }
}
