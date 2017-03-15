using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Level {

	public class Block{
		public int3 pos { get; set; }
		public BlockType type { get; set; }
	}

	public List<Block> blocks;
	public List<List<SpellManager.spell>> spellPrereqs;
	public List<string> levelPrereqs;
	public int3 position;
	public int3 min;
	public int3 max;
	public string name;
	public List<Level> nextLevels;

	private static Dictionary<string,SpellManager.spell> toEnum = new Dictionary<string, SpellManager.spell>(){
		{"push",SpellManager.spell.PUSH},
		{"block",SpellManager.spell.CREATE_BLOCK}
	};

	public Level(string levelName){
		string s = System.IO.File.ReadAllText(levelName);
		Regex regex = new Regex ("([^/]+)\\.txt");
		Match mc = regex.Match (levelName);
		name = mc.Groups[1].Value;

		blocks = new List<Block> ();
		spellPrereqs = new List<List<SpellManager.spell>> ();
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
				blocks.Add (new Block { pos = pos, type = type });
				min = int3.minBound (pos, min);
				max = int3.maxBound (pos, max);
			}
		}
	}

	public void Spawn(){
		//Debug.Log ("Spawning " + name);
		foreach (Block b in blocks) {
			GameObject instance = GameObject.Instantiate (b.type.prefab);
			instance.transform.position = b.pos.ToVector();
			SpawnTiles.blocks.Add (b.pos.ToVector (), instance);
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

}
