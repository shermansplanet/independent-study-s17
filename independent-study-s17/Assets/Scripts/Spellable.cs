using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spellable : MonoBehaviour {
	virtual public void ApplySpell (SpellManager.spell spellType, Vector3 casterPosition) {}
}
