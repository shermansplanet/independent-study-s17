using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour {

	private WaterManager parent = null;

	public bool source = false;

	//direction should be 0, 90, 180, or 270 
	public int direction = 0;

	public void changeParent(WaterManager p) {
		this.parent = p;
	}

	public WaterManager getParent() {
		return this.parent;
	}

	public bool isSource() {
		return this.source;
	}

	public void changeType() {
		this.source = !this.source;
	}

	public int getDirection() {
		return direction;
	}

	public void changeDirection(int d) {
		direction = d;
	}

	public void UpdateDirection(){
		direction = ((int)transform.eulerAngles.y + 360) % 360;
	}
}
