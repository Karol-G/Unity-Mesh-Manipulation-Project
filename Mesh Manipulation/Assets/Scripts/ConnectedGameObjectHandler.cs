using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ConnectedGameObjectHandler {

    public void updateConnectedGameObjects(GameObject gameObject1, GameObject gameObject2, GameObject original, bool combined) {
        GameObjectAttributes originalAttributes = original.GetComponent<GameObjectAttributes>();
        GameObjectAttributes connectedGameObjectAttributes;
        if (combined) {
            gameObject1.GetComponent<GameObjectAttributes>().addConnectedGameObject(gameObject2);
            gameObject2.GetComponent<GameObjectAttributes>().addConnectedGameObject(gameObject1);
        }

        foreach (GameObject connectedGameObject in originalAttributes.getConnectedGameObjectList().Concat(originalAttributes.getPotentialyConnectedGameObjectList())) {
            //print("Original: " + original.name + " other: " + connectedGameObject.name);
            connectedGameObjectAttributes = connectedGameObject.GetComponent<GameObjectAttributes>();
            connectedGameObjectAttributes.removeConnectedGameObject(original);
            connectedGameObjectAttributes.removePotentialyConnectedGameObject(original);
            connectedGameObjectAttributes.addPotentialyConnectedGameObject(gameObject1);
            connectedGameObjectAttributes.addPotentialyConnectedGameObject(gameObject2);
        }

        gameObject1.GetComponent<GameObjectAttributes>().addPotentialyConnectedGameObjectList(originalAttributes.getPotentialyConnectedGameObjectList());
        gameObject2.GetComponent<GameObjectAttributes>().addPotentialyConnectedGameObjectList(originalAttributes.getPotentialyConnectedGameObjectList());
    }

    public void calculateConnectedGameObjects(List<GameObject> gameObjectList) {
        foreach (GameObject gameObject in gameObjectList) {
            calculateConnectedGameObjectsForSingle(gameObject);
        }
    }

    private void calculateConnectedGameObjectsForSingle(GameObject gameObject) {
        GameObjectAttributes attributes = gameObject.GetComponent<GameObjectAttributes>();
        Vector3[] gameObjectVertices1 = convertToWorldCoordinates(gameObject);
        GameObjectAttributes otherAttributes;

        foreach (GameObject other in attributes.getPotentialyConnectedGameObjectList()) {
            Vector3[] otherVertices = convertToWorldCoordinates(other);
            //print("Potentialy connected: " + gameObject.name + " with " + other.name);
            if (isGameObjectConnected(gameObjectVertices1, otherVertices)) {
                //print("Connected: " + gameObject.name + " with " + other.name);
                attributes.addConnectedGameObject(other);
                otherAttributes = other.GetComponent<GameObjectAttributes>();
                otherAttributes.removePotentialyConnectedGameObject(gameObject);
                otherAttributes.addConnectedGameObject(gameObject);
            }
        }

        attributes.clearPotentialyConnectedGameObject();
    }

    private Vector3[] convertToWorldCoordinates(GameObject gameObject) {
        HashSet<Vector3> localVertices = new HashSet<Vector3>(gameObject.GetComponent<MeshFilter>().sharedMesh.vertices);
        Vector3[] globalVertices = new Vector3[localVertices.Count];
        Transform gameObjectTransform = gameObject.transform;

        int index = 0;
        foreach (Vector3 vertex in localVertices) {
            globalVertices[index] = gameObjectTransform.TransformPoint(vertex);
            //print("VERTEX: " + globalVertices[index]); 
            index++;
        }

        return globalVertices;
    }

    private bool isGameObjectConnected(Vector3[] gameObjectVertices, Vector3[] otherVertices) {
        foreach (Vector3 vertex1 in gameObjectVertices) {
            foreach (Vector3 vertex2 in otherVertices) {
                //print("Vertex1: " + vertex1 + " Vertex2: " + vertex2);
                if (vertex1 == vertex2) {
                    //print("Vertex: " + vertex1);
                    return true;
                }
            }
        }

        return false;
    }
}
