using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Side{E, L, R}
public enum Difficulty{SuperEasy, Easy, Medium, Hard, SuperHard}

public class Section {
	public bool spikeLeft;
	public bool spikeRight;
}

public class LevelGenerator : MonoBehaviour {

	Queue<Section> gen = new Queue<Section>();
	List<SectionBlueprint> blueprints = new List<SectionBlueprint>();

	float speed = 4.0f;
	float grav = 4.0f;

	Object spikeLeft;
	Object spikeRight;

	GameObject lastObject;

	float timeLast;
	
	Vector3 posLeft = new Vector3(-2f, -10, 0);
	Vector3 posRight = new Vector3(2f, -10, 0);

	public Side currentSide = Side.E;
	Dictionary<Side, float> sideWeights = new Dictionary<Side, float>();
	Dictionary<Difficulty, float> difficultyWeights = new Dictionary<Difficulty, float>();

	// Level generation modifiers
	float timeGap = 0.35f;                                  // The time in seconds between generating spikes. Minimum = 0.35f
	float globalSpeedModifier = 1.0f;                       // Affects speed of all game related objects including spikes and player
	float spikeDensity = 1.0f;                              // 1.0f = maximum density spikes, 0.0f = no spikes
	bool changeWeightsToMakeSideSwappingMoreLikely = false; // If true, sequences of multiple identical spikes are made less unlikely

	void Awake() {
		spikeLeft = Resources.Load("SpikeLeft");
		spikeRight = Resources.Load("SpikeRight");

		// Weights for which side the player should be on
		sideWeights.Add(Side.E, 1.0f);
		sideWeights.Add(Side.L, 1.0f);
		sideWeights.Add(Side.R, 1.0f);

		// Weights for various difficults of spike generation
		difficultyWeights.Add(Difficulty.SuperEasy, 1.0f);
		difficultyWeights.Add(Difficulty.Easy,      0.0f);
		difficultyWeights.Add(Difficulty.Medium,    0.0f);
		difficultyWeights.Add(Difficulty.Hard,      0.0f);
		difficultyWeights.Add(Difficulty.SuperHard, 0.0f);

		// Create blueprints
		AddBlueprint(Side.E, Side.E, Difficulty.SuperEasy, "n");
		AddBlueprint(Side.L, Side.L, Difficulty.SuperEasy, "nrn");
		AddBlueprint(Side.R, Side.R, Difficulty.SuperEasy, "nln");
		AddBlueprint(Side.L, Side.L, Difficulty.SuperEasy, "nrrn");
		AddBlueprint(Side.R, Side.R, Difficulty.SuperEasy, "nlln");
		AddBlueprint(Side.E, Side.E, Difficulty.SuperEasy, "nbnn");
		AddBlueprint(Side.E, Side.E, Difficulty.SuperEasy, "nrbn");
		AddBlueprint(Side.E, Side.E, Difficulty.SuperEasy, "nlbn");
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
					// Call AddSomeSpikes to add some spikes
					// Tell it what the current side is
					// It will then return the new side we are at
					currentSide = AddSomeSpikes(currentSide);
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

			//Debug.Log("E: " + sideWeights[Side.E] + " L: " + sideWeights[Side.L] + " R: " + sideWeights[Side.R]);
		}
	}

	Side AddSomeSpikes(Side thisSide) {
		Side nextSide = Side.E;
		Difficulty chosenDifficulty = Difficulty.Medium;

		// Choose what difficulty this section will be based upon the weights
		float w_se = difficultyWeights[Difficulty.SuperEasy];
		float w_e = difficultyWeights[Difficulty.Easy];
		float w_m = difficultyWeights[Difficulty.Medium];
		float w_h = difficultyWeights[Difficulty.Hard];
		float w_sh = difficultyWeights[Difficulty.SuperHard];
		float v = Random.value * (w_se + w_e + w_m + w_h + w_sh);

		if (v < w_se) {
			chosenDifficulty = Difficulty.SuperEasy;
		} else if (v < (w_se + w_e)) {
			chosenDifficulty = Difficulty.Easy;
		} else if (v < (w_se + w_e + w_m)) {
			chosenDifficulty = Difficulty.Medium;
		} else if (v < (w_se + w_e + w_m + w_h)) {
			chosenDifficulty = Difficulty.Hard;
		} else {
			chosenDifficulty = Difficulty.SuperHard;
		}

		// We have chosen a difficulty
		// Now find all blueprints that match this difficulty and side
		List<int> goodBlueprints = new List<int>();
		for (int i = 0; i < blueprints.Count; i++) {
			if (blueprints[i].difficulty == chosenDifficulty && blueprints[i].sideIn == thisSide)
				goodBlueprints.Add(i);
		}

		// Choose a random blueprint from the list of good blueprints
		if (goodBlueprints.Count == 0) {
			// Found no blueprints; show warning and generate no spikes
			Debug.LogWarning("Could not find any appropriate blueprints!");

			GenSpikes(false, false);
			nextSide = Side.E;
		} else {
			// Choose a random blueprint from the list of IDs in goodBlueprints
			SectionBlueprint chosenBlueprint = blueprints[goodBlueprints[Random.Range(0, goodBlueprints.Count)]];

			chosenBlueprint.Generate();
			nextSide = chosenBlueprint.sideOut;
		}

		return nextSide;
	}

	public void GenSpikes(bool spikeLeft, bool spikeRight) {
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

	void AddBlueprint(Side sideFrom, Side sideTo, Difficulty difficulty, string spikes) {
		blueprints.Add(new SectionBlueprint(this, sideFrom, sideTo, difficulty, spikes));
	}
}

class SectionBlueprint {
	private LevelGenerator lg;
	public Side sideIn;
	public Side sideOut;
	public Difficulty difficulty;
	private string spikes;

	// e.g. new SectionBlueprint(Side.L, Side.R, Difficulty.Medium, "rrnbnnbnll")
	public SectionBlueprint(LevelGenerator lg, Side sideIn, Side sideOut, Difficulty difficulty, string spikes) {
		this.lg = lg;
		this.sideIn = sideIn;
		this.sideOut = sideOut;
		this.difficulty = difficulty;
		this.spikes = spikes;
	}

	public void Generate() {
		// Use spikes to generate spikes
		// Spikes is e.h. "lnrnnbnn"

		for (int i = 0; i < spikes.Length; i++) {
			switch (spikes[i]) {
			case 'l':	lg.GenSpikes(true, false);		break;
			case 'n':	lg.GenSpikes(false, false);		break;
			case 'r':	lg.GenSpikes(false, true);		break;
			case 'b':	lg.GenSpikes(true, true);		break;
			}
		}
	}
}