﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class int3{
	public int x, y, z;
	public int3(int x, int y, int z){
		this.x = x;
		this.y = y;
		this.z = z;
	}
	public Vector3 ToVector(){
		return new Vector3 (x, y, z);
	}

	public static int3 minBound(int3 a, int3 b){
		return new int3 (
			Mathf.Min(a.x,b.x),
			Mathf.Min(a.y,b.y),
			Mathf.Min(a.z,b.z)
		);
	}

	public static int3 maxBound(int3 a, int3 b){
		return new int3 (
			Mathf.Max(a.x,b.x),
			Mathf.Max(a.y,b.y),
			Mathf.Max(a.z,b.z)
		);
	}

	public override bool Equals ( object obj )
	{
		if ( obj == null ){
			return false;
		}

		if ( this.GetType ( ) != obj.GetType ( ) ){
			return false;
		}

		return Equals ( ( int3 ) obj );
	}
	public bool Equals(int3 other){
		return other.x == x && other.y == y && other.z == z;
	}
	public override int GetHashCode()
	{
		return x + y * 10 + z * 100;
	}
}