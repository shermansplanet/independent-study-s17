using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour {

	const float spellSpeed = 1;

	public enum spell{PUSH};
	private spell[] currentSpell = new spell[]{spell.PUSH,spell.PUSH};
	public Transform[] players;

	public GameObject[] selectors;
	private Material[] selectorMaterials;

	private float[] spellProgress = new float[]{0,0};

	// Use this for initialization
	void Start () {
		selectorMaterials = new Material[selectors.Length];
		selectorMaterials [0] = selectors [0].GetComponent<MeshRenderer> ().material;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.Space)){
			if (spellProgress [0] >= 1) {
				spellProgress [0] = -1;
				Vector3 playerPos = players [0].position;
				playerPos = new Vector3 (
					Mathf.Round (playerPos.x/2),
					Mathf.Round (playerPos.y/2),
					Mathf.Round (playerPos.z/2))*2;
				if (SpawnTiles.tileExists (selectors[0].transform.position)) {
					Spellable spellableBlock = SpawnTiles.blocks [SpawnTiles.roundVector (selectors[0].transform.position)].GetComponent<Spellable>();
					if (spellableBlock != null) {
						spellableBlock.ApplySpell (currentSpell [0], playerPos);
					}
				}
			} else if(spellProgress[0] >= 0){
				spellProgress [0] += spellSpeed * Time.deltaTime;
			}
		} else {
			spellProgress [0] = 0;
		}
		float c = Mathf.Clamp01(spellProgress [0]) * 0.9f + 0.1f;
		selectorMaterials[0].SetColor("_TintColor", new Color (c,c,c,0.5f));
	}
}
