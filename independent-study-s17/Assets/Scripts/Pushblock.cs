using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushblock : Spellable {
	
	public override void ApplySpell (SpellManager.spell spellType, Vector3 casterPosition) {
		if (spellType == SpellManager.spell.PUSH) {
			Vector3 newPosition = SpawnTiles.roundVector (transform.position * 2 - casterPosition);
			if (!SpawnTiles.tileExists (newPosition)) {
				SpawnTiles.blocks.Remove (SpawnTiles.roundVector (transform.position));
				SpawnTiles.blocks.Add (newPosition, gameObject);
				transform.position = newPosition;
			}
		}
	}

}
