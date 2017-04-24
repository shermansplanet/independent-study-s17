using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WorldManager : MonoBehaviour {

	public static BlockType[] blockTypes;
	public BlockType[] publicBlockTypes;
	public Level[] levelPool;
	public GameObject spellPickup;

	const int MIN_PATHS = 1;
	const int MAX_PATHS = 3;

	public static List<int3> liminalBlocks;
	public static List<Level> levelsSpawned;
	public static Queue<Level> levelsToSpawn;

	public static WorldManager instance;

	// Use this for initialization
	void Awake () {
		//Random.InitState (7);
		instance = this;
		levelsSpawned = new List<Level> ();
		liminalBlocks = new List<int3> ();
		SpawnTiles.blocks = new Dictionary<Vector3, GameObject> ();
		blockTypes = publicBlockTypes;

		string[] files = Directory.GetFiles ("Assets/Resources","*.txt");
		levelPool = new Level[files.Length];

		List<Level> levelsAdded = new List<Level> ();

		List<SpellManager.spell> spellsLearned = new List<SpellManager.spell>{
			SpellManager.spell.PUSH,
			SpellManager.spell.PUSH
		};

		List<SpellManager.spell> spellsToLearn = new List<SpellManager.spell>{
			SpellManager.spell.CREATE_BLOCK,
			SpellManager.spell.CREATE_VOID,
			SpellManager.spell.CREATE_BLOCK,
			SpellManager.spell.CREATE_VOID,
		};

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

		while (spellsToLearn.Count > 0) {
			while (possibleLevels.Count > 0) {
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
					if (possibleLevel.canSpawn (spellsLearned, levelsAdded) && !levelsAdded.Contains (possibleLevel) && !possibleLevels.Contains (possibleLevel)) {
						possibleLevels.Add (possibleLevel);
					}
				}
			}
			SpellManager.spell spell = spellsToLearn [0];
			spellsToLearn.RemoveAt (0);
			spellsLearned.Add (spell);
			Level rewardPlace = openLevelExits [Random.Range (0, openLevelExits.Count)];
			while (rewardPlace.spellReward != SpellManager.spell.NO_EFFECT) {
				rewardPlace = openLevelExits [Random.Range (0, openLevelExits.Count)];
			}
			rewardPlace.spellReward = spell;
		}

		/*foreach (Level l1 in levelsAdded) {
			foreach (Level l2 in l1.nextLevels) {
				Debug.Log (l1.name + ">>>" + l2.name);
			}
		}*/

		firstLevel.Zero ();
		levelsSpawned.Add (firstLevel);

		levelsToSpawn = new Queue<Level> ();
		levelsToSpawn.Enqueue (firstLevel);
		StartCoroutine("Generate");
	}

	IEnumerator Generate(){
		while (levelsToSpawn.Count > 0) {
			Level l = levelsToSpawn.Dequeue ();
			l.GenerateConnectors ();
			l.Spawn ();
			foreach (Level child in l.nextLevels) {
				levelsToSpawn.Enqueue (child);
			}
			yield return new WaitForEndOfFrame ();
		}
	}

	public static bool blockIntersect(int3 pos, List<int3> toIgnore = null, int buffer = 4, List<Level> ignoreLevels = null){
		if (toIgnore != null && ignoreLevels==null && toIgnore.Contains (pos)) {
			return false;
		}

		int3 visualPos = new int3 (pos);
		visualPos.x += Mathf.RoundToInt (pos.y * 5f / 7f);
		visualPos.z += Mathf.RoundToInt (pos.y * 5f / 7f);

		foreach (Level l in levelsSpawned) {
			if (
				(visualPos.x >= l.visualMin.x + l.position.x - buffer) &&
				(visualPos.x <= l.visualMax.x + l.position.x + buffer) &&
				(visualPos.z >= l.visualMin.z + l.position.z - buffer) &&
				(visualPos.z <= l.visualMax.z + l.position.z + buffer)
			) {
				if(!(ignoreLevels!= null && ignoreLevels.Contains(l)) || !(toIgnore!= null && toIgnore.Contains(pos))){
					return true;
				}
			}
		}
		if (toIgnore != null && toIgnore.Contains (pos)) {
			return false;
		}
		return liminalBlocks.Contains (pos);
	}

	public static bool regionIntersect(int3 pos, int3 min, int3 max, int3 visualMin, int3 visualMax, int buffer = 4, List<int3> extraBlocks = null){
		foreach (Level l in levelsSpawned) {
			if (
				(pos.x + visualMax.x >= l.visualMin.x + l.position.x - buffer) &&
				(pos.x + visualMin.x <= l.visualMax.x + l.position.x + buffer) &&
				(pos.z + visualMax.z >= l.visualMin.z + l.position.z - buffer) &&
				(pos.z + visualMin.z <= l.visualMax.z + l.position.z + buffer)
			) {
				return true;
			}
		}
		List<int3> blocksToAvoid = new List<int3> (liminalBlocks);
		if (extraBlocks != null) {
			foreach (int3 block in extraBlocks) {
				blocksToAvoid.Add (block);
			}
		}
		foreach (int3 b in blocksToAvoid) {
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
