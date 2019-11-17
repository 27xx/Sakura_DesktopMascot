using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ProgramIcon : MonoBehaviour {

	public Renderer iconVisual;
	public string filePath;

	Transform iconParent;
	bool mouseOver = false;

	// Use this for initialization
	void Start () {
		filePath = filePath.Replace("\"", "");
		if (File.Exists(filePath)) {
			iconVisual.material.mainTexture = Rainity.GetFileIcon(filePath);
			//border.material.SetColor("_Color", Rainity.GetAverageColorOfTexture((Texture2D)iconVisual.material.mainTexture));
		} else {
			UnityEngine.Debug.Log("Please assign a valid file path to the ProgramIcon script on " + transform.name + ".");
		}

		iconParent = transform.GetChild(0);
	}
	
	// Update is called once per frame
	void Update () {
		iconParent.position = Vector3.Lerp(iconParent.position, new Vector3(transform.position.x, -0.5f * (mouseOver ? 1 : 0), transform.position.z), Time.deltaTime * 5);
	}

	private void OnMouseEnter() {
		mouseOver = true;
	}

	private void OnMouseExit() {
		mouseOver = false;
	}

	private void OnMouseOver() {
		if (Input.GetButtonDown("Fire1")) {
			Rainity.OpenFile(filePath);
		}
	}
}
