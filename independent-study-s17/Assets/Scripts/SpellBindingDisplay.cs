using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellBindingDisplay : MonoBehaviour {

	public int height;
	public RawImage childImage;
	public Image image;

	private RectTransform rect;
	private float yPos = 0;
	private bool spawning = true;

	public void Init (SpellManager.spell spell) {
		rect = GetComponent<RectTransform> ();
		yPos = -height * 100;
		image = GetComponent<Image> ();
		childImage = transform.GetChild (0).GetComponent<RawImage> ();
		if (SpellManager.textureDict.ContainsKey (spell)) {
			childImage.texture = SpellManager.textureDict [spell];
		}
	}
	
	public IEnumerator ShiftUp(int delta){
		float newPos = yPos + delta * 100;
		bool toDestroy = newPos/100 + height > Player.activeSpellLimit + 0.1f;
		while (yPos < newPos) {
			yPos += Time.deltaTime * 1000;
			SetPosition ();
			float amountLeft = (newPos - yPos) / (delta * 100);
			if (toDestroy) {
				Color c = image.color;
				c.a = amountLeft;
				image.color = c;
				c = childImage.color;
				c.a = amountLeft;
				childImage.color = c;
			} else if (spawning) {
				Color c = image.color;
				c.a = 1 - amountLeft;
				image.color = c;
				c = childImage.color;
				c.a = 1 - amountLeft;
				childImage.color = c;
			}
			yield return null;
		}
		yPos = newPos;
		SetPosition ();
		if (toDestroy) {
			Destroy (gameObject);
		}
		spawning = false;
	}

	private void SetPosition(){
		rect.offsetMin = new Vector2 (-75, yPos);
		rect.offsetMax = new Vector2 (75, yPos + height * 100);
	}
}
