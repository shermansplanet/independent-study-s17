﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour {

	public static BlockType[] blockTypes;
	public BlockType[] publicBlockTypes;
	public Level[] levelPool;
	public GameObject spellPickup;
	public GameObject levelMarker;
	public GameObject teleporter;
	public UnityEngine.EventSystems.EventSystem events;

	public static List<GameObject> AOTargets;

	//TEMP
	public RawImage testImage;

	const int MIN_PATHS = 1;
	const int MAX_PATHS = 3;

	public static List<int3> liminalBlocks;
	public static List<Level> levelsSpawned;
	public static Queue<Level> extraLevels;
	public static Queue<Level> levelsToSpawn;
	public static bool inMenu = false;
	public static WorldManager instance;
	public static Dictionary<Level,Level> shortcuts;

	// Use this for initialization
	void Awake () {
		//Random.InitState (7);
		instance = this;
		levelsSpawned = new List<Level> ();
		liminalBlocks = new List<int3> ();
		extraLevels = new Queue<Level> ();
		shortcuts = new Dictionary<Level, Level> ();
		AOTargets = new List<GameObject> ();
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
			SpellManager.spell.DESTROY,
			SpellManager.spell.CREATE_BLOCK,
			SpellManager.spell.DESTROY
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
			while (true) {
				foreach (Level possibleLevel in levelPool) {
					if (possibleLevel.canSpawn (spellsLearned, levelsAdded) && !levelsAdded.Contains (possibleLevel) && !possibleLevels.Contains (possibleLevel)) {
						possibleLevels.Add (possibleLevel);
					}
				}
				if (possibleLevels.Count == 0) {
					break;
				}
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
			}
			SpellManager.spell spell = spellsToLearn [0];
			spellsToLearn.RemoveAt (0);
			spellsLearned.Add (spell);
			Level rewardPlace = openLevelExits [Random.Range (0, openLevelExits.Count)];
			int timeout = 1000;
			while (rewardPlace.spellReward != SpellManager.spell.NO_EFFECT) {
				if (--timeout == 0) break;
				rewardPlace = openLevelExits [Random.Range (0, openLevelExits.Count)];
			}
			rewardPlace.spellReward = spell;
		}

		/*
		foreach (Level l1 in levelsAdded) {
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
		while (true) {
			while (levelsToSpawn.Count > 0) {
				Level l = levelsToSpawn.Dequeue ();
				l.GenerateConnectors ();
				l.Spawn ();
				foreach (Level child in l.nextLevels) {
					levelsToSpawn.Enqueue (child);
				}
				yield return new WaitForEndOfFrame ();
			}
			if (extraLevels.Count == 0)
				break;
			Level newLevel = extraLevels.Dequeue ();
			newLevel.Reposition ();
			levelsSpawned.Add (newLevel);
			levelsToSpawn.Enqueue (newLevel);
		}
		GenerateShortcuts ();
		StartCoroutine (GenerateAmbientOcclusion ());
		GenerateMap ();
	}

	void GenerateShortcuts(){
		foreach (Level l1 in shortcuts.Keys) {
			Level l2 = shortcuts [l1];
			TeleportBehaviour t1 = Instantiate (teleporter, (l1.position + l1.startBlocks [Random.Range (0, l1.endBlocks.Count)]).ToVector () + Vector3.up * 1.01f, teleporter.transform.rotation).GetComponent<TeleportBehaviour> ();
			TeleportBehaviour t2 = Instantiate (teleporter, (l2.position + l2.endBlocks [Random.Range (0, l2.endBlocks.Count)]).ToVector() + Vector3.up * 1.01f, teleporter.transform.rotation).GetComponent<TeleportBehaviour> ();
			t1.other = t2;
			t2.other = t1;
		}
	}

	IEnumerator GenerateAmbientOcclusion (){
		int count = 0;
		foreach (GameObject g in AOTargets) {
			Mesh m = g.GetComponent<MeshFilter> ().mesh;
			Color[] colors = m.colors;
			if (colors.Length == 0) {
				colors = new Color[m.vertices.Length];
				for (int i = 0; i < m.vertices.Length; i++) {
					colors [i] = Color.white;
				}
			}
			for (int i = 0; i < m.vertices.Length; i++) {
				Vector3 localPos = g.transform.TransformPoint (m.vertices [i]) - g.transform.position;
				Vector3 normal = g.transform.TransformDirection (m.normals [i]).normalized;
				if (Mathf.Abs (normal.x) + Mathf.Abs (normal.y) + Mathf.Abs (normal.z) > 1.1f) {
					continue;
				}
				foreach(Vector3 v in new Vector3[]{Vector3.up,Vector3.forward,Vector3.right,Vector3.zero}){
					float angle = Vector3.Angle (v, normal);
					Vector3 projection = normal;
					if (angle > 5 && angle < 175) {
						projection = localPos - v * Vector3.Dot (localPos, v);
					}
					if (isAOTarget(SpawnTiles.roundVector(g.transform.position + projection * 2))) {
						colors [i] *= new Color (0.6f, 0.6f, 0.6f);
					}
				}
			}
			// assign the array of colors to the Mesh.
			m.colors = colors;
			if((count++)%100==0){
				yield return null;
			}
		}
	}

	private static bool isAOTarget(Vector3 pos){
		foreach (GameObject g in AOTargets) {
			if (g.transform.position == pos) {
				return true;
			}
		}
		return false;
	}

	private void GenerateMap(){
		//Figure out size of the map
		int3 globalMin = new int3(int.MaxValue,int.MaxValue,int.MaxValue);
		int3 globalMax = new int3(int.MinValue,int.MinValue,int.MinValue);
		foreach (Level l in levelsSpawned) {
			globalMin = int3.minBound (l.min + l.position, globalMin);
			globalMax = int3.maxBound (l.max + l.position, globalMax);
		}
		const int border = 16;
		const int blocksize = 16;
		globalMax += new int3 (border, border, border);
		globalMin -= new int3 (border, border, border);

		Color mapColor = new Color (0.85f,0.8f,0.7f);

		//Create a blank texture of that size
		Texture2D mapTex = new Texture2D((globalMax.x - globalMin.x)*blocksize/2, (globalMax.z - globalMin.z)*blocksize/2);
		Color[] colors = new Color[mapTex.width * mapTex.height];
		for (int i = 0; i < colors.Length; i++) {
			colors [i] = mapColor;// * (Mathf.PerlinNoise ((i % mapTex.width)*0.005f, (Mathf.Floor (i / mapTex.width))*0.005f) * 0.2f + 0.8f);
			colors [i].a = 1;
		}
		mapTex.SetPixels (colors);

		//fill it
		foreach (Vector3 v in SpawnTiles.blocks.Keys) {
			int3 pos = new int3 (v);
			pos -= globalMin;
			for (int dx = 0; dx < blocksize; dx++) {
				for (int dz = 0; dz < blocksize; dz++) {
					mapTex.SetPixel (pos.x * blocksize/2 + dx, pos.z * blocksize/2 + dz, new Color (0, 0, 0, 1));
				}
			}
		}
		mapTex.Apply ();

		testImage.texture = mapTex;
		testImage.SetNativeSize ();

		bool firstLevel = true;

		foreach (Level l in levelsSpawned) {
			GameObject marker = Instantiate (levelMarker);
			marker.transform.parent = testImage.transform;
			int3 newpos = (l.max + l.min)/2 + l.position - globalMin;
			marker.GetComponent<RectTransform>().anchoredPosition = new Vector3 (newpos.x, newpos.z, 0) * blocksize/2;
			marker.GetComponent<LevelMarker> ().level = l;
			l.levelMarker = marker;
			if (firstLevel) {
				events.SetSelectedGameObject (marker);
				firstLevel = false;
			}
		}

		StartCoroutine(SetMenu());
	}

	void Update(){
		if (Input.GetButtonDown ("Map")) {
			inMenu = !inMenu;
			StartCoroutine(SetMenu());
		}
		if (inMenu && (Input.GetButtonDown("Spell0") || Input.GetButtonDown("Spell1"))) {
			foreach (Level l in levelsSpawned) {
				l.Reset ();
			}
			Level selectedLevel = events.currentSelectedGameObject.GetComponent<LevelMarker> ().level;
			foreach (Player p in PlayerManager.staticPlayers) {
				p.Respawn (selectedLevel);
			}
			inMenu = false;
			StartCoroutine(SetMenu());
		}
	}

	IEnumerator SetMenu(){
		testImage.gameObject.SetActive (inMenu);
		yield return null;
		if (inMenu) {
			GameObject marker = PlayerManager.staticPlayers [0].currentLevel.levelMarker;
			events.SetSelectedGameObject (null);
			events.SetSelectedGameObject (marker);
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
