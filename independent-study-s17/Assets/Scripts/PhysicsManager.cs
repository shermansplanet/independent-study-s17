using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour {

	public double tick = 1.0;
	public double nextTime = 0.0;

	//remember we are in units of 2 (that's why all the random 2's)
	public int killPlane = -8;

	void Update () {
		if (Time.time >= nextTime) {
			simulatePhysics ();
			nextTime += tick;
		}
	}

	void simulatePhysics(){

		List<Pushblock> pushables =  new List<Pushblock>();

		foreach (GameObject x in GameObject.FindGameObjectsWithTag("Pushblock")) {
			if (x != null) {
				pushables.Add (x.GetComponent<Pushblock> ());
			}
		}

		foreach (Pushblock p in pushables) {
			Vector3 below = new Vector3 (p.transform.position.x, p.transform.position.y - 2, p.transform.position.z);
			if (!SpawnTiles.tileExists (below)) {
				SpawnTiles.blocks.Remove (SpawnTiles.roundVector (p.transform.position));
				p.transform.Translate(Vector3.down*2);
				SpawnTiles.blocks.Add (below, GameObject.FindGameObjectWithTag("Pushblock"));
			}
			if (p.transform.position.y < killPlane) {
				SpawnTiles.blocks.Remove (SpawnTiles.roundVector (p.transform.position));
				Destroy (p.gameObject);
			}
		}
	}
}
