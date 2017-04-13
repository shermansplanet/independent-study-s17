using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Levelmaker : MonoBehaviour {

	[System.Serializable]
	public class block{
		public string name;
		public Color color;
		public GameObject model;
	};

	class blockData{
		public block blockType;
		public int3 position;
		public byte direction;
		public GameObject obj;
		public int index = -1;

		public blockData(block blockType, int3 position, byte direction, GameObject obj, int index = -1){
			this.blockType = blockType;
			this.position = position;
			this.direction = direction;
			this.obj = obj;
			this.index = index;
		}
	}

	public readonly int3[] blockDirections = new int3[]{
		new int3(1,0,0),
		new int3(0,0,1),
		new int3(-1,0,0),
		new int3(0,0,-1),
	};

	public block[] blocktypes;
	public GameObject backCollider;
	public GameObject indicator;
	public GameObject arrow;
	public GameObject startCube;
	public Text display;
	public InputField saveInput;
	public InputField loadInput;
	public GameObject spellLinePrefab;
	public GameObject levelLinePrefab;
	public Transform canvas;
	public Transform addButton;
	public Transform addLevelButton;

	private bool placing = false;
	private bool deleting = false;
	private bool connecting = false;
	private int3 selectionStart = new int3(0,0,0);
	private Vector3 selectionEnd = new Vector3();
	private Material indicatorMaterial;
	private int currentBlockType = 0;
	private Vector3 cameraOffset;
	private Quaternion originalRotation;
	private static Levelmaker instance;
	private byte currentDirection;
	private blockData currentDoor;
	private GameObject currentButton = null;
	private int buttonCount;
	private LineRenderer line;

	private static List<GameObject> spellLines = new List<GameObject>();
	private static List<GameObject> levelLines = new List<GameObject>();
	private Dictionary<int3,blockData> blocksInScene = new Dictionary<int3,blockData>();

	readonly Vector3[] directions = new Vector3[]{Vector3.right,Vector3.up,Vector3.forward,Vector3.back,Vector3.down,Vector3.left};

	void Start(){
		line = GetComponent<LineRenderer> ();
		line.enabled = false;
		blocksInScene.Add (new int3 (0, 0, 0), new blockData (blocktypes [0], new int3 (0, 0, 0), 0, startCube));
		startCube.GetComponent<Renderer> ().material.color = blocktypes [0].color;
		indicatorMaterial = indicator.GetComponent<Renderer> ().material;
		RefreshDisplay ();
		cameraOffset = transform.InverseTransformVector(transform.position);
		originalRotation = transform.rotation;	
		currentDirection = 1;
		instance = this;
	}

	void RefreshDisplay(){
		string s = "";
		for (int i = 0; i < blocktypes.Length; i++) {
			block type = blocktypes [i];
			string line = "<color=" + ToRGBHex (type.color) + ">" + ((i+1)%10).ToString () + ". " + type.name + "</color>\n";
			if (i == currentBlockType) {
				line = "<b>" + line + "</b>";
			}
			s += line;
		}
		display.text = s;
	}

	private string ToRGBHex(Color c){
		return string.Format("#{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
	}

	private byte ToByte(float f){
		f = Mathf.Clamp01(f);
		return (byte)(f * 255);
	}

	public void AddSpellLine(){
		GameObject line = Instantiate (spellLinePrefab);
		spellLines.Add (line);
		line.transform.SetParent (canvas);
		float h = addButton.GetComponent<RectTransform> ().offsetMax.y;
		line.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, h);
		line.GetComponent<RectTransform> ().offsetMin = new Vector2 (0, h - 50);
		addButton.transform.Translate (0, -70, 0);
	}

	public void AddLevelLine(){
		GameObject line = Instantiate (levelLinePrefab);
		levelLines.Add (line);
		line.transform.SetParent (addButton.transform);
		float h = addLevelButton.GetComponent<RectTransform> ().offsetMax.y;
		line.GetComponent<RectTransform> ().offsetMax = new Vector2 (-60, h);
		line.GetComponent<RectTransform> ().offsetMin = new Vector2 (-300, h - 50);
		addLevelButton.transform.Translate (0, -70, 0);
	}

	public static void RemoveSpellLine(GameObject toRemove){
		bool movingUp = false;
		for (int i = 0; i < spellLines.Count; i++) {
			if (movingUp) {
				spellLines [i].transform.Translate (0, 70, 0);
			}
			if (spellLines [i] == toRemove) {
				movingUp = true;
			}
		}
		instance.addButton.transform.Translate (0, 70, 0);
		spellLines.Remove (toRemove);
		Destroy (toRemove);
	}

	public static void RemoveLevelLine(GameObject toRemove){
		bool movingUp = false;
		for (int i = 0; i < levelLines.Count; i++) {
			if (movingUp) {
				levelLines [i].transform.Translate (0, 70, 0);
			}
			if (levelLines [i] == toRemove) {
				movingUp = true;
			}
		}
		instance.addLevelButton.transform.Translate (0, 70, 0);
		levelLines.Remove (toRemove);
		Destroy (toRemove);
	}

	void Update () {

		if (!saveInput.isFocused && !loadInput.isFocused) {
			for (int i = 0; i < blocktypes.Length; i++) {
				if (Input.GetKeyDown (((i+1)%10).ToString ())) {
					currentBlockType = i;
					RefreshDisplay ();
				}
			}

			transform.Translate (new Vector3 (Input.GetAxis ("Horizontal1"),
				Input.GetAxis ("Vertical1"),
				Input.GetAxis ("Vertical1")) * Time.deltaTime * 10);

			Camera.main.orthographicSize *= 1 - Input.GetAxis ("Vertical0") * Time.deltaTime;

			Vector3 center = transform.position - transform.TransformVector (cameraOffset);
			transform.RotateAround (center, Vector3.up, -Input.GetAxis ("Horizontal0") * Time.deltaTime * 90);
			if (Input.GetKeyDown (KeyCode.R)) {
				transform.position = center;
				transform.rotation = originalRotation;
				transform.Translate (cameraOffset);
			}
		}

		if (placing || deleting) {
			float scaleFactor = deleting ? 2.01f : 1.99f;
			Vector3 startVector = selectionStart.ToVector ();
			if (placing && !Input.GetMouseButton (0)) {
				placing = false;
				indicator.transform.localScale = Vector3.one * 2;
				int blockCount = (int)Vector3.Distance (startVector, selectionEnd) / 2 + 1;
				Vector3 dirVector = (selectionEnd - startVector).normalized * 2;
				indicatorMaterial.SetTextureScale ("_MainTex", new Vector2 (1, 1));
				if (blocktypes [currentBlockType].name == "door") {
					blockCount = 1;
				}
				for (int i = 0; i < blockCount; i++) {
					Vector3 pos = startVector + i * dirVector;
					int3 intPos = getTile (pos);
					if (!blocksInScene.ContainsKey (intPos)) {
						MakeBlock (intPos, blocktypes [currentBlockType], currentDirection);
					}
				}
			} else if (deleting && !Input.GetMouseButton (1)) {
				deleting = false;
				indicator.transform.localScale = Vector3.one * 2;
				int blockCount = (int)Vector3.Distance (startVector, selectionEnd) / 2 + 1;
				Vector3 dirVector = (selectionEnd - startVector).normalized * 2;
				indicatorMaterial.SetTextureScale ("_MainTex", new Vector2 (1, 1));
				for (int i = 0; i < blockCount; i++) {
					int3 pos = getTile (startVector + i * dirVector);
					if (blocksInScene.ContainsKey (pos) && blocksInScene.Count > 1) {
						blockData data = blocksInScene [pos];
						Destroy (data.obj);
						blocksInScene.Remove (pos);
					}
				}
			} else {
				Vector2 screenSpaceStart = Camera.main.WorldToScreenPoint (startVector);
				Vector2 dir = (Vector2)Input.mousePosition - screenSpaceStart;
				float theta = Vector2.Angle (Vector2.right, dir);
				int directionIndex = Mathf.Clamp (Mathf.FloorToInt (theta / 60f), 0, 2);
				int scaleIndex = directionIndex;
				if (dir.y < 0) {
					directionIndex += 3;
					scaleIndex = 2 - scaleIndex;
				}
				Vector3 scaleDirection = directions [scaleIndex];
				Vector3 direction = directions [directionIndex];
				float stretch = Mathf.Floor (dir.magnitude / Screen.height * 13) * 2;
				selectionEnd = startVector + direction * stretch;
				indicator.transform.localScale = Vector3.one * scaleFactor + scaleDirection * ((startVector - selectionEnd).magnitude);
				indicator.transform.position = (startVector + selectionEnd) / 2;
				float s = stretch / 2 + 1;
				indicatorMaterial.SetTextureScale ("_MainTex", new Vector2 (s, s));
			}
		} else if(connecting){
			Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit = new RaycastHit ();
			Vector3 startPos = currentDoor.position.ToVector ();
			Vector3 endPos = Vector3.zero;
			GameObject prevButton = currentButton;
			currentButton = null;
			if (Physics.Raycast (cameraRay, out hit)) {
				if (hit.collider.tag == "Button") {
					currentButton = hit.collider.gameObject;
					endPos = hit.collider.bounds.center;
				} else {
					endPos = hit.point;
				}
			} else {
				endPos = cameraRay.GetPoint (Vector3.Distance (startPos, transform.position));
			}

			line.SetPosition (0, startPos);
			line.SetPosition (1, endPos);

			if (currentButton != null && prevButton == null) {
				currentButton.GetComponent<Renderer> ().material.color = new Color (0.5f, 0.1f, 0.7f);
			}
			if (currentButton == null && prevButton != null) {
				prevButton.GetComponent<Renderer> ().material.color = new Color (0.7f, 0.7f, 0.7f);
			}

			if (Input.GetMouseButton (0)) {
				if (currentButton == null) {
					blocksInScene.Remove (currentDoor.position);
					Destroy (currentDoor.obj);
				} else {
					currentDoor.index = blocksInScene [new int3 (currentButton.transform.position)].index;
					currentButton.GetComponent<Renderer> ().material.color = new Color (0.7f, 0.7f, 0.7f);
				}
				connecting = false;
				line.enabled = false;
			}
		} else {
			Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit = new RaycastHit ();
			if (Physics.Raycast (cameraRay, out hit)) {
				if (hit.collider.name == "BackCollider") {
					selectionStart = getTile (hit.point - hit.normal);
				} else {
					selectionStart = getTile (hit.point + hit.normal);
				}
			}else{
				indicator.SetActive (false);
				arrow.SetActive (false);
				return;
			}
			selectionEnd = selectionStart.ToVector ();
			indicator.transform.position = selectionStart.ToVector ();
			indicator.SetActive (true);
			arrow.SetActive (true);
			if (Input.GetMouseButton(0)) {
				placing = true;
			}
			if (Input.GetMouseButton(1)) {
				selectionStart = getTile (hit.point - hit.normal);
				selectionEnd = selectionStart.ToVector ();
				deleting = true;
			}
		}
		if (Input.GetButtonDown ("RotateSpell1")) {
			currentDirection -= (byte)Mathf.RoundToInt(Input.GetAxisRaw ("RotateSpell1"));
			currentDirection = (byte)((currentDirection + 4) % 4);
			arrow.transform.rotation = Quaternion.LookRotation (blockDirections [currentDirection].ToVector());
		}
		arrow.transform.position = //indicator.transform.position + 
			indicator.transform.TransformPoint(blockDirections [currentDirection].ToVector () * 0.5f) +
			blockDirections [currentDirection].ToVector () * 0.66f;
	}

	private void MakeBlock(int3 pos, block type, byte dir, int index = -1){
		GameObject blockInstance = Instantiate (type.model, pos.ToVector(), Quaternion.identity);
		blockInstance.transform.rotation = Quaternion.LookRotation (blockDirections [dir].ToVector());
		blockInstance.GetComponent<Renderer> ().material.color = type.color;
		blockData instance = new blockData (type, pos, dir, blockInstance);
		blocksInScene.Add (pos, instance);
		if (type.name == "button") {
			instance.index = buttonCount;
			buttonCount++;
		} else {
			Instantiate (backCollider, pos.ToVector (), Quaternion.identity).transform.SetParent (blockInstance.transform);
		}
		if (type.name == "door") {
			currentDoor = instance;
			line.enabled = true;
			connecting = true;
		}
	}

	private int3 getTile(Vector3 pos){
		return new int3 (
			Mathf.RoundToInt (pos.x / 2) * 2,
			Mathf.RoundToInt (pos.y / 2) * 2,
			Mathf.RoundToInt (pos.z / 2) * 2);
	}

	public void Load(){
		Clear ();
		string s = System.IO.File.ReadAllText("Assets/Resources/" + loadInput.text + ".txt");
		saveInput.text = loadInput.text;
		foreach (string line in s.Split('\n')) {
			string[] item = line.Split (',');
			if (item [0] == "spells") {
				AddSpellLine ();
				spellLines[spellLines.Count-1].GetComponent<SpellPanel>().SetSpells(line);
			} else if (item [0] == "prereq") {
				AddLevelLine ();
				levelLines[levelLines.Count-1].GetComponent<LevelPanel>().SetText(item[1]);
			} else {
				int3 pos = new int3 (
					          int.Parse (item [0]),
					          int.Parse (item [1]),
					          int.Parse (item [2]));
				byte dir = item.Length < 5 ? (byte)0 : byte.Parse (item [4]);
				foreach (block b in blocktypes) {
					if (b.name == item [3]) {
						MakeBlock (pos, b, dir,int.Parse(item[5]));
						break;
					}
				}
			}
		}
	}

	private void Clear(){
		foreach (blockData data in blocksInScene.Values) {
			Destroy (data.obj);
		}
		while (spellLines.Count > 0) {
			RemoveSpellLine (spellLines [0]);
		}
		while (levelLines.Count > 0) {
			RemoveLevelLine (levelLines [0]);
		}
		blocksInScene.Clear ();
	}

	public void Save(){
		string s = "";
		foreach (int3 pos in blocksInScene.Keys) {
			blockData data = blocksInScene [pos];
			s += string.Format ("{0},{1},{2},{3},{4},{5}\n",
				pos.x.ToString (),
				pos.y.ToString (),
				pos.z.ToString (),
				data.blockType.name,
				data.direction.ToString(),
				data.index.ToString()
			);
		}
		foreach (GameObject obj in spellLines) {
			s += obj.GetComponent<SpellPanel> ().GetSpells () + "\n";
		}
		foreach (GameObject obj in levelLines) {
			s += "prereq," + obj.GetComponent<LevelPanel> ().GetText () + "\n";
		}
		s = s.TrimEnd ('\n');
		System.IO.File.WriteAllText("Assets/Resources/" + saveInput.text + ".txt", s);
	}
}
