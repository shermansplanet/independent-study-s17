using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour {

	public double tick = 1.0;
	public double nextTime = 0.0;

	//remember we are in units of 2 (that's why all the random 2's)
	public int killPlane = -8;

	public GameObject waterBlock;

	public List<Player> players;

	void Update () {
		if (Time.time >= nextTime) {
			simulatePhysics ();
			nextTime += tick;
		}

		//Move player if in water
		foreach (Player p in players) {
			Vector3 currentTile = new Vector3 (
				Mathf.Round (p.transform.position.x/2),
				Mathf.Round (p.transform.position.y/2),
				Mathf.Round (p.transform.position.z/2))*2;
			if (SpawnTiles.tileExists (SpawnTiles.roundVector (currentTile)) &&
				SpawnTiles.blocks [SpawnTiles.roundVector (currentTile)].GetComponent<WaterManager> () != null) {
				Vector3 newPosition = p.transform.position + Move.WaterMove(SpawnTiles.blocks [SpawnTiles.roundVector (currentTile)].GetComponent<WaterManager> ());
				p.transform.position = newPosition;
			}
		}
	}

	void simulatePhysics(){

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
		//add water blocks in voids
		foreach (VoidManager v in voidblocks) {
			foreach (GameObject g in v.getAllObjects()) {
				if (g.GetComponent<WaterManager> () != null) {
					waterblocks.Add(g.GetComponent<WaterManager>());
				}
			}
		}

		foreach (WaterManager wtr in waterblocks) {
			Vector3 below = new Vector3 (wtr.transform.position.x, wtr.transform.position.y - 2, wtr.transform.position.z);
			Vector3 next = getNextWater (wtr);

			//flow down
			if (!SpawnTiles.tileExists (below) && below.y > killPlane) {
				generateWater (wtr, below, false);
			}
			//flow to next tile if free and not void, and there is not water below
			else if (!SpawnTiles.tileExists (next) &&
			         SpawnTiles.tileExists (below) &&
			         SpawnTiles.blocks [SpawnTiles.roundVector (below)].GetComponent<WaterManager> () == null) {
				generateWater (wtr, next, false);
			} 

			//flow into void block next
			else if (SpawnTiles.tileExists(next) && SpawnTiles.blocks [next].GetComponent<VoidManager> () != null) {
				SpawnTiles.blocks [next].GetComponent<VoidManager> ().addObject (generateWater(wtr, next, true));
			} 
			//flow into void block below
			else if (SpawnTiles.tileExists(below) && SpawnTiles.blocks [below].GetComponent<VoidManager> () != null) {
				SpawnTiles.blocks [below].GetComponent<VoidManager> ().addObject (generateWater(wtr, below, true));
			} 

			//push pushblock
			if (SpawnTiles.tileExists (next) &&
				SpawnTiles.blocks [SpawnTiles.roundVector (next)].GetComponent<Pushblock> () != null) {

				Pushblock p = SpawnTiles.blocks [SpawnTiles.roundVector (next)].GetComponent<Pushblock> ();
				Vector3 nextNext = getNextWaterPush (wtr.getDirection (), p);

				if (!SpawnTiles.tileExists (nextNext)) {
					SpawnTiles.blocks.Remove (SpawnTiles.roundVector (p.transform.position));
					p.transform.position = nextNext;
					SpawnTiles.blocks.Add (p.transform.position, p.gameObject);
				}
			}

			//stop water if parent is destoryed
			if (!wtr.isSource () && wtr.getParent () == null) {
				SpawnTiles.blocks.Remove (SpawnTiles.roundVector (wtr.transform.position));
				Destroy (wtr.gameObject);
			}
		}
	}
		
	//HELPER FUNCTIONS

	Vector3 getNextWater(WaterManager wtr) {
		Vector3 next = new Vector3(0,0,0);
		switch (wtr.getDirection()) {
		case 270:
			next = new Vector3 (wtr.transform.position.x + 2, wtr.transform.position.y, wtr.transform.position.z);
			break;
		case 180:
			next = new Vector3 (wtr.transform.position.x, wtr.transform.position.y, wtr.transform.position.z - 2);
			break;
		case 90:
			next = new Vector3 (wtr.transform.position.x - 2, wtr.transform.position.y, wtr.transform.position.z);
			break;
		case 0:
			next = new Vector3 (wtr.transform.position.x, wtr.transform.position.y, wtr.transform.position.z + 2);
			break;
		}
		return next;
	}

	Vector3 getNextWaterPush(int direction, Pushblock p) {
		Vector3 next = new Vector3(0,0,0);
		switch (direction) {
		case 270:
			next = new Vector3 (p.transform.position.x + 2, p.transform.position.y, p.transform.position.z);
			break;
		case 180:
			next = new Vector3 (p.transform.position.x, p.transform.position.y, p.transform.position.z - 2);
			break;
		case 90:
			next = new Vector3 (p.transform.position.x - 2, p.transform.position.y, p.transform.position.z);
			break;
		case 0:
			next = new Vector3 (p.transform.position.x, p.transform.position.y, p.transform.position.z + 2);
			break;
		}
		return next;
	}

	//instantiates water, adds to SpawnTiles.blocks, makes sure to match parent/type/direciton 
	GameObject generateWater(WaterManager wtr, Vector3 place, bool inVoid) {
		GameObject waterStream = Instantiate (waterBlock, place, Quaternion.Euler (0, 0, 0));
		if (!inVoid) {
			SpawnTiles.blocks.Add (place, waterStream);
		} else {
			waterStream.gameObject.GetComponent<MeshRenderer>().enabled = false;
		}
		waterStream.GetComponent<WaterManager> ().changeParent (wtr);
		if (waterStream.GetComponent<WaterManager> ().isSource ()) {
			waterStream.GetComponent<WaterManager> ().changeType ();
		}
		waterStream.GetComponent<WaterManager> ().changeDirection (wtr.getDirection ());
		return waterStream;
	}
}
