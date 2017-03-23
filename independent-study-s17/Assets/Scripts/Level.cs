using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Level {

	public class Block{
		public int3 pos { get; set; }
		public BlockType type { get; set; }
	}

	public class LiminalBlock{
		public int3 pos { get; set; }
		public byte dir { get; set; }
		/* 0: +x
		 * 1: +z
		 * 2: -x
		 * 3: -z
		 */
	}

	public readonly int3[] directions = new int3[]{
		new int3(2,0,0),
		new int3(0,0,2),
		new int3(-2,0,0),
		new int3(0,0,-2),
	};

	public List<Block> blocks;
	public List<Block> liminalBlocks;
	public List<List<SpellManager.spell>> spellPrereqs;
	public List<string> levelPrereqs;
	public int3 position;
	public int3 min;
	public int3 max;
	public string name;
	public List<Level> nextLevels;
	public List<int3> startBlocks;
	public List<int3> endBlocks;
	public GameObject obj;

	private static Dictionary<string,SpellManager.spell> toEnum = new Dictionary<string, SpellManager.spell>(){
		{"push",SpellManager.spell.PUSH},
		{"block",SpellManager.spell.CREATE_BLOCK}
	};

	public Level(string levelName){
		string s = System.IO.File.ReadAllText(levelName);
		Regex regex = new Regex ("([^/]+)\\.txt");
		Match mc = regex.Match (levelName);
		name = mc.Groups[1].Value;
		nextLevels = new List<Level> ();
		startBlocks = new List<int3> ();
		endBlocks = new List<int3> ();
		obj = new GameObject (name);

		blocks = new List<Block> ();
		spellPrereqs = new List<List<SpellManager.spell>> ();
		liminalBlocks = new List<Block> ();
		levelPrereqs = new List<string> ();
		min = new int3 (int.MaxValue, int.MaxValue, int.MaxValue);
		max = new int3 (int.MinValue, int.MinValue, int.MinValue);

		foreach (string line in s.Split('\n')) {
			string[] item = line.Split (',');
			if (item [0] == "spells") {
				List<SpellManager.spell> andSpells = new List<SpellManager.spell>();
				for (int i = 1; i < item.Length; i++) {
					andSpells.Add (toEnum [item [i]]);
				}
				spellPrereqs.Add (andSpells);
			} else if (item [0] == "prereq") {
				levelPrereqs.Add (item [1]);
			}else {
				int3 pos = new int3 (
					int.Parse (item [0]),
					int.Parse (item [1]),
					int.Parse (item [2]));
				BlockType type = WorldManager.blockTypes [0];
				foreach(BlockType t in WorldManager.blockTypes){
					if (t.name == item [3]) {
						type = t;
						break;
					}
				}
				if (item [3] == "spawn") {
					startBlocks.Add (pos);
				} else if (item [3] == "goal") {
					endBlocks.Add (pos);
				}
				blocks.Add (new Block { pos = pos, type = type });
				min = int3.minBound (pos, min);
				max = int3.maxBound (pos, max);
			}
		}
	}

	public void Spawn(){
		foreach (Block b in blocks) {
			GameObject instance = GameObject.Instantiate (b.type.prefab);
			Vector3 pos = b.pos.ToVector () + position.ToVector ();
			instance.transform.position = pos;
			try{
				SpawnTiles.blocks.Add (pos, instance);
			}catch{
			}
			instance.transform.SetParent (obj.transform);
		}
		foreach (Block b in liminalBlocks) {
			GameObject instance = GameObject.Instantiate (b.type.prefab);
			Vector3 pos = b.pos.ToVector ();
			instance.transform.position = pos;
			SpawnTiles.blocks.Add (pos, instance);
			instance.transform.SetParent (obj.transform);
		}
	}

	public bool canSpawn(List<SpellManager.spell> knownSpells, List<Level> levelsSpawned){
		bool hasNecessarySpells = true;
		foreach (List<SpellManager.spell> andList in spellPrereqs) {
			hasNecessarySpells = true;
			foreach (SpellManager.spell spell in andList) {
				if (!knownSpells.Contains (spell)) {
					hasNecessarySpells = false;
					break;
				}
			}
			if (hasNecessarySpells) {
				break;
			}
		}

		bool hasNecessaryLevels = true;
		foreach (string s in levelPrereqs) {
			hasNecessaryLevels = false;
			foreach (Level l in levelsSpawned) {
				if (l.name == s) {
					hasNecessaryLevels = true;
				}
			}
			if (!hasNecessaryLevels) {
				break;
			}
		}

		return hasNecessaryLevels && hasNecessarySpells;
	}

	public void Zero(){
		int3 startPos = startBlocks [Random.Range (0, startBlocks.Count)];
		position = -startPos;
	}

	private void GenerateLiminality(int3 pos, List<LiminalBlock> blockList){
		if (pos.x == max.x) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 0 });
		}
		if (pos.z == max.z) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 1 });
		}
		if (pos.x == min.x) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 2 });
		}
		if (pos.z == min.z) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 3 });
		}
	}

	public LiminalBlock GenerateLiminalBlock(List<int3> blockList){
		List<LiminalBlock> liminality = new List<LiminalBlock>();
		foreach (int3 pos in blockList) {
			GenerateLiminality (pos, liminality);
		}
		return liminality [Random.Range (0, liminality.Count)];
	}

	public int3 size(){
		return max - min;
	}

	private void AddLiminalBlock(int3 pos){
		if (!WorldManager.liminalBlocks.Contains (pos)) {
			liminalBlocks.Add (new Block{ pos = pos, type = WorldManager.blockTypes [0] });
			WorldManager.liminalBlocks.Add (pos);
		}
	}
		
	public void GenerateConnectors(){
		if (nextLevels.Count == 0) {
			return;
		}
		//TODO: optimize for no two levels on same side
		LiminalBlock exit = GenerateLiminalBlock(endBlocks);
		int3 exitPosition = exit.pos + position + directions[exit.dir];
		AddLiminalBlock (exitPosition);

		int directionChoice = Random.Range (0, 4);
		List<int3> liminalBlocksGenerated = new List<int3>();
		liminalBlocksGenerated.Add (exitPosition);

		for(int i=0; i<nextLevels.Count; i++){
			Level nextLevel = nextLevels [i];

			LiminalBlock entrance = nextLevel.GenerateLiminalBlock (nextLevel.startBlocks);

			//superimpose entrance on exit
			int3 entrancePosition = entrance.pos + directions[entrance.dir];
			nextLevel.position = exitPosition - entrancePosition;

			//while levels intersect, move such that entrance doesn't intersect
			int timeout = 1000;
			List<int3> liminalBlocksToAdd = new List<int3> ();
			while (WorldManager.regionIntersect (nextLevel.position, nextLevel.min, nextLevel.max)) {
				timeout--;
				if (timeout == 0) {
					Debug.Log ("Timeout");
					return;
				}
				List<int3> dirOptions = new List<int3> (directions);
				foreach (int3 dir in directions) {
					if (WorldManager.blockIntersect (nextLevel.position + entrancePosition + dir,liminalBlocksGenerated)) {
						dirOptions.Remove (dir);
					}
				}

				dirOptions.Remove (directions [entrance.dir]);
				dirOptions.Remove (directions [(exit.dir + 2) % 4]);

				if (dirOptions.Count == 0) {
					/*Debug.Log ("Something has gone horribly wrong.");
					return;*/
					dirOptions.Add (directions [entrance.dir]);
				}
				nextLevel.position += dirOptions [directionChoice % dirOptions.Count];

				liminalBlocksToAdd.Add (entrancePosition + nextLevel.position);
				liminalBlocksGenerated.Add (entrancePosition + nextLevel.position);
			}
			foreach(int3 pos in liminalBlocksToAdd){
				AddLiminalBlock (pos);
			}
			WorldManager.levelsSpawned.Add (nextLevel);
		}
	}

}