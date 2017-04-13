﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour {

	const float spellSpeed = 1;

	public enum spell{NO_EFFECT,PUSH, PULL, DOUBLE_PUSH,CREATE_BLOCK,CREATE_PUSHBLOCK,CREATE_VOID,CREATE_RAMP,
						CREATE_ICE, CREATE_ICEBLOCK, REMOVE_ICE, FREEZE_WATER, ICE_ABOVE, WATER_ICE,
						RAISE, RAISE_PUSH, LOWER, DOUBLE_RAISE};
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
					Vector3 aboveSpellPos = new Vector3 (spellPos.x, spellPos.y + 2, spellPos.z);
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
					//PULL
					else if (currentSpell [i].Equals(spell.PUSH) &&
						otherPlayer != -1 &&
						getSpellCombo(currentSpell[i], currentSpell[otherPlayer]).Equals(spell.PULL) &&
						SpawnTiles.tileIsFree(spellPos)) {
						Vector3 toMoveRight = spellPos + Vector3.left * 2;
						Vector3 toMoveForward = spellPos + Vector3.forward * 2;
						Vector3 toMoveLeft = spellPos + Vector3.left * 2;
						if (SpawnTiles.tileExists (toMoveRight)) {
							moveAnyObject (toMoveRight, spellPos);
						} else if (SpawnTiles.tileExists (toMoveForward)) {
							moveAnyObject (toMoveForward, spellPos);
						} else if (SpawnTiles.tileExists (toMoveLeft)) {
							moveAnyObject (toMoveLeft, spellPos);
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
						} else if (!SpawnTiles.tileExists (spellPos) &&
						           !SpawnTiles.tileExists (new Vector3 (spellPos.x, spellPos.y - 2, spellPos.z)) &&
						           getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.CREATE_RAMP)) {
							Vector3 dir = spellPos - players [i].pos;
							GameObject rampClone = Instantiate (ramp, new Vector3 (spellPos.x, spellPos.y, spellPos.z),
								                       Quaternion.LookRotation (dir));
							rampClone.GetComponent<RampBehaviour> ().upSlopeDirection = spellPos - players [i].pos;
							SpawnTiles.blocks.Add (spellPos, rampClone);
						}
					}

					//ADD ICE
					else if (currentSpell [i].Equals (spell.CREATE_ICE)) {
						//add ice
						if (otherPlayer == -1 && SpawnTiles.tileExists (spellPos) &&
						    SpawnTiles.blocks [spellPos].GetComponent<VoidManager> () == null &&
						    SpawnTiles.blocks [spellPos].GetComponent<WaterManager> () == null) {
							SpawnTiles.blocks [spellPos].AddComponent<IceManager> ();
							SpawnTiles.blocks [spellPos].GetComponent<IceManager> ().updateMaterial ();
						} 
						//ice above
						else if (SpawnTiles.tileExists (aboveSpellPos) && getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.ICE_ABOVE)) {
							GameObject above = SpawnTiles.blocks [aboveSpellPos];
							if (above.GetComponent<IceManager> () == null &&
							    above.GetComponent<VoidManager> () == null &&
							    above.GetComponent<WaterManager> () == null) {
								above.AddComponent<IceManager> ();
								above.GetComponent<IceManager> ().updateMaterial ();
							}
						}
						//remove ice
						else if (SpawnTiles.tileExists (spellPos) && getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.REMOVE_ICE)) {
							IceManager active = SpawnTiles.blocks [spellPos].GetComponent<IceManager> ();
							if (active != null) {
								active.applyPastMaterial ();
								Destroy (active);
							}
						}
					}

					//Raise
					else if (currentSpell [i].Equals (spell.RAISE)) {
						//raise block
						if (otherPlayer == -1 && SpawnTiles.tileExists (spellPos) && 
							!SpawnTiles.tileExists (aboveSpellPos)) {
							GameObject toMove = SpawnTiles.blocks [spellPos];
							SpawnTiles.blocks.Remove (spellPos);
							SpawnTiles.blocks.Add (aboveSpellPos, toMove);
							toMove.transform.Translate (Vector3.up * 2);
						}
						//double raise
						else if (getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.DOUBLE_RAISE) &&
							SpawnTiles.tileExists(spellPos) &&
							!SpawnTiles.tileExists(new Vector3(spellPos.x, spellPos.y + 4, spellPos.z))) {
							GameObject toMove = SpawnTiles.blocks [spellPos];
							SpawnTiles.blocks.Remove (spellPos);
							SpawnTiles.blocks.Add (new Vector3(spellPos.x, spellPos.y + 4, spellPos.z), toMove);
							toMove.transform.Translate (Vector3.up * 4);
						}
						//raise push
						else if (getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.RAISE_PUSH) &&
							SpawnTiles.tileExists(spellPos)) {
							GameObject toMove = SpawnTiles.blocks [spellPos];
							Vector3 upAndOver = aboveSpellPos - players [i].pos;;
							SpawnTiles.blocks.Remove (spellPos);
							SpawnTiles.blocks.Add (upAndOver, toMove);
							toMove.transform.position = upAndOver;
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
		case spell.RAISE:
			switch (spell2) {
			case spell.CREATE_VOID:
				return spell.LOWER;
			case spell.RAISE:
				return spell.DOUBLE_RAISE;
			case spell.PUSH:
				return spell.RAISE_PUSH;
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

	void moveAnyObject(Vector3 oldPos, Vector3 newPos){
		if (SpawnTiles.tileExists(oldPos) && !SpawnTiles.tileExists(newPos)) {
			GameObject toMove = SpawnTiles.blocks [oldPos];
			SpawnTiles.blocks.Remove (oldPos);
			SpawnTiles.blocks.Add (newPos, toMove);
			toMove.transform.position = newPos;
		}
	}
}
