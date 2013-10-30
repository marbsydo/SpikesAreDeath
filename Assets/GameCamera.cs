using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour {

	double roomWidth = 5.0;

	void Update() {
		camera.orthographicSize = (float)((roomWidth / 2.0) / (double)Screen.width * (double)Screen.height) - 0.05f;

		camera.transform.position = new Vector3(0, 3, -10);
	}
}
