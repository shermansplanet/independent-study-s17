using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {

	public Transform[] players;

	private Vector3 offset;

	void Start(){
		offset = transform.position;
	}

	void Update () {
		Vector3 average = new Vector3 ();
		foreach (Transform p in players) {
			average += p.position;
		}
		average /= players.Length;
		transform.position = Vector3.Lerp (transform.position, average + offset, 0.05f);
	}
}
