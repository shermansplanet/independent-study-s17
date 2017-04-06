using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceManager : MonoBehaviour {

	private Material pastMaterial = null;

	public void updateMaterial(){
		pastMaterial = GetComponent<MeshRenderer> ().sharedMaterial;
		Material iceMaterial = Resources.Load("Materials/Ice", typeof(Material)) as Material;
		gameObject.GetComponent<MeshRenderer> ().sharedMaterial = iceMaterial;
	}

	public void applyPastMaterial() {
		if (pastMaterial != null) {
			GetComponent<MeshRenderer> ().sharedMaterial = pastMaterial;
		}
	}
}
