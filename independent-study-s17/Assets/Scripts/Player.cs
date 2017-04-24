using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

	public bool isPlayer1;
	public Transform selector;
	public Vector3 pos;
	public SpellDisplay spellDisplayObject;
	public Vector3 prevOffset = Vector3.zero;
	public Level currentLevel;
	public Player otherPlayer;

	private int inventorySize = 5;

	private SpellManager.spell currentSpell = SpellManager.spell.PUSH;

	//set default spells 
	private List<SpellManager.spell> spellInventory = new List<SpellManager.spell> {
		SpellManager.spell.PUSH,
		/*SpellManager.spell.CREATE_BLOCK,
		SpellManager.spell.CREATE_VOID,
		SpellManager.spell.CREATE_ICE,
		SpellManager.spell.RAISE*/
	};

	public void Respawn(){
		Level otherLevel = otherPlayer.currentLevel;
		Debug.Log (otherLevel.name);
		transform.position = (otherLevel.position + otherLevel.startBlocks [Random.Range (0,otherLevel.startBlocks.Count)]).ToVector() + Vector3.up * 2;
	}

	public void UpdateLevel(){
		foreach (Level l in WorldManager.levelsSpawned) {
			if (
				transform.position.x < l.position.x + l.max.x &&
				transform.position.x > l.position.x + l.min.x &&
				transform.position.z < l.position.z + l.max.z &&
				transform.position.z > l.position.z + l.min.z) {
				currentLevel = l;
				break;
			}
		}
	}

	public SpellManager.spell getCurrentSpell () {
		return currentSpell;
	}

	public void rotateSpell () {
		int currentIndex = spellInventory.IndexOf (currentSpell);
		if (currentIndex != -1) {
			//if last spell
			if (currentIndex == spellInventory.Count - 1) {
				currentSpell = spellInventory [0];
			} else {
				currentSpell = spellInventory [currentIndex + 1];
			}
			spellDisplayObject.UpdateText (currentSpell);
		}
	}

	public bool hasSpell(SpellManager.spell h) {
		return spellInventory.Contains (h);
	}

	public bool addSpell(SpellManager.spell s) {
		if (spellInventory.Contains (s)) {
			return false;
		} else if (spellInventory.Count < inventorySize) {
			spellInventory.Add (s);
			return true;
		} else {
			return false;
		}
	}

	public bool removeSpell(SpellManager.spell r) {
		if (spellInventory.Contains (r)) {
			spellInventory.Remove (r);
			return true;
		} else {
			return false;
		}
	}
}
