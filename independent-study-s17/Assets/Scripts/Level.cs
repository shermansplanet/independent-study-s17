using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Level {

	const int LEVEL_BORDER = 2;

	public class Block{
		public int3 pos { get; set; }
		public byte dir { get; set; }
		public BlockType type { get; set; }
		public GameObject spawnedBlock;
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
	public int3 visualMin;
	public int3 visualMax;
	public string name;
	public List<Level> nextLevels;
	public List<int3> startBlocks;
	public List<int3> endBlocks;
	public GameObject obj;
	public SpellManager.spell spellReward;
	public GameObject levelMarker;

	private static Dictionary<string,SpellManager.spell> toEnum = new Dictionary<string, SpellManager.spell>(){
		{"push",SpellManager.spell.PUSH},
		{"block",SpellManager.spell.CREATE_BLOCK},
		{"void",SpellManager.spell.CREATE_VOID}
	};

	private List<Block> iceBlocks;
	private Dictionary<int,Block> buttons;
	private Dictionary<Block,int> doors;

	public Level(string levelName){
		string s = System.IO.File.ReadAllText(levelName);
		Regex regex = new Regex ("([^/]+)\\.txt");
		Match mc = regex.Match (levelName);
		spellReward = SpellManager.spell.NO_EFFECT;
		name = mc.Groups[1].Value;
		nextLevels = new List<Level> ();
		startBlocks = new List<int3> ();
		endBlocks = new List<int3> ();
		obj = new GameObject (name);
		iceBlocks = new List<Block> ();
		buttons = new Dictionary<int, Block> ();
		doors = new Dictionary<Block, int> ();
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
				bool icy = false;
				if (item [3].Substring (0, 4) == "ice_") {
					item [3] = item [3].Substring (4);
					icy = true;
				}
				foreach(BlockType t in WorldManager.blockTypes){
					if (t.name == item [3]) {
						type = t;
						break;
					}
				}

				byte dir = item.Length < 5 ? (byte)0 : byte.Parse (item [4]);
				Block b = new Block { pos = pos, type = type, dir = dir };

				if (item [3] == "spawn") {
					startBlocks.Add (pos);
				} else if (item [3] == "goal") {
					endBlocks.Add (pos);
				} else if (item [3] == "button") {
					buttons.Add (int.Parse(item [5]), b);
				} else if (item [3] == "door") {
					doors.Add (b, int.Parse(item [5]));
				}

				blocks.Add (b);

				if (icy) {
					iceBlocks.Add (b);
				}
				min = int3.minBound (pos, min);
				max = int3.maxBound (pos, max);
			}
		}

		visualMax = new int3(max);
		visualMax.x += Mathf.RoundToInt (max.y * 5f / 7f);
		visualMax.z += Mathf.RoundToInt (max.y * 5f / 7f);
		visualMin = new int3(min);
		visualMin.x += Mathf.RoundToInt (min.y * 5f / 7f);
		visualMin.z += Mathf.RoundToInt (min.y * 5f / 7f);

	}

	private void SpawnNewBlock(Block b, Vector3 pos){
		if (!SpawnTiles.blocks.ContainsKey (pos)) {
			GameObject instance = GameObject.Instantiate (b.type.prefab);
			instance.transform.position = pos;
			if (b.type.name != "button") {
				SpawnTiles.blocks.Add (pos, instance);
			}
			instance.transform.SetParent (obj.transform);
			instance.transform.rotation = Quaternion.LookRotation (directions [b.dir].ToVector());
			if (b.type.name == "ramp") {
				instance.GetComponent<RampBehaviour> ().upSlopeDirection = directions [b.dir].ToVector ();
			} else if (b.type.name == "water") {
				instance.GetComponent<WaterManager> ().UpdateDirection ();
			}
			if (instance.GetComponent<MeshFilter> () != null) {
				SetVertexColors (instance);
			}
			if (iceBlocks.Contains (b)) {
				instance.AddComponent<IceManager> ().updateMaterial();
			}
			b.spawnedBlock = instance;
		}
	}

	public static void SetVertexColors(GameObject obj){
		Mesh m = obj.GetComponent<MeshFilter> ().mesh;
		Color[] colors = new Color[m.vertices.Length];

		for (int i = 0; i < m.vertices.Length; i++) {
			Vector3 pos = obj.transform.TransformPoint(m.vertices[i]) * 0.06f;
			float noise1 = Mathf.PerlinNoise (pos.x, pos.z);
			float noise2 = Mathf.PerlinNoise (pos.x+100, pos.z);
			float noise3 = Mathf.PerlinNoise (pos.x+200, pos.z);
			colors [i] = new Color (noise1, noise2 * noise1, noise3 * noise2 * noise1);
		}

		// assign the array of colors to the Mesh.
		m.colors = colors;
	}

	public void Reset(){
		//Erase all respawning blocks
		foreach (Pushblock p in obj.GetComponentsInChildren<Pushblock>()) {
			SpawnTiles.blocks.Remove (p.transform.position);
			GameObject.Destroy (p.gameObject);
		}
		foreach (Block b in blocks) {
			if (b.type.name == "physics") {
				Vector3 pos = b.pos.ToVector () + position.ToVector ();
				SpawnNewBlock (b, pos);
			}
		}
	}

	public void Spawn(){
		foreach (Block b in blocks) {
			Vector3 pos = b.pos.ToVector () + position.ToVector ();
			SpawnNewBlock (b, pos);
		}
		foreach (Block b in liminalBlocks) {
			Vector3 pos = b.pos.ToVector ();
			SpawnNewBlock (b, pos);
		}
		foreach (Block b in doors.Keys) {
			buttons [doors [b]].spawnedBlock.GetComponent<ButtonBehaviour> ().doors.Add (b.spawnedBlock.GetComponent<DoorBehaviour> ());
		}
		if (spellReward != SpellManager.spell.NO_EFFECT) {
			Vector3 pos = (position + endBlocks [Random.Range (0, endBlocks.Count)] + new int3(0,2,0)).ToVector();
			GameObject rewardInstance = GameObject.Instantiate (WorldManager.instance.spellPickup, pos, Quaternion.identity);
			rewardInstance.GetComponent<SpellPickupBehaviour> ().spell = spellReward;
			rewardInstance.transform.SetParent (obj.transform);
		}
	}

	public bool canSpawn(List<SpellManager.spell> knownSpells, List<Level> levelsSpawned){
		List<SpellManager.spell> localKnownSpells = new List<SpellManager.spell> (knownSpells);
		bool hasNecessarySpells = true;
		foreach (List<SpellManager.spell> andList in spellPrereqs) {
			hasNecessarySpells = true;
			foreach (SpellManager.spell spell in andList) {
				if (localKnownSpells.Contains (spell)) {
					localKnownSpells.Remove (spell);
				}else{
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
			while (blockMax.x < visualMax.x + LEVEL_BORDER) {
				blockMax.x += 2;
			}
		}
		if (pos.z == max.z) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 1 });
			while (blockMax.z < visualMax.z + LEVEL_BORDER) {
				blockMax.z += 2;
			}
		}
		if (pos.x == min.x) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 2 });
			while (blockMin.x > visualMin.x - LEVEL_BORDER) {
				blockMin.x -= 2;
			}
		}
		if (pos.z == min.z) {
			blockList.Add (new LiminalBlock{ pos = pos, dir = 3 });
			while (blockMin.z > visualMin.z - LEVEL_BORDER) {
				blockMin.z -= 2;
			}
		}
		for (int x = blockMin.x; x <= blockMax.x; x+=2) {
			for (int z = blockMin.z; z <= blockMax.z; z+=2) {
				int3 newPos = new int3 (x, pos.y, z);
				whitelist.Add (newPos);
			}
		}
	}

	public LiminalBlock GenerateLiminalBlock(List<int3> blockList){
		List<LiminalBlock> liminality = new List<LiminalBlock>();
		whitelist = new List<int3> ();

		foreach (int3 pos in blockList) {
			GenerateLiminality (pos, liminality);
		}

		foreach (int3 pos in blockList) {
			whitelist.Remove (pos);
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

		List<int3> liminalBlocksGenerated = new List<int3>();

		List<Level> toRemove = new List<Level> ();

		for(int i=0; i<nextLevels.Count; i++){
			Level nextLevel = nextLevels [i];

			bool success = false;
			for (int n = 0; n < 6; n++) {
				
				LiminalBlock exit = GenerateLiminalBlock(endBlocks);
				int3 exitPosition = exit.pos + position;

				if (GenerateConnector (nextLevel, exitPosition, exit, liminalBlocksGenerated)) {
					success = true;
					break;
				}
			}

			if (!success) {
				Debug.Log ("Failed to connect level " + name + " >>> " + nextLevel.name);
				toRemove.Add (nextLevel);
			}
		}

		foreach (Level l in toRemove) {
			nextLevels.Remove (l);
		}

		if (nextLevels.Count == 0) {
			return;
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

		List<Level> allLevels = new List<Level> (nextLevels);
		allLevels.Add (this);

		for (int n = 0; n < extraBlockCount; n++) {
			int3 pos = liminalBlocksGenerated [Random.Range (0, liminalBlocksGenerated.Count)];
			pos += directions [Random.Range(0, 4)];
			if (!WorldManager.blockIntersect (pos, liminalWhitelist, LEVEL_BORDER * 2, allLevels) && !liminalBlocksGenerated.Contains(pos)) {
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

		List<int3> blocksToIgnore = new List<int3> (liminalBlocksGenerated);

		foreach (int3 pos in whitelist) {
			blocksToIgnore.Add (pos + position);
		}

		List<int3> liminalBlocksToAdd = new List<int3> ();

		for (int i = 0; i < nextLevels.Count; i++) {
			exitPosition += directions [exit.dir];
			liminalBlocksToAdd.Add (exitPosition);
		}

		LiminalBlock entrance = nextLevel.GenerateLiminalBlock (nextLevel.startBlocks);

		int3[] entranceBlocks = new int3[LEVEL_BORDER];

		int3 entrancePosition = entrance.pos;
		for (int n = 0; n < LEVEL_BORDER+1; n++) {
			if (n > 0) {
				entranceBlocks [n - 1] = entrancePosition;
			}
			entrancePosition += directions[entrance.dir];
		}

		List<int3> tempDirections = new List<int3> (directions);
		List<int3> directionPriority = new List<int3> ();
		for(int j=4; j>0; j--){
			int index = Random.Range (0, j);
			directionPriority.Add (tempDirections[index]);
			tempDirections.RemoveAt (index);
		}

		Dictionary<int3,int3> parents = new Dictionary<int3, int3> ();
		List<int3> squaresChecked = new List<int3> ();
		Queue<int3> squaresToCheck = new Queue<int3> ();
		squaresToCheck.Enqueue (exitPosition);
		parents.Add (exitPosition, exitPosition);

		int3 successBlock = null;
		while (squaresToCheck.Count > 0) {
			int3 currentSquare = squaresToCheck.Dequeue ();
			squaresChecked.Add (currentSquare);

			List<int3> extraBlocks = Chain (new List<int3> (), parents, currentSquare);

			if (!WorldManager.regionIntersect (currentSquare - entrancePosition, nextLevel.min, nextLevel.max, nextLevel.visualMin, nextLevel.visualMax, LEVEL_BORDER * 2,extraBlocks)) {
				successBlock = currentSquare;
				break;
			}
			foreach (int3 d in directionPriority) {
				int3 newSquare = currentSquare + d;
				if (!squaresChecked.Contains (newSquare) &&
					!squaresToCheck.Contains(newSquare) &&
					!WorldManager.blockIntersect (newSquare, blocksToIgnore, LEVEL_BORDER * 2))
				{
					squaresToCheck.Enqueue (newSquare);
					parents.Add (newSquare, currentSquare);
				}
			}
		}

		if (successBlock == null) {
			/*foreach (int3 pos in squaresChecked) {
				GameObject.Instantiate (WorldManager.blockTypes [2].prefab, pos.ToVector (), Quaternion.identity);
			}*/
			Debug.Log ("BFS failure");
			return false;
		}

		nextLevel.position = successBlock - entrancePosition;

		Chain (liminalBlocksToAdd, parents, successBlock);

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

	public List<int3> Chain(List<int3> list, Dictionary<int3,int3> parents, int3 start){
		int3 next = parents [start];
		if (start == next) {
			return list;
		} else {
			list.Add (start);
			return Chain (list, parents, next);
		}
	}
}