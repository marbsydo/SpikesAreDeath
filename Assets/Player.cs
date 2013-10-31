using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	float grav;// = 4.0f;
	float jump = 1.8f;
	bool gravDirection = false;

	public PlayerCollider colliderLeft;
	public PlayerCollider colliderRight;

	LevelGenerator lg;

	float pressedJumpAt = 0;
	float pressedFlipAt = 0;
	float earlyButtonPressGraceTime = 0.2f;

	float actuallyJumpAt = 0;
	float actuallyFlipAt = 0;
	float actuallyJumpCoolDownTime = 0.1f;
	float actuallyFlipCoolDownTime = 0.1f;

	void Awake() {
		lg = GameObject.Find("LevelGenerator").GetComponent<LevelGenerator>() as LevelGenerator;
	}

	void Update() {

		grav = lg.GetGravity();

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

		if (IsGrounded()) {
			if (jump || Time.time < pressedJumpAt + earlyButtonPressGraceTime)
				if (Time.time > actuallyJumpAt + actuallyJumpCoolDownTime)
					Jump();	
			if (flip || Time.time < pressedFlipAt + earlyButtonPressGraceTime)
				if (Time.time > actuallyFlipAt + actuallyFlipCoolDownTime)
					Flip();
		} else {
			if (jump)
				pressedJumpAt = Time.time;
			if (flip)
				pressedFlipAt = Time.time;
		}
	}

	void FixedUpdate() {
		// Apply gravity
		rigidbody.AddForce(new Vector3(grav * (gravDirection ? 1.0f : -1.0f) * grav, 0, 0), ForceMode.Acceleration);
	}

	void Jump() {
		actuallyJumpAt = Time.time;
		rigidbody.AddForce(new Vector3(grav * (gravDirection ? -1.0f : 1.0f) * jump, 0, 0), ForceMode.VelocityChange);
	}

	void Flip() {
		actuallyFlipAt = Time.time;
		Jump();
		gravDirection = !gravDirection;
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
