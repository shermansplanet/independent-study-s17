using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour {

	const float spellSpeed = 1;

	public enum spell{NO_EFFECT,PUSH,DOUBLE_PUSH,CREATE_BLOCK,CREATE_PUSHBLOCK,CREATE_VOID,CREATE_RAMP};
	//this is currently initialized this way for testing 
	private spell[] currentSpell = new spell[]{spell.PUSH,spell.CREATE_VOID};
	public Player[] players;

	public GameObject[] selectors;
	private Material[] selectorMaterials;

	//needed to instantiate new blocks
	public GameObject tile;
	public GameObject Pushblock;
	public GameObject Voidblock;
	public GameObject ramp;

	private float[] spellProgress = new float[]{0,0};

	void Start () {
		selectorMaterials = new Material[selectors.Length];
		for (int i = 0; i < players.Length; i++) {
			selectorMaterials [i] = selectors [i].GetComponent<MeshRenderer> ().material;
		}
	}

	private Vector3 getTile(Vector3 pos){
		return new Vector3 (
			Mathf.Round (pos.x / 2),
			Mathf.Round (pos.y / 2),
			Mathf.Round (pos.z / 2)) * 2;
	}

	void Update () {
		for (int i = 0; i < players.Length; i++) {
			//uncomment line below to make use of player inventory 
			//currentSpell [i] = players [i].getCurrentSpell ();
			if (Input.GetButton("Spell"+i.ToString())) {
				if (spellProgress [i] >= 1) {
					spellProgress [i] = -1;
					Vector3 playerPos = getTile(players [i].transform.position);
					int otherPlayer = -1;
					Vector3 spellPos = selectors [i].transform.position;
					for (int j = 0; j < players.Length; j++) {
						if (j == i)
							continue;
						Vector3 otherSpellPos = selectors [j].transform.position;
						if (otherSpellPos == spellPos && spellProgress[j] > 0) {
							otherPlayer = j;
							spellProgress [j] = -1;
							break;
						}
					}
					//PUSH and DOUBlE PUSH
					if (currentSpell [i].Equals (spell.PUSH) && SpawnTiles.tileExists (spellPos)) {
						Spellable spellableBlock = SpawnTiles.blocks [SpawnTiles.roundVector (spellPos)].GetComponent<Spellable> ();
						//should also handle void blocks
						VoidManager v = SpawnTiles.blocks [SpawnTiles.roundVector (spellPos)].GetComponent<VoidManager> ();
						if (v != null) {
							foreach (GameObject g in v.getAllObjects()) {
								if (g.GetComponent<Spellable>() != null) {
									spellableBlock = g.GetComponent<Spellable> ();
									break;
								}
							}
						}
						//back to your regullarly scheduled push...
						if (spellableBlock != null) {
							if (otherPlayer == -1) {
								spellableBlock.ApplySpell (currentSpell [i], playerPos, Vector3.zero);
							} else {
								spellableBlock.ApplySpell (getSpellCombo (currentSpell [i], currentSpell [otherPlayer]), playerPos, getTile (players [otherPlayer].transform.position));
							}
						}
					}
					//CREATE BLOCK/PUSHBLOCK
					else if (currentSpell [i].Equals (spell.CREATE_BLOCK) && !SpawnTiles.tileExists (spellPos)) {
						if (otherPlayer == -1) {
							GameObject tileClone = Instantiate (tile, spellPos, Quaternion.Euler (0, 0, 0));
							SpawnTiles.blocks.Add (spellPos, tileClone);
						} else if (getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.CREATE_PUSHBLOCK)) {
							GameObject pushblockClone = Instantiate (Pushblock, spellPos, Quaternion.Euler (0, 0, 0));
							SpawnTiles.blocks.Add (spellPos, pushblockClone);
						}
					} 
					//CREAT VOID/RAMP
					else if (currentSpell [i].Equals (spell.CREATE_VOID)) {
						//no void on top of another void
						if (otherPlayer == -1 &&
							(!SpawnTiles.tileExists(SpawnTiles.roundVector (spellPos)) ||
							(SpawnTiles.tileExists(SpawnTiles.roundVector (spellPos)) &&
									(SpawnTiles.blocks[SpawnTiles.roundVector (spellPos)].GetComponent<VoidManager>() == null)))) {
							GameObject voidClone = Instantiate (Voidblock, spellPos, Quaternion.Euler (0, 0, 0));
							VoidManager v = voidClone.GetComponent<VoidManager>();
							if (SpawnTiles.tileExists (spellPos)) {
								GameObject g = SpawnTiles.blocks [SpawnTiles.roundVector (spellPos)];
								g.GetComponent<MeshRenderer> ().enabled = false;
								v.addObject (g);
								SpawnTiles.blocks.Remove (spellPos);
							}
							SpawnTiles.blocks.Add (spellPos, voidClone);
						} else if (!SpawnTiles.tileExists(spellPos) && getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.CREATE_RAMP)) {
							float rotate = players [i].transform.rotation.y;
							Debug.Log (rotate);
							GameObject rampClone = Instantiate (ramp, new Vector3(spellPos.x,spellPos.y - 1,spellPos.z), Quaternion.Euler (-90, rotate, 0));
							SpawnTiles.blocks.Add (spellPos, rampClone);
						}
					}



				} else if (spellProgress [i] >= 0) {
					spellProgress [i] += spellSpeed * Time.deltaTime;
				}
			} else {
				spellProgress [i] = 0;
			}
			float c = Mathf.Clamp01 (spellProgress [i]) * 0.9f + 0.1f;
			selectorMaterials [i].SetColor ("_TintColor", new Color (c, c, c, 0.5f));
		}
	}

	spell getSpellCombo(spell spell1, spell spell2, bool switched = false){
		switch (spell1) {
		case spell.PUSH:
			switch (spell2) {
			case spell.PUSH:
				return spell.DOUBLE_PUSH;
			}
			break;
		case spell.CREATE_BLOCK:
			switch (spell2) {
			case spell.PUSH:
				return spell.CREATE_PUSHBLOCK;
			}
			break;
		case spell.CREATE_VOID:
			switch (spell2) {
			case spell.CREATE_BLOCK:
				return spell.CREATE_RAMP;
			}
			break;
		}
		return switched ? spell.NO_EFFECT : getSpellCombo(spell2,spell1,true);
	}
}
