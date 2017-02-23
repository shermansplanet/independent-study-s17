using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

	readonly Vector3 UP = new Vector3(1,0,1).normalized;
	readonly Vector3 RIGHT = new Vector3(1,0,-1).normalized;
	const float RADIUS = 0.5f;

	public float speed = 2;
	public Transform selector;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 translation = new Vector2(Input.GetAxis ("Vertical"),Input.GetAxis("Horizontal"));
		translation = translation.normalized * GetMagnitude (translation) * Time.deltaTime * speed;
		if (translation.magnitude > 0) {

			Vector3 positionOffset = UP * translation.x + RIGHT * translation.y;

			Vector3 currentTile = new Vector3 (
				                      Mathf.Round (transform.position.x/2),
				                      Mathf.Round (transform.position.y/2),
				                      Mathf.Round (transform.position.z/2))*2;

			transform.rotation = Quaternion.LookRotation (positionOffset);

			int normalizedX = positionOffset.x > 0 ? 2 : -2;
			int normalizedZ = positionOffset.z > 0 ? 2 : -2;

			Vector3 positionWithinTile = transform.position + positionOffset - currentTile;

			float distX = Mathf.Abs (normalizedX - positionWithinTile.x * 2) / Mathf.Abs (positionOffset.x);
			float distZ = Mathf.Abs (normalizedZ - positionWithinTile.z * 2) / Mathf.Abs (positionOffset.z);
			selector.position = currentTile + (distX < distZ ? new Vector3 (normalizedX, 0, 0) : new Vector3 (0, 0, normalizedZ));

			if (!SpawnTiles.tileExists (currentTile + new Vector3 (normalizedX, -2, 0))) {
				if (positionOffset.x > 0 && positionWithinTile.x > RADIUS) {
					positionOffset.x = 0;
				} else if (positionOffset.x < 0 && positionWithinTile.x < -RADIUS) {
					positionOffset.x = 0;
				}
			}
			if (!SpawnTiles.tileExists (currentTile + new Vector3 (0, -2, normalizedZ))) {
				if (positionOffset.z > 0 && positionWithinTile.z > RADIUS) {
					positionOffset.z = 0;
				} else if (positionOffset.z < 0 && positionWithinTile.z < -RADIUS) {
					positionOffset.z = 0;
				}
			}

			Vector3 newPosition = transform.position + positionOffset;

			transform.position = newPosition;
		}
	}

	float GetMagnitude(Vector2 input){
		float deadZone = 0.1f;
		return Mathf.Clamp01(input.magnitude - deadZone) / (1 - deadZone);
	}
}
