using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameObjectAttributes : MonoBehaviour {

    private List<string> tagList = new List<string>();
    public List<GameObject> connectedGameObjectList = new List<GameObject>();
    public List<GameObject> potentialyConnectedGameObjectList = new List<GameObject>();
    public bool debug = false;

    void Update() {
        if (Input.GetKeyDown("u") && debug) {
            setVisible();
        }
    }

    private void setVisible() {
        foreach (GameObject gameObject in connectedGameObjectList) {
            gameObject.GetComponent<MeshRenderer>().enabled = !gameObject.GetComponent<MeshRenderer>().isVisible;
        }
    }

    public void setTagList(List<string> tagList) {
        this.tagList = tagList;
    }

    public void addTag(string tag) {
        tagList.Add(tag);
    }

    public List<string> getTagList() {
        return new List<string>(tagList);
    }

    public void addConnectedGameObject(GameObject gameObject) {
        connectedGameObjectList.Add(gameObject);
    }

    public List<GameObject> getConnectedGameObjectList() {
        return connectedGameObjectList;
    }

    public void removeConnectedGameObject(GameObject gameObject) {
        connectedGameObjectList.Remove(gameObject);
    }

    public void addPotentialyConnectedGameObject(GameObject gameObject) {
        potentialyConnectedGameObjectList.Add(gameObject);
    }

    public void addPotentialyConnectedGameObjectList(List<GameObject> gameObjectList) {
        potentialyConnectedGameObjectList.AddRange(gameObjectList);
    }

    public List<GameObject> getPotentialyConnectedGameObjectList() {
        return potentialyConnectedGameObjectList;
    }

    public void removePotentialyConnectedGameObject(GameObject gameObject) {
        potentialyConnectedGameObjectList.Remove(gameObject);
    }

    public void clearPotentialyConnectedGameObject() {
        potentialyConnectedGameObjectList.Clear();
    }

    void OnDestroy() {
        destroyGameObject();
    }

    private void destroyGameObject() {
        foreach (GameObject gameObject in connectedGameObjectList.Concat(potentialyConnectedGameObjectList)) {
            gameObject.GetComponent<GameObjectAttributes>().removeConnectedGameObject(this.gameObject);
            gameObject.GetComponent<GameObjectAttributes>().removePotentialyConnectedGameObject(this.gameObject);
        }
    }
}
