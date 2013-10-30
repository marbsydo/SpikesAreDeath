using UnityEngine;
using System.Collections;

public class PlayerCollider : MonoBehaviour {

	public bool isGrounded = false;
	public bool wasGrounded = false;

	void Update() {
		wasGrounded = isGrounded;
		isGrounded = false;
	}

	public bool IsGrounded() {
		return isGrounded || wasGrounded;
	}

	void OnTriggerStay(Collider collider) {
		if (collider.gameObject.CompareTag("Wall")) {
			isGrounded = true;
		}
	}
}
