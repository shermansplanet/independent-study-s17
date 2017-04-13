﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour {

	const float spellSpeed = 1;

	public enum spell{NO_EFFECT,PUSH, PULL, DOUBLE_PUSH,CREATE_BLOCK,CREATE_PUSHBLOCK,CREATE_VOID,CREATE_RAMP,
						CREATE_ICE, CREATE_ICEBLOCK, REMOVE_ICE, FREEZE_WATER, FREEZE_MACHINE};
	//this is currently initialized this way for testing 
	private spell[] currentSpell = new spell[]{spell.CREATE_ICE,spell.CREATE_VOID};
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
			currentSpell [i] = players [i].getCurrentSpell ();
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
								if (g.GetComponent<Spellable> () != null) {
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
					else if (currentSpell [i].Equals (spell.CREATE_BLOCK) &&
					         (!SpawnTiles.tileExists (spellPos)) ||
					         (SpawnTiles.tileExists (spellPos) && SpawnTiles.blocks [spellPos].GetComponent<WaterManager> () != null)) {
						//destory current water block
						if (SpawnTiles.tileExists (spellPos)) {
							WaterManager currentWater = SpawnTiles.blocks [SpawnTiles.roundVector (spellPos)].GetComponent<WaterManager> ();
							Destroy (currentWater.gameObject);
							SpawnTiles.blocks.Remove (SpawnTiles.roundVector (spellPos));
						}
						//now create block
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
						    (!SpawnTiles.tileExists (SpawnTiles.roundVector (spellPos)) ||
						    (SpawnTiles.tileExists (SpawnTiles.roundVector (spellPos)) &&
						    (SpawnTiles.blocks [SpawnTiles.roundVector (spellPos)].GetComponent<VoidManager> () == null)))) {
							GameObject voidClone = Instantiate (Voidblock, spellPos, Quaternion.Euler (0, 0, 0));
							VoidManager v = voidClone.GetComponent<VoidManager> ();
							//add current object to void inventory if needed
							if (SpawnTiles.tileExists (spellPos)) {
								GameObject g = SpawnTiles.blocks [SpawnTiles.roundVector (spellPos)];
								g.GetComponent<MeshRenderer> ().enabled = false;
								v.addObject (g);
								SpawnTiles.blocks.Remove (spellPos);
							}
							SpawnTiles.blocks.Add (spellPos, voidClone);
						} else if (!SpawnTiles.tileExists (spellPos) && getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.CREATE_RAMP)) {
							Vector3 dir = spellPos - players [i].pos;
							GameObject rampClone = Instantiate (ramp, new Vector3 (spellPos.x, spellPos.y, spellPos.z),
								                       Quaternion.LookRotation (dir));
							rampClone.GetComponent<RampBehaviour> ().upSlopeDirection = spellPos - players [i].pos;
							SpawnTiles.blocks.Add (spellPos, rampClone);
						}
					}

					//ADD ICE
					else if (currentSpell [i].Equals (spell.CREATE_ICE)) {
						if (otherPlayer == -1 && SpawnTiles.tileExists (spellPos)) {
							SpawnTiles.blocks [spellPos].AddComponent<IceManager> ();
							SpawnTiles.blocks [spellPos].GetComponent<IceManager> ().updateMaterial ();
						} else if (SpawnTiles.tileExists (spellPos)) {
							IceManager active = SpawnTiles.blocks [spellPos].GetComponent<IceManager> ();
							if (active != null) {
								active.applyPastMaterial ();
								Destroy (active);
							}
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
			case spell.CREATE_VOID:
				return spell.PULL;
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
		case spell.CREATE_ICE:
			switch (spell2) {
			case spell.CREATE_VOID:
				return spell.REMOVE_ICE;
			}
			break;
		}
		return switched ? spell.NO_EFFECT : getSpellCombo(spell2,spell1,true);
	}

	float snapRotation(Vector3 r) {
		float x = r.x;
		float z = r.z;
		//Debug.Log (x);
		//Debug.Log (z);
		if (x < 0) {
			return 0;
		} else if (x > 0) {
			return 180;
		} else if (z > 0 ) {
			return 90;
		} else {
			return 270;
		}
	}
}
