﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour {

	const float spellSpeed = 1;

	public enum spell{NO_EFFECT,PUSH,DOUBLE_PUSH,CREATE_BLOCK,CREATE_PUSHBLOCK};
	private spell[] currentSpell = new spell[]{spell.PUSH,spell.CREATE_BLOCK};
	public Transform[] players;

	public GameObject[] selectors;
	private Material[] selectorMaterials;

	private float[] spellProgress = new float[]{0,0};

	// Use this for initialization
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
	
	// Update is called once per frame
	void Update () {
		for (int i = 0; i < players.Length; i++) {
			if (Input.GetButton("Spell"+i.ToString())) {
				if (spellProgress [i] >= 1) {
					spellProgress [i] = -1;
					Vector3 playerPos = getTile(players [i].position);
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
					if (currentSpell [i].Equals(spell.PUSH) && SpawnTiles.tileExists (spellPos)) {
						Spellable spellableBlock = SpawnTiles.blocks [SpawnTiles.roundVector (spellPos)].GetComponent<Spellable> ();
						if (spellableBlock != null) {
							if (otherPlayer == -1) {
								spellableBlock.ApplySpell (currentSpell [i], playerPos, Vector3.zero);
							} else {
								spellableBlock.ApplySpell (getSpellCombo(currentSpell [i],currentSpell[otherPlayer]), playerPos, getTile (players [otherPlayer].position));
							}
						}
					}
					//CREATE BLOCK/PUSHBLOCK
					else if (currentSpell [i].Equals(spell.CREATE_BLOCK)) {
						if (otherPlayer == -1) {
							GameObject tile = Instantiate (Resources.FindObjectsOfTypeAll(typeof(TileBehavior))[0], spellPos, Quaternion.Euler (0,0,0)) as GameObject;
							SpawnTiles.blocks.Add (spellPos, tile);
						} else {
							GameObject p = Instantiate (Resources.FindObjectsOfTypeAll(typeof(Pushblock))[0], spellPos, Quaternion.Euler (0,0,0)) as GameObject;
							SpawnTiles.blocks.Add (spellPos, p);
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

	spell getSpellCombo(spell spell1, spell spell2){
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
		}
		return spell.NO_EFFECT;
	}
}
