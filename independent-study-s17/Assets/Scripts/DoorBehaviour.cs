using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour {

	public bool passable = false;

	private Renderer r;

	void Start(){
		r = GetComponent<Renderer> ();
	}

	public void SetPassable(bool status){
		passable = status;
		if (!passable && SpawnTiles.tileIsFree (transform.position)) {
			SpawnTiles.blocks [SpawnTiles.roundVector (transform.position)] = gameObject;
			r.enabled = true;
		}
		if (!passable && SpawnTiles.tileExists (transform.position) && !SpawnTiles.tileHasPlayer(SpawnTiles.roundVector (transform.position)) && SpawnTiles.blocks[SpawnTiles.roundVector (transform.position)].GetComponent<WaterManager>()!=null) {
			Destroy (SpawnTiles.blocks [SpawnTiles.roundVector (transform.position)]);
			SpawnTiles.blocks [SpawnTiles.roundVector (transform.position)] = gameObject;
			r.enabled = true;
		}
		if (passable && SpawnTiles.tileExists (transform.position) &&
			SpawnTiles.blocks [SpawnTiles.roundVector (transform.position)] == gameObject) {

			SpawnTiles.blocks.Remove (SpawnTiles.roundVector (transform.position));
			r.enabled = false;
		}
	}
}
