using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
	
	public Player[] players;

	void Update () {
		for (int i = 0; i < players.Length; ++i) {
			Move.ObjectMove ("Vertical" + i.ToString (), "Horizontal" + i.ToString (), players [i]);

			if (Input.GetButtonDown ("RotateSpell" + i.ToString())) {
				players [i].rotateSpell();
			}
		}
	}
}
