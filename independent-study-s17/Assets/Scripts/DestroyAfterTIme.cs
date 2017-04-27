using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTIme : MonoBehaviour {

	public float delay;

	void Start () {
		Invoke ("Kill", delay);
	}

	void Kill(){
		Destroy (gameObject);
	}
}
