using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScaler : MonoBehaviour {
	void Start () {
		transform.localScale *= Screen.height / 1200f;
	}
}
