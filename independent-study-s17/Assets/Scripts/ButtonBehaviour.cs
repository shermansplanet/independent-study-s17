using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour {

	public List<DoorBehaviour> doors = new List<DoorBehaviour>();
	public Transform buttonObject;

	private Vector3 buttonPos;

	void Start(){
		buttonPos = buttonObject.position;
	}

	void Update(){
		Refresh ();
	}

	void Refresh(){
		bool pushed = !SpawnTiles.tileIsFree (transform.position);
		foreach (DoorBehaviour d in doors) {
			d.SetPassable (pushed);
		}

		buttonObject.position = pushed ? buttonPos + Vector3.down * 0.2f : buttonPos;
	}

}
