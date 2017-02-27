using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

	readonly Vector3 UP = new Vector3(1,0,1).normalized;
	readonly Vector3 RIGHT = new Vector3(1,0,-1).normalized;
	const float RADIUS = 0.5f;

	public float speed = 2;
	public Transform selector;

	//for two players
	List<GameObject> players = new List<GameObject>();

	//I am assuming the players will always be in the scene?
	void Start () {
		foreach(GameObject player in GameObject.FindObjectsOfType (typeof(GameObject)))
		{
			if(player.tag == "Player" && !players.Contains(player))
				players.Add (player);
		}
	}

	void Update () {
		Move ("Vertical1", "Horizontal1", players [0]);

		if (players.Count > 1){
			Move ("Vertical2", "Horizontal2", players [1]);
		}

		/*Vector2 translationP1 = new Vector2(Input.GetAxis ("Vertical1"),Input.GetAxis("Horizontal1"));
		translationP1 = translationP1.normalized * GetMagnitude (translationP1) * Time.deltaTime * speed;

		if (translationP1.magnitude > 0) {

			Vector3 positionOffset = UP * translationP1.x + RIGHT * translationP1.y;

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

			if (!SpawnTiles.tileExists (currentTile + new Vector3 (normalizedX, -2, 0))
				|| SpawnTiles.tileExists (currentTile + new Vector3 (normalizedX, 0, 0))) {
				if (positionOffset.x > 0 && positionWithinTile.x > RADIUS) {
					positionOffset.x = 0;
				} else if (positionOffset.x < 0 && positionWithinTile.x < -RADIUS) {
					positionOffset.x = 0;
				}
			}
			if (!SpawnTiles.tileExists (currentTile + new Vector3 (0, -2, normalizedZ))
				|| SpawnTiles.tileExists (currentTile + new Vector3 (0, 0, normalizedZ))) {
				if (positionOffset.z > 0 && positionWithinTile.z > RADIUS) {
					positionOffset.z = 0;
				} else if (positionOffset.z < 0 && positionWithinTile.z < -RADIUS) {
					positionOffset.z = 0;
				}
			}

			Vector3 newPosition = transform.position + positionOffset;

			players[0].transform.position = newPosition; 
		}
		*/
	}

	float GetMagnitude(Vector2 input){
		float deadZone = 0.1f;
		return Mathf.Clamp01(input.magnitude - deadZone) / (1 - deadZone);
	}


	void Move(string axisV, string axisH, GameObject p){

		Vector2 translation = new Vector2(Input.GetAxis (axisV),Input.GetAxis(axisH));
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

			if (!SpawnTiles.tileExists (currentTile + new Vector3 (normalizedX, -2, 0))
				|| SpawnTiles.tileExists (currentTile + new Vector3 (normalizedX, 0, 0))) {
				if (positionOffset.x > 0 && positionWithinTile.x > RADIUS) {
					positionOffset.x = 0;
				} else if (positionOffset.x < 0 && positionWithinTile.x < -RADIUS) {
					positionOffset.x = 0;
				}
			}
			if (!SpawnTiles.tileExists (currentTile + new Vector3 (0, -2, normalizedZ))
				|| SpawnTiles.tileExists (currentTile + new Vector3 (0, 0, normalizedZ))) {
				if (positionOffset.z > 0 && positionWithinTile.z > RADIUS) {
					positionOffset.z = 0;
				} else if (positionOffset.z < 0 && positionWithinTile.z < -RADIUS) {
					positionOffset.z = 0;
				}
			}

			Vector3 newPosition = transform.position + positionOffset;

			p.transform.position = newPosition;
		}
	}
}
