using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WorldManager : MonoBehaviour {

	public static BlockType[] blockTypes;
	public BlockType[] publicBlockTypes;
	public Level[] levelPool;

	// Use this for initialization
	void Awake () {
		SpawnTiles.blocks = new Dictionary<Vector3, GameObject> ();
		blockTypes = publicBlockTypes;

		string[] files = Directory.GetFiles ("Assets/Resources","*.txt");
		levelPool = new Level[files.Length];

		List<Level> levelsSpawned = new List<Level> ();
		List<SpellManager.spell> spellsLearned = new List<SpellManager.spell> (); 

		List<Level> possibleFirstLevels = new List<Level> ();

		for(int i=0; i<files.Length;i++){
			string s = files [i];
			levelPool[i] = new Level (s);
			if (levelPool [i].canSpawn (spellsLearned, levelsSpawned)) {
				possibleFirstLevels.Add (levelPool [i]);
			}
		}

		Level firstLevel = possibleFirstLevels [Random.Range (0, possibleFirstLevels.Count)];
		levelsSpawned.Add (firstLevel);

		firstLevel.Spawn ();
	}
}
