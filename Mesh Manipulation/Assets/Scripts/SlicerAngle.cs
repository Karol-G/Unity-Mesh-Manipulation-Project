using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerAngle : MonoBehaviour {

    float slicerAngle = 90;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        updateSlicerAngle();
    }

    public float getSlicerAngle()
    {
        return slicerAngle;
    }

    public Vector3 getTransformRight() {
        return this.transform.GetChild(0).transform.right;
    }

    private void updateSlicerAngle()
    {
        if (Input.GetKeyDown("1"))
        {
            slicerAngle += 10;
            this.transform.GetChild(0).localEulerAngles = new Vector3(0, 0, slicerAngle - 90);
            //print("slicerAngle: " + slicerAngle);
        }

        if (Input.GetKeyDown("2"))
        {
            slicerAngle -= 10;
            this.transform.GetChild(0).localEulerAngles = new Vector3(0, 0, slicerAngle - 90);
            //print("slicerAngle: " + slicerAngle);
        }
    }
}
