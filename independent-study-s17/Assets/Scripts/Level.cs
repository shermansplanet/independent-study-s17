using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Level {

	const int LEVEL_BORDER = 2;

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

	public struct connectorOption{
		public List<int3> blocks;
		public int3 newPos;
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
			Vector3 pos = b.pos.ToVector () + position.ToVector ();
			if (!SpawnTiles.blocks.ContainsKey (pos)) {
				GameObject instance = GameObject.Instantiate (b.type.prefab);
				instance.transform.position = pos;
				SpawnTiles.blocks.Add (pos, instance);
				instance.transform.SetParent (obj.transform);
			}
		}
		foreach (Block b in liminalBlocks) {
			Vector3 pos = b.pos.ToVector ();
			if (!SpawnTiles.blocks.ContainsKey (pos)) {
				GameObject instance = GameObject.Instantiate (b.type.prefab);
				instance.transform.position = pos;
				SpawnTiles.blocks.Add (pos, instance);
				instance.transform.SetParent (obj.transform);
				instance.transform.localScale = Vector3.one * 1.5f;
			}
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

	public List<int3> whitelist;

	private void GenerateLiminality(int3 pos, List<LiminalBlock> blockList){
		int3 blockMin = new int3(pos);
		int3 blockMax = new int3(pos);
		if (pos.x == max.x) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 0 });
			blockMax.x += LEVEL_BORDER * 2;
		}
		if (pos.z == max.z) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 1 });
			blockMax.z += LEVEL_BORDER * 2;
		}
		if (pos.x == min.x) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 2 });
			blockMin.x -= LEVEL_BORDER * 2;
		}
		if (pos.z == min.z) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 3 });
			blockMin.z -= LEVEL_BORDER * 2;
		}
		for (int x = blockMin.x; x <= blockMax.x; x+=2) {
			for (int z = blockMin.z; z <= blockMax.z; z+=2) {
				int3 newPos = new int3 (x, pos.y, z);
				if (newPos != pos) {
					whitelist.Add (newPos);
				}
			}
		}
	}

	public LiminalBlock GenerateLiminalBlock(List<int3> blockList){
		List<LiminalBlock> liminality = new List<LiminalBlock>();
		whitelist = new List<int3> ();
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

		List<int3> liminalBlocksGenerated = new List<int3>();

		for(int i=0; i<nextLevels.Count; i++){
			Level nextLevel = nextLevels [i];

			bool success = false;
			for (int n = 0; n < 10; n++) {
				LiminalBlock exit = GenerateLiminalBlock(endBlocks);
				int3 exitPosition = exit.pos + position;

				if (GenerateConnector (nextLevel, exitPosition, exit, liminalBlocksGenerated)) {
					success = true;
					break;
				}
			}

			if (!success) {
				Debug.Log ("Failed to connect level");
			}
		}

		List<int3> liminalWhitelist = new List<int3> (liminalBlocksGenerated);

		foreach (int3 b in whitelist) {
			liminalWhitelist.Add (b + position);
			//GameObject.Instantiate (WorldManager.blockTypes [2].prefab, (b + position).ToVector(), Quaternion.identity);
		}

		foreach (Level l in nextLevels) {
			foreach (int3 b in l.whitelist) {
				liminalWhitelist.Add (b + l.position);
				//GameObject.Instantiate (WorldManager.blockTypes [2].prefab, (b + l.position).ToVector(), Quaternion.identity);
			}
		}

		int extraBlockCount = 100;//liminalBlocksGenerated.Count * 2;

		for (int n = 0; n < extraBlockCount; n++) {
			int3 pos = liminalBlocksGenerated [Random.Range (0, liminalBlocksGenerated.Count)];
			pos += directions [Random.Range(0, 4)];
			if (!WorldManager.blockIntersect (pos, liminalWhitelist, LEVEL_BORDER * 2, this) && !liminalBlocksGenerated.Contains(pos)) {
				AddLiminalBlock (pos);
				liminalBlocksGenerated.Add (pos);
				liminalWhitelist.Add (pos);
			}
		}

		for (int n = 0; n < LEVEL_BORDER; n++) {
			int len = liminalBlocksGenerated.Count;
			for(int i=0; i<len; i++){
				int3 b = liminalBlocksGenerated [i];
				foreach (int3 d in directions) {
					if (!WorldManager.liminalBlocks.Contains(b+d)) {
						WorldManager.liminalBlocks.Add (b + d);
						liminalBlocksGenerated.Add (b + d);
					}
				}
			}
		}
	}

	private bool GenerateConnector(Level nextLevel, int3 exitPosition, LiminalBlock exit, List<int3> liminalBlocksGenerated){

		for (int j = 0; j < LEVEL_BORDER+1; j++) {
			exitPosition += directions [exit.dir];
			if (WorldManager.blockIntersect (exitPosition, liminalBlocksGenerated, LEVEL_BORDER * 2, this)) {
				Debug.Log ("Exit untenable");
				return false;
			} else {
				AddLiminalBlock (exitPosition);
				liminalBlocksGenerated.Add (exitPosition);
			}
		}

		LiminalBlock entrance = nextLevel.GenerateLiminalBlock (nextLevel.startBlocks);
		List<int3> possibleBlocksGenerated = new List<int3> (liminalBlocksGenerated);
		List<int3> liminalBlocksToAdd = new List<int3> ();
		int3[] entranceBlocks = new int3[LEVEL_BORDER];

		//superimpose entrance on exit
		int3 entrancePosition = entrance.pos;
		for (int n = 0; n < LEVEL_BORDER+1; n++) {
			if (n > 0) {
				entranceBlocks [n - 1] = entrancePosition;
			}
			entrancePosition += directions[entrance.dir];
		}
		nextLevel.position = exitPosition - entrancePosition;

		//while levels intersect, move such that entrance doesn't intersect
		int timeout = 1000;

		List<int3> tempDirections = new List<int3> (directions);
		List<int3> directionPriority = new List<int3> ();
		for(int j=4; j>0; j--){
			int index = Random.Range (0, j);
			directionPriority.Add (tempDirections[index]);
			tempDirections.RemoveAt (index);
		}
			

		while (WorldManager.regionIntersect (nextLevel.position, nextLevel.min, nextLevel.max,LEVEL_BORDER * 2)) {
			timeout--;
			if (timeout == 0) {
				Debug.Log ("Timeout");
				return false;
			}
			List<int3> dirOptions = new List<int3> (directionPriority);
			foreach (int3 dir in directions) {
				if (WorldManager.blockIntersect (nextLevel.position + entrancePosition + dir, possibleBlocksGenerated, LEVEL_BORDER * 2) ||
					liminalBlocksToAdd.Contains(nextLevel.position + entrancePosition + dir)
				) {
					dirOptions.Remove (dir);
				}
			}

			dirOptions.Remove (directions [entrance.dir]);
			dirOptions.Remove (directions [(exit.dir + 2) % 4]);

			if (dirOptions.Count == 0) {
				Debug.Log ("We're trapped!");
				return false;
			}
			nextLevel.position += dirOptions [0];

			liminalBlocksToAdd.Add (entrancePosition + nextLevel.position);
			possibleBlocksGenerated.Add (entrancePosition + nextLevel.position);
		}

		WorldManager.levelsSpawned.Add (nextLevel);

		foreach (int3 pos in entranceBlocks) {
			liminalBlocksToAdd.Add (pos + nextLevel.position);
		}

		foreach(int3 pos in liminalBlocksToAdd){
			AddLiminalBlock (pos);
			liminalBlocksGenerated.Add (pos);
		}
		return true;
	}

}