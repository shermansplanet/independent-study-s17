using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushblock : Spellable {
	
	public override void ApplySpell (SpellManager.spell spellType, Vector3 casterPosition, Vector3 casterPosition2) {
		switch(spellType){
		case SpellManager.spell.PUSH:
			Vector3 newPosition = SpawnTiles.roundVector (transform.position * 2 - casterPosition);
			if (SpawnTiles.tileIsFree (newPosition) || (SpawnTiles.tileExists(newPosition) && SpawnTiles.blocks[newPosition].GetComponent<WaterManager>() != null)) {
				SpawnTiles.blocks.Remove (SpawnTiles.roundVector (transform.position));
				if (SpawnTiles.tileExists(newPosition)) {
					Destroy (SpawnTiles.blocks [newPosition]);
					SpawnTiles.blocks.Remove(newPosition);
				}
				SpawnTiles.blocks.Add (newPosition, gameObject);
				transform.position = newPosition;
			}
			break;
		case SpellManager.spell.DOUBLE_PUSH:
			if (SpawnTiles.roundVector (casterPosition-transform.position) == SpawnTiles.roundVector (transform.position - casterPosition2)) {
				SpawnTiles.blocks.Remove (SpawnTiles.roundVector (transform.position));
				Destroy (gameObject);
			} else {
				Vector3 midPosition = SpawnTiles.roundVector (transform.position * 2 - casterPosition);
				newPosition = SpawnTiles.roundVector (transform.position * 3 - casterPosition * 2);
				if (SpawnTiles.tileIsFree (midPosition)) {
					SpawnTiles.blocks.Remove (SpawnTiles.roundVector (transform.position));
					if (SpawnTiles.tileIsFree (newPosition) || (SpawnTiles.tileExists(newPosition) && SpawnTiles.blocks[newPosition].GetComponent<WaterManager>() != null)) {
						if (SpawnTiles.tileExists(newPosition)) {
							Destroy (SpawnTiles.blocks [newPosition]);
							SpawnTiles.blocks.Remove(newPosition);
						}
						SpawnTiles.blocks.Add (newPosition, gameObject);
						transform.position = newPosition;
					} else {
						SpawnTiles.blocks.Add (midPosition, gameObject);
						transform.position = midPosition;
					}
				}
			}
			break;
		}
	}

}
