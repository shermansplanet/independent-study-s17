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
	}

	void Update () {
		for (int i = 0; i < players.Count; ++i) {
			Move.ObjectMove ("Vertical" + (i+1).ToString(), "Horizontal"+ (i+1).ToString(), players [i]);
		}
	}
}
