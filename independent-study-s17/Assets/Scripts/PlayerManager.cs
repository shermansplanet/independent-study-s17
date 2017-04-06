using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
	
	public Player[] players;

	public static Player[] staticPlayers;

	void Start(){
		staticPlayers = players;
	}

	void Update () {
		for (int i = 0; i < players.Length; ++i) {

			Vector3 below = SpawnTiles.round2Vector (players[i].transform.position) + Vector3.down * 2;
			Debug.Log (below);
			//Debug.Log (SpawnTiles.blocks [below].GetComponent<IceManager> ());
			if (SpawnTiles.tileExists (below) && SpawnTiles.blocks [below].GetComponent<IceManager> () == null) {
				Move.ObjectMove ("Vertical" + i.ToString (), "Horizontal" + i.ToString (), players [i]);
			} else if (SpawnTiles.tileExists (below)) {
				Move.IceMove (players [i]);
			} else {
				//falling could go here
				break;
			}

			if (Input.GetButtonDown ("RotateSpell" + i.ToString())) {
				players [i].rotateSpell();
			}
		}
	}
}
