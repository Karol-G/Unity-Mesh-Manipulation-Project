using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectAttributes : MonoBehaviour {

    public List<string> tagList = new List<string>();

    public void setTagList(List<string> tagList) {
        this.tagList = tagList;
    }

    public void addTag(string tag) {
        tagList.Add(tag);
    }

    public List<string> getTagList() {
        return new List<string>(tagList);
    }
}
