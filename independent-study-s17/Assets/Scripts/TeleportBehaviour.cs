using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBehaviour : MonoBehaviour {
	public TeleportBehaviour other;
	public bool active = true;

	void Update(){
		foreach (Player p in PlayerManager.staticPlayers) {
			if (SpawnTiles.round2Vector (p.transform.position) != SpawnTiles.round2Vector (transform.position+Vector3.up)) {
				active = true;
				return;
			}
		}
		if (active) {
			foreach (Player p in PlayerManager.staticPlayers) {
				p.transform.position = other.transform.position + Vector3.up + p.offset () * 0.5f;
			}
			other.active = false;
		}
	}
}
