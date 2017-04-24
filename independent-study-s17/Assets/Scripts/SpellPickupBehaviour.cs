using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellPickupBehaviour : MonoBehaviour {

	public SpellManager.spell spell;

	private void OnTriggerEnter(Collider other){
		if (other.CompareTag ("Player")) {
			if (other.GetComponent<Player> ().addSpell (spell)) {
				Destroy (gameObject);
			}
		}
	}

}
