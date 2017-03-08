using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public bool isPlayer1;
	public Transform selector;

	private int inventorySize = 3;

	private SpellManager.spell currentSpell = SpellManager.spell.PUSH;

	//set default spells 
	private List<SpellManager.spell> spellInventory = new List<SpellManager.spell> {
		SpellManager.spell.PUSH,
		SpellManager.spell.CREATE_BLOCK
	};

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
