using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Side{E, L, R}

public class Section {
	public bool spikeLeft;
	public bool spikeRight;
}

public class LevelGenerator : MonoBehaviour {

	

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

	Dictionary<Side, float> sideWeights = new Dictionary<Side, float>();
	public Side currentSide = Side.E;

	// Level generation modifiers
	float globalSpeedModifier = 1.0f;                       // Affects speed of all game related objects including spikes and player
	float spikeDensity = 1.0f;                              // 1.0f = maximum density spikes, 0.0f = no spikes
	bool changeWeightsToMakeSideSwappingMoreLikely = false; // If true, sequences of multiple identical spikes are made less unlikely

	void Awake() {
		spikeLeft = Resources.Load("SpikeLeft");
		spikeRight = Resources.Load("SpikeRight");

		sideWeights.Add(Side.E, 1.0f);
		sideWeights.Add(Side.L, 1.0f);
		sideWeights.Add(Side.R, 1.0f);
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
					v *= (sideWeights[Side.E] + sideWeights[Side.L] + sideWeights[Side.R]);
					if (v < sideWeights[Side.E])
						nextSide = Side.E;
					else if (v < (sideWeights[Side.E] + sideWeights[Side.L]))
						nextSide = Side.L;
					else
						nextSide = Side.R;
				}

				if (currentSide == Side.L) {
					v *= (sideWeights[Side.E] + sideWeights[Side.L]);
					if (v < sideWeights[Side.E])
						nextSide = Side.E;
					else
						nextSide = Side.L;
				}

				if (currentSide == Side.R) {
					v *= (sideWeights[Side.E] + sideWeights[Side.R]);
					if (v < sideWeights[Side.E])
						nextSide = Side.E;
					else
						nextSide = Side.R;
				}

				// Add things to the queue to be generated
				currentSide = nextSide;
				v = Random.value;
				if (v <= spikeDensity) {
					v = Random.value;
					switch (currentSide) {
					case Side.E:
						GenSpikes(true, true);
						GenSpikes(false, false);
						GenSpikes(false, false);
						break;
					case Side.L:
						GenSpikes(false, true);
						/*
						GenSpikes(false, true);
						GenSpikes(true, false);
						GenSpikes(true, false);
						GenSpikes(false, true);
						GenSpikes(false, true);
						*/
						break;
					case Side.R:
						GenSpikes(true, false);
						break;
					}
				}

				// Change weights to make side swapping more likely
				if (changeWeightsToMakeSideSwappingMoreLikely) {
					if (currentSide == Side.L) {
						sideWeights[Side.L] = 0.1f;
						sideWeights[Side.R] = 1.0f;
					} else if (currentSide == Side.R) {
						sideWeights[Side.L] = 1.0f;
						sideWeights[Side.R] = 0.1f;
					}
				}
			}

			// Generate the thing at the front of the queue and remove it
			GenGenerate();

			Debug.Log("E: " + sideWeights[Side.E] + " L: " + sideWeights[Side.L] + " R: " + sideWeights[Side.R]);
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
		if (GenEmpty())
			return;

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
}