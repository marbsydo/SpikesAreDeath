using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Side{E, L, R}

public class LevelGenerator : MonoBehaviour {

	float speed = 4.0f;

	Object spikeLeft;
	Object spikeRight;

	float cursor = -10;
	GameObject lastObject;

	//Vector3 posLeft = new Vector3(-2f, -10, 0);
	//Vector3 posRight = new Vector3(2f, -10, 0);

	float timeLast;
	float timeGap = 0.5f;
	
	Vector3 posLeft = new Vector3(-2f, -10, 0);
	Vector3 posRight = new Vector3(2f, -10, 0);

	Side currentSide = Side.E;
	int gap = 0;

	void Awake() {
		spikeLeft = Resources.Load("SpikeLeft");
		spikeRight = Resources.Load("SpikeRight");
	}

	void OnLevelWasLoaded() {
		timeLast = Time.time;
	}

	public float GetSpeed() {
		return speed;
	}

	void Update() {
		if (Time.time > timeLast + timeGap) {
			timeLast = Time.time;

			if (gap > 0) {
				gap--;
			} else {
				Side nextSide = Side.E;
				float v = Random.value;

				if (currentSide == Side.E) {
					if (v < 0.33f)
						nextSide = Side.E;
					else if (v < 0.67f)
						nextSide = Side.L;
					else
						nextSide = Side.R;
				}

				if (currentSide == Side.L) {
					if (v < 0.5f)
						nextSide = Side.E;
					else
						nextSide = Side.L;
				}

				if (currentSide == Side.R) {
					if (v < 0.5f)
						nextSide = Side.E;
					else
						nextSide = Side.R;
				}

				v = Random.value;

				switch (nextSide) {
				case Side.E:
					CreateSpikeLeft();
					CreateSpikeRight();
					CreateGap(1);
					break;
				case Side.L:
					CreateSpikeRight();
					break;
				case Side.R:
					CreateSpikeLeft();
					break;
				}
			}
		}
	}

	void CreateSpikeLeft() {
		GameObject obj = (GameObject)GameObject.Instantiate(spikeLeft, posLeft, Quaternion.identity);
		obj.GetComponent<Spike>().lg = this;
	}

	void CreateSpikeRight() {
		GameObject obj = (GameObject)GameObject.Instantiate(spikeRight, posRight, Quaternion.identity);
		obj.GetComponent<Spike>().lg = this;
	}

	void CreateGap(int gapSize) {
		gap = gapSize;
	}

}
/*
public enum Side{E, L, R}

public abstract class SectionBase {
	protected Side sideIn;
	protected Side sideOut;

	List<Side> AllowedNext() {
		List<Side> st = new List<Side>();

		switch (sideOut) {
		case Side.E:
			st.Add(Side.E);
			st.Add(Side.L);
			st.Add(Side.R);
		break;
		case Side.L:
			st.Add(Side.E);
			st.Add(Side.L);
			break;
		case Side.R:
			st.Add(Side.E);
			st.Add(Side.R);
			break;
		}

		return st;
	}

	public abstract List<Vector3> GeneratePoints();
}

public class SectionEmpty : SectionBase {
	SectionEmpty() {
		sideIn = Side.E;
		sideOut = Side.E;
	}

	public override List<Vector3> GeneratePoints() {
		List<Vector3> p = new List<Vector3>();

		return p;
	}
}
*/