using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public bool isPlayer1;
	public Transform selector;

	public int inventorySize = 3;
	private int currentIndex = 0;

	//set default spells 
	public List<SpellManager.spell> spellInventory = new List<SpellManager.spell> {
		SpellManager.spell.PUSH
	};
		
	public SpellManager.spell getCurrentSpell () {
		return spellInventory [currentIndex];
	}

	public void rotateSpell () {
		if (currentIndex < inventorySize - 1) {
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
