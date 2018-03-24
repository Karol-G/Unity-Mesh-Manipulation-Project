using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SelecterRaycast : MonoBehaviour { 

    public static GameObject getSelectedGameObject(Ray ray) {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100.0f)) {
            return hit.collider.gameObject;
        }

        return null;
    }

    public static List<GameObject> getSelectedGameObjects(Ray ray) {
        List<GameObject> selectedGameObjects = new List<GameObject>();
        RaycastHit firstHit;
        if (Physics.Raycast(ray, out firstHit, 100.0f))
        {
            GameObject root = firstHit.transform.root.gameObject;
            print("Root: " + root.name);
            
            foreach (Transform child in root.GetComponentsInChildren<Transform>()) {
                print("Foreach loop: " + child.name);
                selectedGameObjects.Add(child.gameObject);
            }

            // Die die hinter origin liegen müssen raus fliegen
            selectedGameObjects = selectedGameObjects.OrderBy(x => Vector2.Distance(ray.origin, x.transform.position)).ToList();
        }

        return selectedGameObjects;
    }

    /*public static List<GameObject> getSelectedGameObjects(Ray ray) {
        List<GameObject> selectedGameObjects = new List<GameObject>();
        RaycastHit firstHit;
        if (Physics.Raycast(ray, out firstHit, 100.0f)) {
            GameObject root = firstHit.transform.root.gameObject;            
            foreach (RaycastHit hit in Physics.RaycastAll(ray)) {
                if (hit.collider.transform.IsChildOf(root.transform)) {
                    selectedGameObjects.Add(hit.collider.gameObject);
                }
            }
            // Die die hinter origin liegen müssen raus fliegen
            selectedGameObjects = selectedGameObjects.OrderBy(x => Vector2.Distance(ray.origin, x.transform.position)).ToList();
        }

        return selectedGameObjects;
    }*/
}
