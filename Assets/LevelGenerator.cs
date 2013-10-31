using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Side{E, L, R}

public class Section {
	public bool spikeLeft;
	public bool spikeRight;
}

public class LevelGenerator : MonoBehaviour {

	float globalSpeedModifier = 1.0f;

	Queue<Section> gen = new Queue<Section>();

	float speed = 4.0f;
	float grav = 4.0f;

	Object spikeLeft;
	Object spikeRight;

	float cursor = -10;
	GameObject lastObject;

	float timeLast;
	float timeGap = 0.35f;
	
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
		return speed * globalSpeedModifier;
	}

	public float GetGravity() {
		return grav * globalSpeedModifier;
	}

	void Update() {

		if (Time.time > timeLast + (timeGap / globalSpeedModifier)) {
			timeLast = Time.time;

			// Only add more to the generation queue if it is empty
			if (GenEmpty()) {
				Side nextSide = Side.E;
				float v = Random.value;

				// Based upon our current side, work out which side to be next
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

				// Add things to the queue to be generated
				currentSide = nextSide;
				switch (currentSide) {
				case Side.E:
					GenSpikes(true, true);
					GenSpikes(false, false);
					GenSpikes(false, false);
					break;
				case Side.L:
					GenSpikes(false, true);
					GenSpikes(false, false);
					break;
				case Side.R:
					GenSpikes(true, false);
					GenSpikes(false, false);
					break;
				}
			}

			// Generate the thing at the front of the queue and remove it
			GenGenerate();
		}
	}

	void GenSpikes(bool spikeLeft, bool spikeRight) {
		Section s = new Section();
		s.spikeLeft = spikeLeft;
		s.spikeRight = spikeRight;
		gen.Enqueue(s);
	}

	bool GenEmpty() {
		return gen.Count == 0;
	}

	void GenGenerate() {
		Section s = gen.Dequeue();
		if (s.spikeLeft)
			CreateSpikeLeft();
		if (s.spikeRight)
			CreateSpikeRight();
	}

	void CreateSpikeLeft() {
		GameObject obj = (GameObject)GameObject.Instantiate(spikeLeft, posLeft, Quaternion.identity);
		obj.GetComponent<Spike>().lg = this;
	}

	void CreateSpikeRight() {
		GameObject obj = (GameObject)GameObject.Instantiate(spikeRight, posRight, Quaternion.identity);
		obj.GetComponent<Spike>().lg = this;
	}

	void CreateGapNext(int gapSize) {
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