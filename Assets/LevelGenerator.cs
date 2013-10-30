using UnityEngine;
using System.Collections;

public class LevelGenerator : MonoBehaviour {

	float speed = 4.0f;
	float jumpGap = 2.0f;
	float landGap = 2.0f;
	float flipGap = 1.0f;

	Object spikeLeft;
	Object spikeRight;

	float timeLast;
	float timeGap = 0.5f;
	
	Vector3 posLeft = new Vector3(-2f, -10, 0);
	Vector3 posRight = new Vector3(2f, -10, 0);

	void Awake() {
		spikeLeft = Resources.Load("SpikeLeft");
		spikeRight = Resources.Load("SpikeRight");
	}

	public float GetSpeed() {
		return speed;
	}

	void OnLevelWasLoaded() {
		timeLast = Time.time;
	}

	void Update() {
		if (Time.time > timeLast + timeGap) {
			timeLast = Time.time;
			GameObject obj;
			if (Random.value < 0.5f)
				obj = (GameObject)GameObject.Instantiate(spikeLeft, posLeft, Quaternion.identity);
			else
				obj = (GameObject)GameObject.Instantiate(spikeRight, posRight, Quaternion.identity);
			obj.GetComponent<Spike>().lg = this;
		}
	}

}
