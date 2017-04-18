using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class World : MonoBehaviour {

	//# of cells on x axis
	public int worldW = 16;
	//# of cells on z axis
	public int worldH = 16;
	//layers: 0:non-passable, 1:passable
	public int worldLayers = 2;
	//width of each cell
	public float cellWidth = 1f;

	//3D array for world occupation data
	//column, row, layer
	public WorldDynamic[,,] grid;

	public static World instance;

	Color gridColor;

	// Use this for initialization
	void Awake () {
		instance = this;
		grid = new WorldDynamic[worldW, worldH, worldLayers];
		gridColor = Color.white;
		gridColor.a = 0.25f;
	}


	// Update is called once per frame
	void Update () {
		DrawLines ();
	}

	//Draw grid
	void DrawLines(){
		for(int i = 0; i<=worldH; i++)
			Debug.DrawLine(new Vector3(0, 0, i)*cellWidth, new Vector3(worldW, 0, i)*cellWidth, gridColor);
		for(int i = 0; i<=worldW; i++)
			Debug.DrawLine(new Vector3(i, 0, 0)*cellWidth, new Vector3(i, 0, worldH)*cellWidth, gridColor);
	}

	//Check if target position (col, row) is within grid bounds
	bool IsInBounds(int[] tar){
		if (tar [0] < 0 || tar [0] >= worldW || tar[1] < 0 || tar[1] >= worldH) {
			Debug.Log ("Position (" + tar [0] + ", " + tar [1] + ") is out of bounds.");
			return false;
		}
		return true;
	}

	//Get object at pos(col, row) + layer from the grid
	WorldDynamic GetObject(int[] pos, int layer){
		if (!IsInBounds (pos))
			return null;
		
		return grid [pos [0], pos [1], layer];
	}

	//Remove any objects at pos[] + layer from the grid
	void Clear(int[] pos, int layer){
		if (!IsInBounds (pos))
			return;
		grid [pos [0], pos [1], layer] = null;
	}

	//Force set object at pos[] + layer in the grid
	void SetObject(WorldDynamic tar, int[] pos){
		if (!IsInBounds (pos))
			return;	
		grid [pos [0], pos [1], tar.layer] = tar;
	}

	//Destroys any object at pos[] + layer and removes it from the grid
	void DestroyObject(int[] pos, int layer){
		if (!IsInBounds (pos))
			return;
		
		GetObject (pos, layer).Destroy ();
		Clear (pos, layer);
	}

	//Destroys any object at to[] + tar's layer and removes it from the grid and moves tar to its position in grid and world space
	public void ForceMoveObject(WorldDynamic tar, int[] to){
		if (!IsInBounds (to))
			return;
		
		DestroyObject (to, tar.layer);
		MoveObject (tar, to);
	}

	//Object to move, position to move to
	//should only be used to move 1 unit, movement is smoothed (visually)
	public bool WalkObject(WorldDynamic tar, int[] to){
		if (!IsInBounds (to))
			return false;

		if (GetObject (to, tar.layer) != null) {
			return false;
		} 
		// If moving diagonally
		if (to [0] - tar.gridPos [0] != 0 && to [1] - tar.gridPos [1] != 0) {
			if (!isDiagonalClear(new int[]{tar.gridPos[0],tar.gridPos[1]}, to))
				return false;
		}
		Clear (tar.gridPos, tar.layer);
		SetObject (tar, to);
		tar.MoveToPos (to, true);
		return true;
	}

	//Does either cell adjacent to both the start/end position of a diagonal movement contain a non-passable object?
	public bool isDiagonalClear(int[] from, int[] to){
		if (GetObject (new int[]{ to [0] - (to [0] - from [0]), to [1] }, 0) != null) {
			return false;
		} if (GetObject (new int[]{ to [0], to [1] - (to [1] - from [1]) }, 0) != null) {
			return false;
		}
		return true;
	}
		
	//Object to move, position to move to - any position, no smoothed movement visual
	public void MoveObject(WorldDynamic tar, int[] to){
		if (!IsInBounds (to))
			return;

		if (GetObject (to, tar.layer) != null) {
			Debug.Log ("Cannot move " + tar.name);
			return;
		} 

		//Debug.Log ("Moving " + tar + " to (" + to [0] + ", " + to [1] + ")");
		Clear (tar.gridPos, tar.layer);
		SetObject (tar, to);
		tar.MoveToPos (to, false);

	}

	//Attempt to move to position
	//Else, attempt to move to adjacent positions
	public bool WalkObjectToNearest(WorldDynamic tar, int[] to){
		if (WalkObject (tar, to))
			return true;

		int rando = Random.Range (0, 2);
		if (rando == 0) {
			int[] tryTo = new int[]{ to [0] - (to [0] - tar.gridPos [0]), to [1] };
			if(WalkObject (tar, tryTo))
				return true;
		
			tryTo = new int[]{ to [0], to [1] - (to [1] - tar.gridPos [1]) };
			if(WalkObject (tar, tryTo))
				return true;
		} else {
			int[] tryTo = new int[]{ to [0], to [1] - (to [1] - tar.gridPos [1]) };
			if(WalkObject (tar, tryTo))
				return true;

			tryTo = new int[]{ to [0] - (to [0] - tar.gridPos [0]), to [1] };
			if(WalkObject (tar, tryTo))
				return true;
		}
		return false;
	}

	//If a WorldInteractive object exists on any layer at pos, activate it.
	public bool Activate(int[] pos){
		WorldDynamic tar;
		for (int i = 0; i < worldLayers; i++) {
			tar = GetObject (pos, i);
			if (tar != null) {
				if (tar is WorldInteractive) {
					//Debug.Log ("is interactive");
					if ((tar as WorldInteractive).Activate ()) {
						return true;
					}
				}
			}
		}
		return false;
	}

	//Change a WorldDynamic's layer and update its position in the grid
	public bool SwitchLayer(WorldDynamic tar){
		if(GetObject(tar.gridPos, Mathf.Abs(tar.layer-1))!=null)
			return false;
		Clear(tar.gridPos, tar.layer);
		tar.layer = Mathf.Abs(tar.layer-1);
		SetObject(tar, tar.gridPos);
		return true;
	}
}
