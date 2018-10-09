using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

public class FieldController : MonoBehaviour {

	public Transform portalPair;
	public Transform player;
	public Transform exit;
	public Transform item;
	public Transform horiz_wall;
	public Transform vert_wall;


	private int[] portalTracker;
	private List<Vector3> positions;
	private List<Vector2> pairs;
	private ArrayList grid_spots;
	private ArrayList path;
	private int playerIndex;
	private int exitIndex;
	private int portalPairs;
	private int length;

	private enum HSV {Hue, Sat, Val};
	private HSV picked;
	private float pivot = 0f;
	private ArrayList fixed_params;
	private float hue;
	private float sat;
	private float val;
	private ArrayList free_choices;

	private string readPath;
	private string writePath;
	private List<string> readList = new List<string> ();
	private List<string> writeList = new List<string> ();

	// Use this for initialization
	void Start() {
		readPath = Application.dataPath + "/Log/gameStats.txt";
		writePath = Application.dataPath + "/Log/gameStats.txt";
		
	}

	void ReadFile(string filePath) {
		StreamReader sReader = new StreamReader (filePath);
		while(!sReader.EndOfStream) {
			string line = sReader.ReadLine ();
			readList.Add (line);
		}
		sReader.Close ();
	}

	void WriteFile(string filePath) {
		StreamWriter sWriter = new StreamWriter (filePath);
		if (!File.Exists(filePath)) {
			sWriter = File.CreateText (Application.dataPath + "/Log/gameStats.txt");
		} else {
			sWriter = new StreamWriter (filePath);
		}

		sWriter.Close ();
	}

	void AppendFile(string filePath, string myString){
		StreamWriter sAppender; 
		Debug.Log (filePath);
		if(!File.Exists(filePath)){
			sAppender = File.CreateText(filePath);
		}
		else{
			sAppender = new StreamWriter(filePath, append: true); 
			Debug.Log("opening existing file");
		}

		sAppender.WriteLine(myString);

		sAppender.Close(); 
	}

	void Awake() {
		writePath = Application.dataPath + "/Log/gameStats.txt";

		portalTracker = new int[]{2, 2, 2, 2};
		positions = new List<Vector3> ();
		pairs = new List<Vector2> ();
		grid_spots = new ArrayList ();
		path = new ArrayList ();
		free_choices = new ArrayList ();

		initWalls ();

		playerIndex = UnityEngine.Random.Range (0, grid_spots.Count);
		exitIndex = UnityEngine.Random.Range (0, grid_spots.Count);
		while(playerIndex == exitIndex) {
			exitIndex = UnityEngine.Random.Range(0, grid_spots.Count);
		}
		initPlayer (playerIndex);
		initExit (exitIndex);
		initPortals ();
		String str = DateTime.Now.ToString ("MM/dd/yyyy HH:mm:ss") + ", " 
			+ "Generating new level, " + length + ", " + portalPairs + ", " + grid_spots.Count;
		AppendFile (writePath, str);
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	void initBorders() {
		Vector2 topRightCorner = new Vector2(Screen.width, Screen.height);
		Vector2 edgeVector = Camera.main.ScreenToWorldPoint(topRightCorner);
		float h = edgeVector.y * 2;
		float w = edgeVector.x * 2;
		Transform hwall = Instantiate (horiz_wall, new Vector3(0, h / 2, 0), Quaternion.identity); 
		hwall.localScale += new Vector3 (w, 0, 0);
		hwall = Instantiate (horiz_wall, new Vector3(0, -1 * h / 2, 0), Quaternion.identity); 
		hwall.localScale += new Vector3 (w, 0, 0);
		Transform vwall = Instantiate (vert_wall, new Vector3(-1 * w / 2, 0, 0), Quaternion.identity); 
		vwall.localScale += new Vector3 (0, h, 0);
		vwall = Instantiate (vert_wall, new Vector3(w / 2, 0, 0), Quaternion.identity); 
		vwall.localScale += new Vector3 (0, h, 0);
	}

	void initWalls() {
		initBorders ();

		Vector2 topRightCorner = new Vector2(Screen.width, Screen.height);
		Vector2 edgeVector = Camera.main.ScreenToWorldPoint(topRightCorner);
		float h = edgeVector.y * 2;
		float w = edgeVector.x * 2;
		float unit_h = h / 3;
		float unit_w = w / 5;

		int num_vert_walls = UnityEngine.Random.Range (1, 5); // 1, 2, 3, 4, 5; columns = this + 1

		// Pick locations for vertical walls

		int[] widths = new int[6];
		for (int i = 0; i < num_vert_walls; i++) {
			int x_loc = UnityEngine.Random.Range (1, 5);
			while (widths[x_loc] == 1) {
				x_loc = UnityEngine.Random.Range (1, 5);
			}
			widths [x_loc] = 1;
		}
		widths [0] = 1;
		widths [5] = 1;

		// We want to go through the walls in sorted order now

		int vert_wall_iter = 0;
		int last_found = 0;
		while (vert_wall_iter < widths.Length) {
			if (widths [vert_wall_iter] == 0 || vert_wall_iter == 0) {
				vert_wall_iter += 1;
			} else {
				int x_loc = vert_wall_iter;
				float wall_loc = x_loc * unit_w - w / 2;
				Vector3 pos = new Vector3 (wall_loc, 0f, 0f);
				Transform genWall = Instantiate (vert_wall, pos, Quaternion.identity);
				genWall.localScale += new Vector3 (0, h, 0);

				float gap = (x_loc - last_found) * unit_w;

				// Generate horizontal walls within this column

				int[] heights = new int[4];
				int num_horiz_walls = UnityEngine.Random.Range (1, 3);
				heights [0] = 1;
				heights [3] = 1;

				for (int j = 0; j < num_horiz_walls; j++) {
					int h_loc = UnityEngine.Random.Range (1, 3);
					while (heights [h_loc] == 1) {
						h_loc = UnityEngine.Random.Range (1, 3);	
					}
					heights [h_loc] = 1;
				}
				int horiz_wall_iter = 0;
				int h_last_found = 0;
				while (horiz_wall_iter < heights.Length) {
					if (heights[horiz_wall_iter] == 0 || horiz_wall_iter == 0) {
						horiz_wall_iter += 1;
					} else {
						int h_loc = horiz_wall_iter;
						float v_gap = (h_loc - h_last_found) * unit_h;
						float h_wall_loc_y = h_loc * unit_h - h / 2;
						float h_wall_loc_x = wall_loc - gap / 2;
						Vector3 h_pos = new Vector3 (h_wall_loc_x, h_wall_loc_y, 0f);
						Transform h_genWall = Instantiate (horiz_wall, h_pos, Quaternion.identity);
						h_genWall.localScale += new Vector3 (gap, 0, 0);

						Vector2 topCorner = new Vector2 (wall_loc, h_wall_loc_y);
						Vector2 bottomCorner = new Vector2 (wall_loc - gap, h_wall_loc_y - v_gap);
						ArrayList grid_coords = new ArrayList ();
						grid_coords.Add (topCorner);
						grid_coords.Add (bottomCorner);
						grid_spots.Add (grid_coords);
						h_last_found = horiz_wall_iter;
						horiz_wall_iter += 1;
					}

				}
				last_found = vert_wall_iter;
				vert_wall_iter += 1;

			}
			
		}
	}

	void initPortals() {
		// first, pick one of {H,S,V}
		picked = (HSV) UnityEngine.Random.Range(0, 3);
		pivot = UnityEngine.Random.Range (.0f, .4f);
		pivot = pivot - .4f;
		if (picked == HSV.Hue) {
			sat = UnityEngine.Random.Range (.2f, .8f);
			val = UnityEngine.Random.Range (.2f, .8f);
			
		} else if (picked == HSV.Sat) {
			val = UnityEngine.Random.Range (.2f, .8f);
			hue = UnityEngine.Random.Range (.2f, .8f);
			
		} else {
			hue = UnityEngine.Random.Range (.2f, .8f);
			sat = UnityEngine.Random.Range (.2f, .8f);
		}

		// Construct list of values for free param



		for (int i = 0; i < 30; i++) {
			free_choices.Add (pivot);
			pivot += .02f;
		}

		// fix the other two values

		// fix a pivot point for the free parameter

		// first pick a path from player to exit

		length = UnityEngine.Random.Range (2, grid_spots.Count - 2);
		while (length % 2 != 0) {
			length = UnityEngine.Random.Range (2, grid_spots.Count - 2);
		}
		Debug.Log ("Num spots: " + grid_spots.Count);
		Debug.Log ("Length " + length);
		ArrayList enumerate = new ArrayList ();
		for (int i = 0; i < grid_spots.Count; i++) {
			if (i != playerIndex && i != exitIndex) {
				enumerate.Add (i);
			}
		}
		int index = 0;
		ArrayList everything_else = new ArrayList ();
		for (int i = 0; i < length; i++) {
			index = UnityEngine.Random.Range (0, enumerate.Count);
			Debug.Log ("Size of enumerate: " + enumerate.Count);
			Debug.Log ("index: " + index);
			path.Add (enumerate [index]);
			enumerate.RemoveAt (index);
		}

		path.Insert (0, playerIndex);
		everything_else.Add (playerIndex);
		everything_else.Add (exitIndex);
		path.Add (exitIndex);

		// At this point, path contains the grid spots that _need_ to be linked
		// everything_else contains everything else twice, and elements in path once
		// Link path up first, then add random portals

		for (int i = 0; i < path.Count - 1; i++) {
			initPortalPair ((int) path[i], (int) path[i+1]);
		}

		enumerate.Remove (playerIndex);
		enumerate.Remove (exitIndex);
		for (int i = 0; i < enumerate.Count; i++){
			everything_else.Add (enumerate [i]);
			everything_else.Add (enumerate [i]);
			if (UnityEngine.Random.Range(0, 2) == 0 && i > 1) {
				everything_else.Add (enumerate [i]);
				everything_else.Add (enumerate [i - 1]);
			}
		}

		int index_a = 0;
		int index_b = 0;

		int spot_a = 0;
		int spot_b = 0;

		while(everything_else.Count != 0) {
			index_a = UnityEngine.Random.Range (0, everything_else.Count);
			spot_a = (int) everything_else [index_a];
			everything_else.RemoveAt (index_a);
			index_b = UnityEngine.Random.Range (0, everything_else.Count);
			spot_b = (int) everything_else [index_b];
			while (spot_b == spot_a) {
				index_b = UnityEngine.Random.Range (0, everything_else.Count);
				spot_b = (int) everything_else [index_b];
			}
			everything_else.RemoveAt (index_b);
			initPortalPair (spot_a, spot_b);
		}
	}

	void initPortalPair(int locA, int locB) {
		portalPairs += 1;
		Transform pair = Instantiate (portalPair, new Vector3(0,0,0), Quaternion.identity);
		if (picked == HSV.Hue) {
			int index = UnityEngine.Random.Range (0, free_choices.Count);
			hue = (float) free_choices [index];
			free_choices.Remove (index);
		} else if (picked == HSV.Sat) {
			int index = UnityEngine.Random.Range (0, free_choices.Count);
			sat = (float) free_choices [index];
			free_choices.Remove (index);
		} else if (picked == HSV.Val) {
			int index = UnityEngine.Random.Range (0, free_choices.Count);
			val = (float) free_choices [index];
			free_choices.Remove (index);		
		}
		foreach (Renderer r in pair.gameObject.GetComponentsInChildren<Renderer>()) {
			r.material.color = Color.HSVToRGB (hue, sat, val);
		}

		Vector3 posA = generatePosition (locA);
		Vector3 posB = generatePosition (locB); 

		bool redoA = false;

		foreach (Vector3 p in positions) {
			if (Vector3.Distance(p, posA) < 1) {
				redoA = true;	
			}
		}
		while (redoA) {
			posA = generatePosition (locA); 	
			redoA = false;
			foreach (Vector3 p in positions) {
				if (Vector3.Distance(p, posA) < 1) {
					redoA = true;	
				}
			}	
			Debug.Log ("2");
		}

		positions.Add (posA);

		bool redoB = false;

		foreach (Vector3 p in positions) {
			if (Vector3.Distance(p, posB) < 1) {
				redoB = true;	
			}
		}
		while (redoB) {
			posB = generatePosition (locB); 	
			redoB = false;
			foreach (Vector3 p in positions) {
				if (Vector3.Distance(p, posB) < 1) {
					redoB = true;	
				}
			}	
			Debug.Log ("3");
		}
		positions.Add (posB);

		Transform[] portals = pair.gameObject.GetComponentsInChildren<Transform> ();
		portals [0].SetPositionAndRotation (posA, Quaternion.identity);
		portals [1].SetPositionAndRotation (posB, Quaternion.identity);
	}

	void initPlayer(int grid_index) {
		Vector3 pos = generatePosition (grid_index);
		positions.Add (pos);
		pos.z = -.5f;

		Transform generatedPlayer = Instantiate (player, pos, Quaternion.identity);
	}

	void initExit(int grid_index) {
		Vector3 pos = generatePosition (grid_index);
		positions.Add (pos);

		Transform generatedExit = Instantiate (exit, pos, Quaternion.identity);
	}

	Vector3 generatePosition(int grid_index) {
		ArrayList grid_coords = (ArrayList) grid_spots [grid_index];
		Vector2 topCorner = (Vector2) grid_coords [0];
		Vector2 bottomCorner = (Vector2) grid_coords [1];

		float xpos = UnityEngine.Random.Range ( bottomCorner [0] + .5f, topCorner [0] - .5f);
		float ypos = UnityEngine.Random.Range (bottomCorner [1] + .5f, topCorner [1] - .5f);
		return new Vector3 (xpos, ypos, 0f);
	}
}
