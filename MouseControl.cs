//Author: Dalton M. Techmanski
//Represents a cursor/tile marker on a grid

using UnityEngine;
using System.Collections;

public class MouseControl : MonoBehaviour {

	bool onCooldown;
	WorldDynamic wd;
	Camera camera;
	Vector3 worldPos;
	int[] gridPos;
	MeshRenderer render;
	bool isVisible = true;

	// Use this for initialization
	void Awake () {
		wd = GetComponentInParent<WorldDynamic> ();
		camera = Camera.main;
		render = GetComponent<MeshRenderer> ();
	}

	// Update is called once per frame
	void Update () {
		//Get grid position from mouse (x, y) + distance to world
		worldPos = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(camera.transform.localPosition.z)));
		gridPos = new int[] { 
			Mathf.RoundToInt (worldPos.x - 0.5f),
			Mathf.RoundToInt (worldPos.z - 0.5f)
		};
		//Force cursor grid position within 1 unit of player grid position
		gridPos = new int[] {
			Mathf.Clamp (gridPos [0], wd.gridPos [0] - 1, wd.gridPos [0] + 1),
			Mathf.Clamp (gridPos [1], wd.gridPos [1] - 1, wd.gridPos [1] + 1)
		};
		//Update position in world space
		transform.position = new Vector3 (gridPos[0]+0.5f, transform.position.y, gridPos[1]+0.5f);

		//Make cursor visible then check for conditions that wound render it invisible
		if (!isVisible)
			isVisible = true;
		if (World.instance.grid [gridPos [0], gridPos [1], 0] != null)
			isVisible = false;
		if (gridPos [0] - wd.gridPos [0] != 0 && gridPos [1] - wd.gridPos [1] != 0) {
			if (!World.instance.isDiagonalClear(wd.gridPos, gridPos))
				isVisible = false;
		}
		render.enabled = isVisible;

		//Placeholder cooldown- to be replaced by turn-based system
		if (!onCooldown) {
			if (Input.GetMouseButtonDown (0)) {
				if(World.instance.WalkObject (wd, gridPos)){
					StartCoroutine ("StartCooldown");
					return;
				}
				else if(World.instance.Activate(gridPos)){
					StartCoroutine ("StartCooldown");
					return;
				}
			}
		}
	}


	IEnumerator StartCooldown(){
		onCooldown = true;
		yield return new WaitForSeconds (0.6f);
		onCooldown = false;
	}
}
