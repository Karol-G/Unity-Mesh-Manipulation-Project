using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestructor : MonoBehaviour {

    public float timeTillDestruction = 10F;

	// Use this for initialization
	void Start () {
        StartCoroutine(destroyAfterSeconds(timeTillDestruction));
    }

    IEnumerator destroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(this.gameObject);
    }
}
