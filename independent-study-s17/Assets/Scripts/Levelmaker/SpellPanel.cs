using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellPanel : MonoBehaviour {

	List<string> spells = new List<string>();
	Color selectedColor = new Color(0.5f,1f,0.5f);

	public string GetSpells(){
		string s = "spells";
		foreach (string spell in spells) {
			s += "," + spell;
		}
		return s;
	}

	public void Remove(){
		Levelmaker.RemoveSpellLine (gameObject);
	}

	public void SetSpells(string s){
		s = s.Replace ("spells,", "");
		foreach (string spell in s.Split(',')) {
			spells.Add (spell);
			transform.FindChild (spell).GetComponent<Image> ().color = selectedColor;
		}
	}

	public void UpdateSpell(string spellname){
		Image clicked = transform.FindChild (spellname).GetComponent<Image>();
		if (spells.Contains (spellname)) {
			clicked.color = Color.white;
			spells.Remove (spellname);
		} else {
			clicked.color = selectedColor;
			spells.Add (spellname);
		}
	}

}
