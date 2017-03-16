﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour {

	public double tick = 1.0;
	public double nextTime = 0.0;

	//remember we are in units of 2 (that's why all the random 2's)
	public int killPlane = -8;

	public GameObject waterBlock;

	void Update () {
		if (Time.time >= nextTime) {
			simulatePhysics ();
			nextTime += tick;
		}
	}

	void simulatePhysics(){

		//putting this crappy annoying ridiculous code here becuase it destroys sanity
		//just take a look at the water logic, I mean just look at it
		//and yes I wrote this code at 4:30 am don't judge me
		List<VoidManager> voidblocks = new List<VoidManager>();

		foreach (GameObject v in GameObject.FindGameObjectsWithTag("Void")) {
			if (v != null) {
				voidblocks.Add (v.GetComponent<VoidManager> ());
			}
		}
			
		//Pushblock physics
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

		//Water physics
		List<WaterManager> waterblocks = new List<WaterManager>();

		foreach (GameObject w in GameObject.FindGameObjectsWithTag("Water")) {
			if (w != null) {
				waterblocks.Add (w.GetComponent<WaterManager> ());
			}
		}

		foreach (WaterManager wtr in waterblocks) {
			Vector3 below = new Vector3 (wtr.transform.position.x, wtr.transform.position.y - 2, wtr.transform.position.z);
			Vector3 next = new Vector3 (0,0,0);
			next = getNextWater (wtr);
			Debug.Log (next);
			//flow down
			if (!SpawnTiles.tileExists (below) && below.y > killPlane) {
				GameObject waterStream = Instantiate (waterBlock, below, Quaternion.Euler (0,0,0));
				SpawnTiles.blocks.Add (below, waterStream);
				waterStream.GetComponent<WaterManager> ().changeParent (wtr);
				if (waterStream.GetComponent<WaterManager> ().isSource ()) {
					waterStream.GetComponent<WaterManager> ().changeType ();
				}
				waterStream.GetComponent<WaterManager> ().changeDirection (wtr.getDirection ());
			}
				
			//flow to next tile if free or void (that doesn't already contain water), and there is not water below
			else if ((!SpawnTiles.tileExists (next) ||
				(SpawnTiles.blocks[SpawnTiles.roundVector(next)].GetComponent<VoidManager>() != null &&
					!SpawnTiles.blocks[SpawnTiles.roundVector(next)].GetComponent<VoidManager>().hasObject(wtr.gameObject))) && 
				SpawnTiles.tileExists (below) && 
				SpawnTiles.blocks[SpawnTiles.roundVector (below)].GetComponent<WaterManager>() == null) {
				//add water to void if there is a void
				GameObject waterStream = null;
				if (SpawnTiles.tileExists(next) && SpawnTiles.blocks [SpawnTiles.roundVector (next)].GetComponent<VoidManager> () != null) {
					waterStream = Instantiate (waterBlock, next, Quaternion.Euler (0, 0, 0));
					waterStream.transform.localScale = new Vector3 (0, 0, 0);
					SpawnTiles.blocks [SpawnTiles.roundVector (next)].GetComponent<VoidManager> ().addObject (waterStream);
				} else {
					waterStream = Instantiate (waterBlock, next, Quaternion.Euler (0, 0, 0));
				}
				SpawnTiles.blocks.Add (next, waterStream);
				waterStream.GetComponent<WaterManager> ().changeParent (wtr);
				if (waterStream.GetComponent<WaterManager> ().isSource ()) {
					waterStream.GetComponent<WaterManager> ().changeType ();
				}
				waterStream.GetComponent<WaterManager> ().changeDirection (wtr.getDirection ());
			}

			//push pushblock
			if (SpawnTiles.tileExists (next) &&
			    SpawnTiles.blocks [SpawnTiles.roundVector (next)].GetComponent<Pushblock> () != null) {
				Pushblock p = SpawnTiles.blocks [SpawnTiles.roundVector (next)].GetComponent<Pushblock> ();
				SpawnTiles.blocks.Remove (SpawnTiles.roundVector (p.transform.position));


				p.transform.position = getNextWaterPush (wtr.getDirection (), p);

				SpawnTiles.blocks.Add (p.transform.position, GameObject.FindGameObjectWithTag("Pushblock"));
			}

			//destroy water if too low
			if (wtr.transform.position.y < killPlane) {
				SpawnTiles.blocks.Remove (SpawnTiles.roundVector (wtr.transform.position));
				Destroy (wtr.gameObject);
			}

			//stop water if parent is destoryed
			if (!wtr.isSource () && wtr.getParent () == null) {
				SpawnTiles.blocks.Remove (SpawnTiles.roundVector (wtr.transform.position));
				Destroy (wtr.gameObject);
			}
		}
	}

	Vector3 getNextWater(WaterManager wtr) {
		Vector3 next = new Vector3(0,0,0);
		switch (wtr.getDirection()) {
		case 0:
			next = new Vector3 (wtr.transform.position.x + 2, wtr.transform.position.y, wtr.transform.position.z);
			break;
		case 90:
			next = new Vector3 (wtr.transform.position.x, wtr.transform.position.y, wtr.transform.position.z - 2);
			break;
		case 180:
			next = new Vector3 (wtr.transform.position.x - 2, wtr.transform.position.y, wtr.transform.position.z);
			break;
		case 270:
			next = new Vector3 (wtr.transform.position.x, wtr.transform.position.y, wtr.transform.position.z + 2);
			break;
		}
		return next;
	}

	Vector3 getNextWaterPush(int direction, Pushblock p) {
		Vector3 next = new Vector3(0,0,0);
		switch (direction) {
		case 0:
			next = new Vector3 (p.transform.position.x + 2, p.transform.position.y, p.transform.position.z);
			break;
		case 90:
			next = new Vector3 (p.transform.position.x, p.transform.position.y, p.transform.position.z - 2);
			break;
		case 180:
			next = new Vector3 (p.transform.position.x - 2, p.transform.position.y, p.transform.position.z);
			break;
		case 270:
			next = new Vector3 (p.transform.position.x, p.transform.position.y, p.transform.position.z + 2);
			break;
		}
		return next;
	}
}
