using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {

	static readonly Vector3 UP = new Vector3(1,0,1).normalized;
	static readonly Vector3 RIGHT = new Vector3(1,0,-1).normalized;
	const float RADIUS = 0.5f;
	static public float speed = 2;

	public static void ObjectMove(string axisV, string axisH, Player p){

		Vector2 translation = new Vector2(Input.GetAxis (axisV),Input.GetAxis(axisH));
		translation = translation.normalized * GetMagnitude (translation) * Time.deltaTime * speed;

		if (translation.magnitude > 0) {

			Vector3 positionOffset = UP * translation.x + RIGHT * translation.y;

			Vector3 currentTile = new Vector3 (
				Mathf.Round (p.transform.position.x/2),
				Mathf.Round (p.transform.position.y/2),
				Mathf.Round (p.transform.position.z/2))*2;

			p.transform.rotation = Quaternion.LookRotation (positionOffset);

			int normalizedX = positionOffset.x > 0 ? 2 : -2;
			int normalizedZ = positionOffset.z > 0 ? 2 : -2;

			Vector3 positionWithinTile = p.transform.position + positionOffset - currentTile;

			float distX = Mathf.Abs (normalizedX - positionWithinTile.x * 2) / Mathf.Abs (positionOffset.x);
			float distZ = Mathf.Abs (normalizedZ - positionWithinTile.z * 2) / Mathf.Abs (positionOffset.z);
			p.selector.position = currentTile + (distX < distZ ? new Vector3 (normalizedX, 0, 0) : new Vector3 (0, 0, normalizedZ));

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

			Vector3 newPosition = p.transform.position + positionOffset;

			p.transform.position = newPosition;
		}
	}

	static float GetMagnitude(Vector2 input){
		float deadZone = 0.1f;
		return Mathf.Clamp01(input.magnitude - deadZone) / (1 - deadZone);
	}
}
