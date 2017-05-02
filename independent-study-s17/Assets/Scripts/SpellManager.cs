using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour {

	const float spellSpeed = 1;

	public enum spell{NO_EFFECT,PUSH, PULL, DOUBLE_PUSH,CREATE_BLOCK,CREATE_PUSHBLOCK,CREATE_VOID,CREATE_RAMP,
						CREATE_ICE, CREATE_ICEBLOCK, REMOVE_ICE, FREEZE_WATER, ICE_ABOVE, WATER_ICE,
						RAISE, RAISE_PUSH, LOWER, DOUBLE_RAISE, CREATE_BLACKHOLE};
	//this is currently initialized this way for testing 
	private spell[] currentSpell = new spell[]{spell.CREATE_ICE,spell.RAISE};
	public Player[] players;
	public Texture[] spellTextures;

	public static Dictionary<spell,Texture> textureDict;

	public GameObject[] selectors;
	private Material[] selectorMaterials;

	//needed to instantiate new blocks
	public GameObject tile;
	public GameObject Pushblock;
	public GameObject Voidblock;
	public GameObject ramp;
	public GameObject BlackHole;

	public GameObject pushEffect;

	private float[] spellProgress = new float[]{0,0};

	void Start () {
		selectorMaterials = new Material[selectors.Length];
		for (int i = 0; i < players.Length; i++) {
			selectorMaterials [i] = selectors [i].GetComponent<MeshRenderer> ().sharedMaterial;
		}
		textureDict = new Dictionary<spell, Texture> {
			{spell.PUSH,spellTextures[0]},
			{spell.CREATE_BLOCK,spellTextures[1]},
			{spell.CREATE_VOID,spellTextures[2]},
			{spell.RAISE,spellTextures[3]},
			{spell.CREATE_ICE,spellTextures[4]},
		};
	}

	private Vector3 getTile(Vector3 pos){
		return new Vector3 (
			Mathf.Round (pos.x / 2),
			Mathf.Round (pos.y / 2),
			Mathf.Round (pos.z / 2)) * 2;
	}

	void Update () {
		if (WorldManager.inMenu)
			return;
		for (int i = 0; i < players.Length; i++) {
			//uncomment line below to make use of player inventory
			currentSpell [i] = players [i].getCurrentSpell ();
			if (Input.GetButton("Spell"+i.ToString())) {
				if (spellProgress [i] >= 1) {
					spellProgress [i] = -1;
					Vector3 playerPos = getTile(players [i].transform.position);
					int otherPlayer = -1;
					Vector3 spellPos = selectors [i].transform.position;
					Vector3 belowSpellPos = new Vector3 (spellPos.x, spellPos.y - 2, spellPos.z);
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

					if (currentSpell [i].Equals (spell.PUSH)) {
						Instantiate (pushEffect, playerPos, Quaternion.LookRotation (spellPos - playerPos)).transform.Translate (0, 0, -1);
						if (otherPlayer != -1) {
							Vector3 otherPlayerPos =  getTile(players [otherPlayer].transform.position);
							Instantiate (pushEffect, otherPlayerPos, Quaternion.LookRotation (spellPos - otherPlayerPos)).transform.Translate (0, 0, -1);
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
							//in case it was inside a void 
							MeshRenderer rend = spellableBlock.gameObject.GetComponent<MeshRenderer> ();
							if (rend == null) {
								rend = spellableBlock.GetComponent<Pushblock> ().graphics;
							}
							rend.enabled = true;
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
						if (SpawnTiles.tileExists (toMoveRight) && SpawnTiles.blocks[toMoveRight].GetComponent<Spellable>() != null) {
							moveAnyObject (toMoveRight, spellPos);
						} else if (SpawnTiles.tileExists (toMoveForward) && SpawnTiles.blocks[toMoveForward].GetComponent<Spellable>() != null) {
							moveAnyObject (toMoveForward, spellPos);
						} else if (SpawnTiles.tileExists (toMoveLeft) && SpawnTiles.blocks[toMoveLeft].GetComponent<Spellable>() != null) {
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
						} 
						//create ramp
						else if (!SpawnTiles.tileExists (spellPos) &&
						         SpawnTiles.tileExists (belowSpellPos) &&
						         getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.CREATE_RAMP)) {
							Vector3 dir = spellPos - players [i].pos;
							Debug.Log ("Ramp building");
							GameObject rampClone = Instantiate (ramp, spellPos, Quaternion.LookRotation (dir));
							rampClone.GetComponent<RampBehaviour> ().upSlopeDirection = spellPos - players [i].pos;
							SpawnTiles.blocks.Add (spellPos, rampClone);
						}
						//create Black Hole
						else if (getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.CREATE_BLACKHOLE)) {
							if (SpawnTiles.tileExists (spellPos)) {
								Destroy (SpawnTiles.blocks [spellPos].gameObject);
								SpawnTiles.blocks.Remove (spellPos);
							}
							GameObject bh = Instantiate(BlackHole, spellPos, Quaternion.Euler(0,0,0));
							SpawnTiles.blocks.Add(spellPos, bh);
						}
					}

					//ADD ICE
					else if (currentSpell [i].Equals (spell.CREATE_ICE)) {
						//add ice
						if (otherPlayer == -1 && SpawnTiles.tileExists (belowSpellPos) &&
							SpawnTiles.blocks [belowSpellPos].GetComponent<VoidManager> () == null &&
							SpawnTiles.blocks [belowSpellPos].GetComponent<WaterManager> () == null) {
							SpawnTiles.blocks [belowSpellPos].AddComponent<IceManager> ();
							SpawnTiles.blocks [belowSpellPos].GetComponent<IceManager> ().updateMaterial ();
						} 
						//ice above - note that it places ice on the top face of spellPos tile
						else if (SpawnTiles.tileExists (spellPos) && getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.ICE_ABOVE)) {
							GameObject above = SpawnTiles.blocks [spellPos];
							if (above.GetComponent<IceManager> () == null &&
							    above.GetComponent<VoidManager> () == null &&
							    above.GetComponent<WaterManager> () == null) {
								above.AddComponent<IceManager> ();
								above.GetComponent<IceManager> ().updateMaterial ();
							}
						}
						//remove ice
						else if (SpawnTiles.tileExists (belowSpellPos) && getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.REMOVE_ICE)) {
							IceManager active = SpawnTiles.blocks [belowSpellPos].GetComponent<IceManager> ();
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
							moveAnyObject (spellPos, aboveSpellPos);
						}
						//double raise
						else if (getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.DOUBLE_RAISE) &&
						         SpawnTiles.tileExists (spellPos) &&
						         !SpawnTiles.tileExists (aboveSpellPos + Vector3.up * 2)) {
							moveAnyObject (spellPos, aboveSpellPos + Vector3.up * 2);
						}
						//raise push
						else if (getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.RAISE_PUSH) &&
						         SpawnTiles.tileExists (spellPos)) {
							Vector3 upAndOver = new Vector3 ((spellPos.x - playerPos.x) + spellPos.x, spellPos.y + 2, (spellPos.z - playerPos.z) + spellPos.z);
							moveAnyObject (spellPos, upAndOver);
						}
						//lower
						else if (getSpellCombo (currentSpell [i], currentSpell [otherPlayer]).Equals (spell.LOWER) &&
						         SpawnTiles.tileExists (spellPos)) {
							moveAnyObject (spellPos, belowSpellPos);
						}
					}


				} else if (spellProgress [i] >= 0) {
					spellProgress [i] += spellSpeed * Time.deltaTime;
				}
			} else {
				spellProgress [i] = 0;
			}
			const float max = 0.7f;
			const float min = 0.03f;
			float c = Mathf.Clamp(max - (spellProgress[i] * (max - min)),min,max);
			selectorMaterials [i].SetFloat ("_Cutoff", c);
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
			case spell.CREATE_VOID:
				return spell.CREATE_BLACKHOLE;
			}
			break;
		case spell.CREATE_ICE:
			switch (spell2) {
			case spell.CREATE_VOID:
				return spell.REMOVE_ICE;
			case spell.RAISE:
				return spell.ICE_ABOVE;
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

	static void moveAnyObject(Vector3 oldPos, Vector3 newPos){
		if (SpawnTiles.tileExists(oldPos) && !SpawnTiles.tileExists(newPos)) {
			GameObject toMove = SpawnTiles.blocks [oldPos];
			SpawnTiles.blocks.Remove (oldPos);
			SpawnTiles.blocks.Add (newPos, toMove);
			toMove.transform.position = newPos;
		}
	}
}
