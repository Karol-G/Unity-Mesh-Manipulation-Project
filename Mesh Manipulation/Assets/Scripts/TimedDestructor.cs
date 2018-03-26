using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestructor : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(destroyAfterSeconds(15F));
    }

    IEnumerator destroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(this.gameObject);
    }
}
