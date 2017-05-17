using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushblockMove : MonoBehaviour {

	private Transform toFollow;

	// Use this for initialization
	void Start () {
		toFollow = transform.parent;
		transform.SetParent (toFollow.parent);
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (toFollow == null) {
			Destroy (gameObject);
			return;
		}
		float timeUntilTick = (float)PhysicsManager.nextTime - Time.time;
		Vector3 diff = toFollow.position - transform.position;
		if(diff.magnitude > 0.05f && timeUntilTick > 0.01f){
			transform.position += diff * Mathf.Clamp01(Time.deltaTime / timeUntilTick);
		}
	}
}
