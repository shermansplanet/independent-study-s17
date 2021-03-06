using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTiles : MonoBehaviour {

	//this will clone the prefab object(s) and then place them at the designated locations

	public Transform[] spawnLocations;
	public GameObject[] baseTilePrefab;
	public GameObject[] cloneSpawn;
	public bool debug = true;

	static SpawnTiles instance;

	public static Dictionary<Vector3,GameObject> blocks;

	void Start(){
		if (debug) {
			blocks = new Dictionary<Vector3, GameObject> ();
			spawnTilesPlease ();
			instance = this;
		}
	}

	void spawnTilesPlease(){
		for (int i = 0; i < spawnLocations.Length; ++i) {
			GameObject tileInstance = Instantiate (baseTilePrefab [0], spawnLocations [i].transform.position, Quaternion.Euler (0, 0, 0)) as GameObject;
			blocks.Add (roundVector(spawnLocations [i].transform.position), tileInstance);
		}
		foreach (Spellable s in FindObjectsOfType<Spellable>()) {
			GameObject blockInstance = s.gameObject;
			blocks.Add (roundVector (blockInstance.transform.position), blockInstance);
		}
	}

	public static bool tileExists(Vector3 pos){
		return blocks.ContainsKey (roundVector (pos));
	}

	public static bool tileIsFree(Vector3 pos){
		pos = roundVector (pos);
		return !tileHasPlayer(pos) && !tileExists(pos);
	}

	public static bool tileHasPlayer(Vector3 pos){
		foreach (Player p in PlayerManager.staticPlayers) {
			if (roundVector(p.pos) == pos) {
				return true;
			}
		}
		return false;
	}

	public static Vector3 roundVector(Vector3 input){
		return new Vector3 (
			Mathf.Round (input.x),
			Mathf.Round (input.y),
			Mathf.Round (input.z));
	}

	//rounds the vector to nearest even 
	public static Vector3 round2Vector(Vector3 input){
		float x = Mathf.Round(input.x);
		float y = Mathf.Round(input.y);
		float z = Mathf.Round(input.z);

		if (x > 0) {
			if (x % 2 == 1) {
				x += 1;
			}
		} else {
			if (x % 2 == 1) {
				x -= 1;
			}
		}

		if (y > 0) {
			if (y % 2 == 1) {
				y += 1;
			}
		} else {
			if (y % 2 == 1) {
				y -= 1;
			}
		}

		if (z > 0) {
			if (z % 2 == 1) {
				z += 1;
			}
		} else {
			if (z % 2 == 1) {
				z -= 1;
			}
		}
		return new Vector3 (x, y, z);
	}
}
