using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	float grav = 4.0f;
	float jump = 1.8f;
	bool gravDirection = false;

	public PlayerCollider colliderLeft;
	public PlayerCollider colliderRight;

	void Update() {

		bool jump = false, flip = false;

		if (Input.GetKeyDown(KeyCode.UpArrow))
			jump = true;

		if (Input.GetKeyDown(KeyCode.DownArrow))
			flip = true;

		// Touch controls
		foreach (Touch touch in Input.touches) {
			//if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled) {
			if (touch.phase == TouchPhase.Began) {
				if (touch.position.x < Screen.width / 2) {
					// Clicked on the left
					if (rigidbody.position.x < 0)
						jump = true;
					else
						flip = true;
				} else {
					// Clicked on the right
					if (rigidbody.position.x < 0)
						flip = true;
					else
						jump = true;
				}
			}
		}

		if (jump)
			Jump();
		
		if (flip)
			Flip();
		}

	void FixedUpdate() {
		// Apply gravity
		rigidbody.AddForce(new Vector3(grav * (gravDirection ? 1.0f : -1.0f) * grav, 0, 0), ForceMode.Acceleration);
	}

	void Jump() {
		if (IsGrounded())
			rigidbody.AddForce(new Vector3(grav * (gravDirection ? -1.0f : 1.0f) * jump, 0, 0), ForceMode.VelocityChange);
	}

	void Flip() {
		if (IsGrounded()) {
			Jump();
			gravDirection = !gravDirection;
		}
	}

	bool IsGrounded() {
		//return (colliderLeft.IsGrounded() || colliderRight.IsGrounded());
		return ((colliderLeft.IsGrounded() && !gravDirection) || (colliderRight.IsGrounded() && gravDirection));
	}
	
	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag("Death")) {
			Debug.Log("Died");
			Application.LoadLevel(Application.loadedLevelName);
		}
	}
}
