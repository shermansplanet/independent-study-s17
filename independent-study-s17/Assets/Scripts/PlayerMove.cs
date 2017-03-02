using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

	//for two (or more) players
	List<Player> players = new List<Player>();

	void Start () {
		foreach(Player player in GameObject.FindObjectsOfType (typeof(Player))){
			players.Add (player);
		}
		//hopefully we can find a more elegent solution, but I just want to see if this helps
		if (!players [0].isPlayer1) {
			Player temp = players [0];
			players.Remove(players[0]);
			players.Add (temp);
		}
	}

	void Update () {
		for (int i = 0; i < players.Count; ++i) {
			Move.ObjectMove ("Vertical"+i.ToString(), "Horizontal"+i.ToString(), players [i]);
		}
	}
}
