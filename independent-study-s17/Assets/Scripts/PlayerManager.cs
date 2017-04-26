using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
	
	public Player[] players;

	public static Player[] staticPlayers;

	void Start(){
		staticPlayers = players;
		players [0].otherPlayer = players [1];
		players [1].otherPlayer = players [0];
	}

	void Update () {
		if (WorldManager.inMenu)
			return;
		
		for (int i = 0; i < players.Length; ++i) {

			Move.ObjectMove ("Vertical" + i.ToString (), "Horizontal" + i.ToString (), players [i]);

			players [i].UpdateLevel ();

			if (Input.GetButtonDown ("RotateSpell" + i.ToString())) {
				players [i].rotateSpell();
			}
		}
	}
}
