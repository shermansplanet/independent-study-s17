using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour {

	public InputField textField;

	public void Remove(){
		Levelmaker.RemoveLevelLine (gameObject);
	}

	public string GetText(){
		return textField.text;
	}

	public void SetText(string s){
		textField.text = s;
	}
}
