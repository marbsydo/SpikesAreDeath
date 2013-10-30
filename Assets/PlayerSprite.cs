using UnityEngine;
using System.Collections;

public class PlayerSprite : MonoBehaviour {
	void Update() {

		float n = 1.0f / 16.0f;

		Vector3 p = transform.parent.transform.position;
		p.x = RoundNear(p.x, n);
		p.y = RoundNear(p.y, n);
		transform.position = p;
	}

	float RoundNear(float x, float n) {
		return Mathf.Round(x / n) * n;
	}
}
