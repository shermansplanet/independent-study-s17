using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public bool isPlayer1;
	public Transform selector;

	private int inventorySize = 2;
	private int currentIndex = 0;

	//set default spells 
	private List<SpellManager.spell> spellInventory = new List<SpellManager.spell> {
		SpellManager.spell.PUSH,
		SpellManager.spell.CREATE_BLOCK
	};

	public SpellManager.spell getCurrentSpell () {
		if (currentIndex < spellInventory.Count - 1) {
			return spellInventory [currentIndex];
		} else {
			return SpellManager.spell.NO_EFFECT;
		}
	}

	public void rotateSpell () {
		if (currentIndex < inventorySize) {
			currentIndex += 1;
		} else {
			currentIndex = 0;
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
