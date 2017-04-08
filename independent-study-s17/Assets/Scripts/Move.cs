using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {

	static readonly Vector3 UP = new Vector3(1,0,1).normalized;
	static readonly Vector3 RIGHT = new Vector3(1,0,-1).normalized;
	const float RADIUS = 0.5f;
	const bool PLAYER_CAN_FALL = true;
	static public float speed = 4;

	public static void ObjectMove(string axisV, string axisH, Player p){

		Vector2 translation = new Vector2(Input.GetAxis (axisV),Input.GetAxis(axisH));
		translation = translation.normalized * GetMagnitude (translation) * Time.deltaTime * speed;

		Vector3 positionOffset = UP * translation.x + RIGHT * translation.y;

		Vector3 waterOffset = new Vector3 (0, 0, 0);

		Vector3 currentTile = new Vector3 (
			Mathf.Round (p.transform.position.x/2),
			Mathf.Round (p.transform.position.y/2),
			Mathf.Round (p.transform.position.z/2))*2;

		RampBehaviour ramp = SpawnTiles.tileExists(currentTile) ? SpawnTiles.blocks [currentTile].GetComponent<RampBehaviour>() : null;

		Vector3 tileBelow = currentTile + Vector3.down * 2;

		if (ramp == null && SpawnTiles.tileExists(tileBelow)) {
			ramp = SpawnTiles.blocks [tileBelow].GetComponent<RampBehaviour> ();
			if (ramp != null) {
				currentTile = tileBelow;
			}
		}

		p.pos = currentTile;

		p.transform.rotation = Quaternion.LookRotation (positionOffset);

		if (!SpawnTiles.tileExists (tileBelow)) {
			positionOffset = Vector3.down * Time.deltaTime * 6;
		}

		int normalizedX = positionOffset.x > 0 ? 2 : -2;
		int normalizedZ = positionOffset.z > 0 ? 2 : -2;

		Vector3 positionWithinTile = p.transform.position + positionOffset - currentTile;

		float distX = Mathf.Abs (normalizedX - positionWithinTile.x * 2) / Mathf.Abs (positionOffset.x);
		float distZ = Mathf.Abs (normalizedZ - positionWithinTile.z * 2) / Mathf.Abs (positionOffset.z);
		p.selector.position = currentTile + (distX < distZ ? new Vector3 (normalizedX, 0, 0) : new Vector3 (0, 0, normalizedZ));

		bool exceedingBoundaryX = (positionOffset.x > 0 && positionWithinTile.x > RADIUS) || (positionOffset.x < 0 && positionWithinTile.x < -RADIUS);
		bool exceedingBoundaryZ = (positionOffset.z > 0 && positionWithinTile.z > RADIUS) || (positionOffset.z < 0 && positionWithinTile.z < -RADIUS);
		bool onEdge = false;

		Vector3 rampDirection = ramp == null ? Vector3.zero : ramp.upSlopeDirection;

		if (movementBlocked(currentTile,new Vector3 (normalizedX, 0, 0),rampDirection)) {
			if (exceedingBoundaryX) {
				positionOffset.x = 0;
				onEdge = true;
			}
		}
		if (movementBlocked(currentTile,new Vector3 (0, 0, normalizedZ),rampDirection)) {
			if (exceedingBoundaryZ) {
				positionOffset.z = 0;
				onEdge = true;
			}
		}

		if (movementBlocked(currentTile,new Vector3 (normalizedX, 0, normalizedZ),rampDirection)) {
			if (exceedingBoundaryX && exceedingBoundaryZ && !onEdge) {
				positionOffset.x = 0;
				positionOffset.z = 0;
			}
		}


		//acount for water
		if (SpawnTiles.tileExists (currentTile) &&
		    SpawnTiles.blocks [SpawnTiles.roundVector (currentTile)].GetComponent<WaterManager> () != null) {

			//WaterManager w = SpawnTiles.blocks [SpawnTiles.roundVector (currentTile)].GetComponent<WaterManager> ();
			//waterOffset = waterDirection (w);
		}

		float rampOffset = ramp == null ? 0 : mul((positionWithinTile + ramp.upSlopeDirection * 0.5f), ramp.upSlopeDirection).magnitude * 0.5f;

		Vector3 newPosition = p.transform.position + positionOffset + waterOffset;

		if (ramp == null) {
		} else {
			newPosition.y = currentTile.y + rampOffset;
		}

		p.transform.position = newPosition;

		if (p.transform.position.y < -10) {
			p.transform.position = new Vector3 (0, 2, 0);
		}
	}

	private static Vector3 mul(Vector3 a, Vector3 b){
		return new Vector3 (
			a.x * b.x,
			a.y * b.y,
			a.z * b.z
		);
	}

	private static bool movementBlocked(Vector3 currentTile, Vector3 offset, Vector3 currentRampDirection){
		if (currentRampDirection == offset) {
			currentTile += Vector3.up * 2;
		}
		GameObject block = SpawnTiles.tileExists (currentTile + offset) ? SpawnTiles.blocks [SpawnTiles.roundVector (currentTile + offset)] : null;
		RampBehaviour ramp = block == null ? null : block.GetComponent<RampBehaviour> ();
		return
			(!PLAYER_CAN_FALL && !SpawnTiles.tileExists (currentTile + offset + new Vector3 (0, -2, 0)))
			||(
				block !=null &&
				block.GetComponent<VoidManager> () == null &&
				block.GetComponent<WaterManager> () == null &&
				(ramp == null || (ramp.upSlopeDirection != offset && currentRampDirection != ramp.upSlopeDirection))
			);
	}

	static float GetMagnitude(Vector2 input){
		float deadZone = 0.1f;
		return Mathf.Clamp01(input.magnitude - deadZone) / (1 - deadZone);
	}

	public static Vector3 WaterMove(WaterManager wtr) {
		Vector3 next = new Vector3(0,0,0);
		float constant = 0.786F;
		switch (wtr.getDirection()) {
		case 0:
			next = (RIGHT + UP) * Time.deltaTime * speed * constant;
			break;
		case 90:
			next = (RIGHT + UP * -1) * Time.deltaTime * speed * constant;
			break;
		case 180:
			next = (RIGHT + UP) * -1 * Time.deltaTime * speed * constant;
			break;
		case 270:
			next = (RIGHT * -1 + UP) * Time.deltaTime * speed * constant;
			break;
		}
		return next;
	}


	public static void IceMove(Player p){
		p.transform.position += p.transform.forward * Time.deltaTime * speed;
	}

}
