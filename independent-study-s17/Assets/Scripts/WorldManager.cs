using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WorldManager : MonoBehaviour {

	public static BlockType[] blockTypes;
	public BlockType[] publicBlockTypes;
	public Level[] levelPool;

	const int MIN_PATHS = 1;
	const int MAX_PATHS = 3;

	public static List<int3> liminalBlocks;
	public static List<Level> levelsSpawned;
	public static Queue<Level> levelsToSpawn;

	// Use this for initialization
	void Awake () {
		Random.InitState (0);
		levelsSpawned = new List<Level> ();
		liminalBlocks = new List<int3> ();
		SpawnTiles.blocks = new Dictionary<Vector3, GameObject> ();
		blockTypes = publicBlockTypes;

		string[] files = Directory.GetFiles ("Assets/Resources","*.txt");
		levelPool = new Level[files.Length];

		List<Level> levelsAdded = new List<Level> ();
		List<SpellManager.spell> spellsLearned = new List<SpellManager.spell> (); 

		List<Level> possibleLevels = new List<Level> ();

		for(int i=0; i<files.Length;i++){
			string s = files [i];
			levelPool[i] = new Level (s);
			if (levelPool [i].canSpawn (spellsLearned, levelsAdded)) {
				possibleLevels.Add (levelPool [i]);
			}
		}

		List<Level> openLevelExits = new List<Level> ();
		Level firstLevel = null;

		while(possibleLevels.Count > 0){
			Level l = possibleLevels [Random.Range (0, possibleLevels.Count)];
			possibleLevels.Remove (l);

			if (openLevelExits.Count == 0) {
				firstLevel = l;
			} else {
				Level parent = openLevelExits [Random.Range (0, openLevelExits.Count)];
				parent.nextLevels.Add (l);
				openLevelExits.Remove (parent);
			}

			levelsAdded.Add (l);
			int paths = Random.Range (MIN_PATHS, MAX_PATHS + 1);
			for (int i = 0; i < paths; i++) {
				openLevelExits.Add (l);
			}

			foreach (Level possibleLevel in levelPool) {
				if (possibleLevel.canSpawn (spellsLearned, levelsAdded) && !levelsAdded.Contains(possibleLevel) && !possibleLevels.Contains(possibleLevel)) {
					possibleLevels.Add (possibleLevel);
				}
			}
		}

		foreach (Level l1 in levelsAdded) {
			foreach (Level l2 in l1.nextLevels) {
				Debug.Log (l1.name + ">>>" + l2.name);
			}
		}

		firstLevel.Zero ();
		levelsSpawned.Add (firstLevel);

		levelsToSpawn = new Queue<Level> ();
		levelsToSpawn.Enqueue (firstLevel);
		StartCoroutine("Generate");
	}

	IEnumerator Generate(){
		while (levelsToSpawn.Count > 0) {
			Level l = levelsToSpawn.Dequeue ();
			foreach (Level child in l.nextLevels) {
				levelsToSpawn.Enqueue (child);
			}
			l.GenerateConnectors ();
			l.Spawn ();
			yield return null;
		}
	}

	public static bool blockIntersect(int3 pos, List<int3> toIgnore = null, int buffer = 4, Level ignoreLevel = null){
		if (toIgnore != null && ignoreLevel==null && toIgnore.Contains (pos)) {
			return false;
		}
		foreach (Level l in levelsSpawned) {
			if (ignoreLevel == l) {
				continue;
			}
			if (
				(pos.x >= l.min.x + l.position.x - buffer) &&
				(pos.x <= l.max.x + l.position.x + buffer) &&
				(pos.z >= l.min.z + l.position.z - buffer) &&
				(pos.z <= l.max.z + l.position.z + buffer)
			) {
				return true;
			}
		}
		if (toIgnore != null && toIgnore.Contains (pos)) {
			return false;
		}
		return liminalBlocks.Contains (pos);
	}

	public static bool regionIntersect(int3 pos, int3 min, int3 max, int buffer = 4){
		foreach (Level l in levelsSpawned) {
			if (
				(pos.x + max.x >= l.min.x + l.position.x - buffer) &&
				(pos.x + min.x <= l.max.x + l.position.x + buffer) &&
				(pos.z + max.z >= l.min.z + l.position.z - buffer) &&
				(pos.z + min.z <= l.max.z + l.position.z + buffer)
			) {
				return true;
			}
		}
		foreach (int3 b in liminalBlocks) {
			if (
				(b.x >= min.x + pos.x - buffer) &&
				(b.x <= max.x + pos.x + buffer) &&
				(b.z >= min.z + pos.z - buffer) &&
				(b.z <= max.z + pos.z + buffer)
			) {
				return true;
			}
		}
		return false;
	}
}
