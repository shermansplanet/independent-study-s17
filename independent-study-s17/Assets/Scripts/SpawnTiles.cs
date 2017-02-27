using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTiles : MonoBehaviour {

	//this will clone the prefab object(s) and then place them at the designated locations

	public Transform[] spawnLocations;
	public GameObject[] baseTilePrefab;
	public GameObject[] cloneSpawn;

	static SpawnTiles instance;

	public static Dictionary<Vector3,GameObject> blocks;

	void Start(){
		blocks = new Dictionary<Vector3, GameObject> ();
		spawnTilesPlease();
		instance = this;
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

	public static Vector3 roundVector(Vector3 input){
		return new Vector3 (
			Mathf.Round (input.x),
			Mathf.Round (input.y),
			Mathf.Round (input.z));
	}

}
