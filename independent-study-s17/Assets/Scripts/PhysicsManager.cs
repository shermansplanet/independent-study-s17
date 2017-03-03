using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour {

	public double tick = 1.0;
	public double nextTime = 0.0;



	void Update () {
		if (Time.time >= nextTime) {
			simulatePhysics ();
			nextTime += tick;
		}
	}

	void simulatePhysics(){


	}
}
