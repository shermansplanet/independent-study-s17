using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTiles : MonoBehaviour {

	//this will clone the prefab object(s) and then place them at the designated locations

	public Transform[] spawnLocations;
	public GameObject[] baseTilePrefab;
	public GameObject[] cloneSpawn;

	void Start(){
		spawnTilesPlease();
	}

	void spawnTilesPlease(){
		cloneSpawn[0] = Instantiate(baseTilePrefab[0], spawnLocations[0].transform.position, Quaternion.Euler(0,0,0)) as GameObject;
	}

}
