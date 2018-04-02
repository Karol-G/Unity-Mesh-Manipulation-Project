using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentHandler : MonoBehaviour {

    public void addMeshColliderToGameObject(GameObject gameObject, bool convex = true) {
        Destroy(gameObject.GetComponent<Collider>());
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
        meshCollider.convex = convex;
    }

    public void addMeshColliderToGameObjectDelayed(GameObject gameObject, bool convex = true) {
        Destroy(gameObject.GetComponent<Collider>());
        StartCoroutine(meshColliderCoroutine(gameObject));
    }

    IEnumerator meshColliderCoroutine(GameObject gameObject) {
        yield return new WaitForSeconds(0.01F);
        if (gameObject != null) {
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
            meshCollider.convex = true;
        }
    }

    public void addRigidbodyToGameObject(GameObject gameObject) {
        Destroy(gameObject.GetComponent<Rigidbody>());
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>() as Rigidbody;
        rigidbody.useGravity = true;
    }

    public void addRigidbodyToGameObjectDelayed(GameObject gameObject) {
        Destroy(gameObject.GetComponent<Rigidbody>());
        StartCoroutine(rigidbodyCoroutine(gameObject));
    }

    IEnumerator rigidbodyCoroutine(GameObject gameObject) {
        yield return new WaitForSeconds(0.01F);
        if (gameObject != null) {
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>() as Rigidbody;
            rigidbody.useGravity = true;
        }
    }
}
