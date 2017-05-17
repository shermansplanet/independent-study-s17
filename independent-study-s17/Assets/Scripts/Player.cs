using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

	public bool isPlayer1;
	public Transform selector;
	public Vector3 pos;
	public RawImage spellDisplayObject;
	public Vector3 prevOffset = Vector3.zero;
	public Level currentLevel;
	public Player otherPlayer;
	public GameObject SpellBindingPrefab;
	public GameObject selectorChild;

	private int inventorySize = 5;

	private SpellManager.spell currentSpell = SpellManager.spell.PUSH;

	//set default spells 
	private List<SpellManager.spell> spellInventory = new List<SpellManager.spell> {
		SpellManager.spell.PUSH,
		SpellManager.spell.CREATE_BLOCK,
		/*SpellManager.spell.CREATE_ICE,
		SpellManager.spell.RAISE*/
	};


	//active spell limit
	public const int activeSpellLimit = 10;
	private int activeSpellPoints = 0;
	private List<KeyValuePair<Vector3, int>> activeSpellObjects = new List<KeyValuePair<Vector3,int>>();
	private List<SpellBindingDisplay> activeSpellBindings = new List<SpellBindingDisplay>();

	public bool freeActiveSlots(){
		return activeSpellPoints < activeSpellLimit;
	}

	public int spellPoints(){
		return activeSpellPoints;
	}

	public void addActive(Vector3 pos, SpellManager.spell spell){
		int points = SpellManager.spellCosts [spell];
		activeSpellPoints += points;

		GameObject bindingObject = Instantiate (SpellBindingPrefab);
		bindingObject.transform.SetParent (spellDisplayObject.transform);
		bindingObject.transform.localScale = Vector3.one;
		SpellBindingDisplay bindingScript = bindingObject.GetComponent<SpellBindingDisplay> ();
		bindingScript.height = points;
		bindingScript.Init (spell);
		activeSpellBindings.Add (bindingScript);
		foreach (SpellBindingDisplay s in activeSpellBindings) {
			s.StartCoroutine (s.ShiftUp (points));
		}

		if (activeSpellPoints <= activeSpellLimit) {
			activeSpellObjects.Add (new KeyValuePair<Vector3, int>(pos, points));
		} else {
			while (activeSpellPoints > activeSpellLimit) {
				this.removeFirstActive();
			} activeSpellObjects.Add (new KeyValuePair<Vector3, int>(pos, points));
		}
	}

	public void removeFirstActive(){
		Destroy(SpawnTiles.blocks [activeSpellObjects [0].Key].gameObject);
		SpawnTiles.blocks.Remove (activeSpellObjects [0].Key);
		activeSpellPoints -= activeSpellObjects [0].Value;
		activeSpellObjects.RemoveAt (0);
		activeSpellBindings.RemoveAt (0);
	}
	//remove until x spaces are free
	public void removeFirstActiveConditional(int x){
		while (activeSpellLimit - activeSpellPoints < x) {
			this.removeFirstActive ();
		}
	}

	public void Respawn(Level otherLevel = null){
		if(otherLevel==null){
			otherLevel = otherPlayer.currentLevel;
		}
		transform.position = (otherLevel.position + otherLevel.startBlocks [Random.Range (0, otherLevel.startBlocks.Count)]).ToVector ()
		+ Vector3.up * 2
		+ offset ();
	}

	public Vector3 offset(){
		return isPlayer1 ? new Vector3 (0.5f, 0.0f, -0.5f) : new Vector3 (-0.5f, 0.0f, 0.5f);
	}

	public void UpdateLevel(){
		const int border = 4;
		foreach (Level l in WorldManager.levelsSpawned) {
			if (
				transform.position.x < l.position.x + l.max.x + border &&
				transform.position.x > l.position.x + l.min.x - border &&
				transform.position.z < l.position.z + l.max.z + border &&
				transform.position.z > l.position.z + l.min.z - border) {
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
			spellDisplayObject.texture = SpellManager.textureDict [currentSpell];
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
