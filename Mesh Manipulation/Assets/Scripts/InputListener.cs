using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputListener : MonoBehaviour {

    private ObjectSlicer objectSlicer;
    private AxeCut axeCut;

    // Use this for initialization
    void Start() {
        objectSlicer = GetComponent<ObjectSlicer>();
        axeCut = GetComponent<AxeCut>();
    }

    // Update is called once per frame
    void Update() {
        checkForInput();
    }

    private void checkForInput() {
        if (Input.GetMouseButtonDown(0)) {
            GameObject selectedGameObject = getSelectedGameObject();

            if (selectedGameObject != null) {
                objectSlicer.sliceSelectedGameObjectCombined(selectedGameObject, this.transform.position, 0);
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            GameObject selectedGameObject = getSelectedGameObject();

            if (selectedGameObject != null) {
                objectSlicer.sliceSelectedGameObjectNonCombined(selectedGameObject, this.transform.position, 0);
            }
        }

        if (Input.GetMouseButtonDown(2)) {
            GameObject selectedGameObject = getSelectedGameObject();

            if (selectedGameObject != null) {
                axeCut.doAxeCut(selectedGameObject);
            }
        }
    }

    private GameObject getSelectedGameObject() {
        return SelecterRaycast.getSelectedGameObject(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)));
    }
}
