using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Output : MonoBehaviour {

    public static void print(List<GameObject> gameObjectList) {
        Debug.Log("List Count: " + gameObjectList.Count);
        foreach (GameObject gameObject in gameObjectList) {
            Debug.Log("Name: " + gameObject.name);
        }
    }
}
