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
	};

	class int3{
		public int x, y, z;
		public int3(int x, int y, int z){
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public Vector3 ToVector(){
			return new Vector3 (x, y, z);
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

	class blockData{
		public block blockType;
		public int3 position;
		public int direction;
		public GameObject obj;
		/* 0: +y
		 * 1: +x
		 * 2: +z
		 * 3: -x
		 * 4: -z
		 * 5: -y
		 */
		public blockData(block blockType, int3 position, int direction, GameObject obj){
			this.blockType = blockType;
			this.position = position;
			this.direction = direction;
			this.obj = obj;
		}
	}

	public block[] blocktypes;
	public GameObject blockPrefab;
	public GameObject indicator;
	public GameObject startCube;
	public Text display;
	public InputField saveInput;
	public InputField loadInput;

	private bool placing = false;
	private bool deleting = false;
	private int3 selectionStart = new int3(0,0,0);
	private Vector3 selectionEnd = new Vector3();
	private Material indicatorMaterial;
	private int currentBlockType = 0;
	private Vector3 cameraOffset;
	private Quaternion originalRotation;

	private Dictionary<int3,blockData> blocksInScene = new Dictionary<int3,blockData>();

	readonly Vector3[] directions = new Vector3[]{Vector3.right,Vector3.up,Vector3.forward,Vector3.back,Vector3.down,Vector3.left};

	void Start(){
		blocksInScene.Add (new int3 (0, 0, 0), new blockData (blocktypes [0], new int3 (0, 0, 0), 0, startCube));
		startCube.GetComponent<Renderer> ().material.color = blocktypes [0].color;
		indicatorMaterial = indicator.GetComponent<Renderer> ().material;
		RefreshDisplay ();
		cameraOffset = transform.InverseTransformVector(transform.position);
		originalRotation = transform.rotation;
	}

	void RefreshDisplay(){
		string s = "";
		for (int i = 0; i < blocktypes.Length; i++) {
			block type = blocktypes [i];
			string line = "<color=" + ToRGBHex (type.color) + ">" + (i + 1).ToString () + ". " + type.name + "</color>\n";
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

	void Update () {

		if (!saveInput.isFocused && !loadInput.isFocused) {
			for (int i = 0; i < blocktypes.Length; i++) {
				if (Input.GetKeyDown ((i + 1).ToString ())) {
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
				for (int i = 0; i < blockCount; i++) {
					Vector3 pos = startVector + i * dirVector;
					int3 intPos = getTile (pos);
					if (!blocksInScene.ContainsKey (intPos)) {
						MakeBlock (intPos, blocktypes [currentBlockType]);
					}
				}
			}else if (deleting && !Input.GetMouseButton (1)) {
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
				float s = stretch/2 + 1;
				indicatorMaterial.SetTextureScale ("_MainTex", new Vector2 (s, s));
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
				return;
			}
			selectionEnd = selectionStart.ToVector ();
			indicator.transform.position = selectionStart.ToVector ();
			indicator.SetActive (true);
			if (Input.GetMouseButton(0)) {
				placing = true;
			}
			if (Input.GetMouseButton(1)) {
				selectionStart = getTile (hit.point - hit.normal);
				selectionEnd = selectionStart.ToVector ();
				deleting = true;
			}
		}
	}

	private void MakeBlock(int3 pos, block type){
		GameObject blockInstance = Instantiate (blockPrefab, pos.ToVector(), Quaternion.identity);
		blockInstance.GetComponent<Renderer> ().material.color = type.color;
		blocksInScene.Add (pos, new blockData (type, pos, 0, blockInstance));
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
		foreach (string line in s.Split('\n')) {
			string[] item = line.Split (',');
			int3 pos = new int3 (
				           int.Parse (item [0]),
				           int.Parse (item [1]),
				           int.Parse (item [2]));
			foreach (block b in blocktypes) {
				if (b.name == item [3]) {
					MakeBlock (pos, b);
					break;
				}
			}
		}
	}

	private void Clear(){
		foreach (blockData data in blocksInScene.Values) {
			Destroy (data.obj);
		}
		blocksInScene.Clear ();
	}

	public void Save(){
		string s = "";
		foreach (int3 pos in blocksInScene.Keys) {
			blockData data = blocksInScene [pos];
			s += string.Format ("{0},{1},{2},{3}\n",
				pos.x.ToString (),
				pos.y.ToString (),
				pos.z.ToString (),
				data.blockType.name);
		}
		s = s.TrimEnd ('\n');
		System.IO.File.WriteAllText("Assets/Resources/" + saveInput.text + ".txt", s);
	}
}
