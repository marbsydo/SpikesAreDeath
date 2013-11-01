using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SADText : MonoBehaviour {

	public enum SADAlign {Left, Center, Right}

	private List<Object> spritePrefabs = new List<Object>();
	private List<GameObject> sprites = new List<GameObject>();

	public SADAlign align = SADAlign.Left;

	private string _text = "";
	public string text {
		get {
			return _text;
		}
		set {
			_text = value;
			UpdateSprites();
		}
	}

	void Awake() {
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum0"));
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum1"));
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum2"));
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum3"));
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum4"));
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum5"));
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum6"));
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum7"));
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum8"));
		spritePrefabs.Add(Resources.Load("SADFont/SpriteNum9"));

		UpdateSprites();
	}

	void UpdateSprites() {

		// Destroy all sprites
		foreach (GameObject sprite in sprites) {
			Destroy(sprite);
		}
		sprites.Clear();

		float xOffset = 0;
		if (align == SADAlign.Right)
			xOffset = (text.Length - 1) * -1.0f;
		if (align == SADAlign.Center)
			xOffset = (text.Length - 1) * -0.5f;

		for (int i = 0; i < text.Length; i++) {
			GameObject obj;
			switch (text[i]) {
			default:
			case '0':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[0], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			case '1':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[1], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			case '2':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[2], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			case '3':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[3], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			case '4':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[4], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			case '5':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[5], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			case '6':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[6], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			case '7':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[7], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			case '8':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[8], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			case '9':	obj = (GameObject) GameObject.Instantiate(spritePrefabs[9], transform.position + new Vector3(xOffset + i, 0, 0), Quaternion.identity);	break;
			}
			obj.transform.parent = transform;
			sprites.Add(obj);
		}
	}
}
