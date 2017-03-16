using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidManager : MonoBehaviour {
	
	private List<GameObject> inventory = new List<GameObject>();

	public void addObject(GameObject g) {
		inventory.Add (g);
	}

	public GameObject getFirstObject() {
		if (inventory.Count > 0) {
			return inventory [0];
		} else {
			return null;
		}
	}

	public void removeObject(GameObject g) {
		if (inventory.Contains (g)) {
			inventory.Remove (g);
		}
	}

	public bool hasObject(GameObject g) {
		return inventory.Contains (g);
	}

	public List<GameObject> getAllObjects() {
		return inventory;
	}
}