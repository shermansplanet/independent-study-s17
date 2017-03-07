using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {
	
	public Player[] players;

	void Update () {
		for (int i = 0; i < players.Length; ++i) {
			Move.ObjectMove ("Vertical"+i.ToString(), "Horizontal"+i.ToString(), players [i]);
			float rotate = Input.GetAxis ("RotateSpell" + i.ToString ());
			//Debug.Log (rotate);
			if (rotate > 0) {
				players [i].rotateSpell();
				Debug.Log (players [i].getCurrentSpell ());
			}
		}
	}
}
