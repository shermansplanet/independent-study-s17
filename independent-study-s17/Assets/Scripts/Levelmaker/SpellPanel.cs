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

	public void DoubleSpell(string spellname){
		Image clicked = transform.FindChild (spellname+"x2").GetComponent<Image>();
		int count = 0;
		foreach (string s in spells) {
			if (s.Equals (spellname)) {
				count++;
			}
		}
		if (count==2) {
			clicked.color = Color.white;
			spells.Remove (spellname);
		} else {
			clicked.color = selectedColor;
			spells.Add (spellname);
		}
	}

	public void UpdateSpell(string spellname){
		Image clicked = transform.FindChild (spellname).GetComponent<Image>();
		Image x2 = transform.FindChild (spellname+"x2").GetComponent<Image>();
		if (spells.Contains (spellname)) {
			clicked.color = Color.white;
			while(spells.Contains(spellname)){
				spells.Remove(spellname);
			}
			x2.color = Color.white;
			x2.GetComponent<Button> ().interactable = false;
		} else {
			clicked.color = selectedColor;
			spells.Add (spellname);
			x2.GetComponent<Button> ().interactable = true;
		}
	}

}
