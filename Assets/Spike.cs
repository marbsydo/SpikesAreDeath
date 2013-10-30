using UnityEngine;
using System.Collections;

public class Spike : MonoBehaviour {

	public LevelGenerator lg;

	void FixedUpdate() {
		rigidbody.MovePosition(rigidbody.position + new Vector3(0, Time.fixedDeltaTime * lg.speed, 0));

		if (transform.position.y < -5) {
			Destroy(gameObject);
		}
	}
}
