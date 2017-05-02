using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellDisplay : MonoBehaviour {

	public static readonly Dictionary<SpellManager.spell,string> spellNames = new Dictionary<SpellManager.spell, string>(){
		{SpellManager.spell.CREATE_BLOCK,"block"},
		{SpellManager.spell.PUSH,"push"},
		{SpellManager.spell.CREATE_VOID,"void"}
	};
	public Transform player;

	private Text text;

	void Start(){
		text = GetComponent<Text> ();
	}

	public void UpdateText(SpellManager.spell spell){
		CancelInvoke ();
		text.text = spellNames [spell];
		Invoke ("Clear", 1);
	}

	private void Clear(){
		text.text = "";
	}

	void Update () {
		transform.position = Camera.main.WorldToScreenPoint (player.position);
	}
}
