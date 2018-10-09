using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class PlayerController : MonoBehaviour {



	Rigidbody2D rb;
	GameObject portal;
	public TextMesh winText;
	public FieldController fc;
	bool finish;
	bool gameOver;
	int num_portals;
	// Use this for initialization
	private string writePath;


	void AppendFile(string filePath, string myString){
		StreamWriter sAppender; 

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

	void Start() {
		writePath = Application.dataPath + "/Log/gameStats.txt";
	}

	void Awake() {
		rb = GetComponent<Rigidbody2D> ();
		finish = false;
		winText.text = "";
		gameOver = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!gameOver) {
			if (portal != null && Input.GetKeyDown("space")) {
				Vector3 portalPosition = portal.GetComponent<PortalController> ().getOtherPortalLoc();
				portal = portal.GetComponent<PortalController> ().getOtherPortal ();
				rb.MovePosition (portalPosition);		
				num_portals += 1;
				String str = DateTime.Now.ToString ("MM/dd/yyyy HH:mm:ss") + ", Player took portal";
				AppendFile (writePath, str);
			} else if (finish && Input.GetKeyDown("space")) {
				String str = DateTime.Now.ToString ("MM/dd/yyyy HH:mm:ss") + ", Player exited, " + num_portals;
				AppendFile (writePath, str);
				gameOver = true;
				SceneManager.LoadScene ("EndState");
			}else {
				var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
				var y = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
				Vector2 movement = new Vector2 (x, y);

				rb.MovePosition (rb.position + movement);		
			}		
		}
	}



	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.tag == "PortalA" || other.gameObject.tag == "PortalB") {
			portal = other.gameObject;
		} else if (other.gameObject.tag == "Finish") {
			finish = true;
			Debug.Log ("Enter finish");
		}
		Debug.Log ("Enter portal");
	}

	void OnTriggerExit2D (Collider2D other) {
		if (other.gameObject.tag == "PortalA" || other.gameObject.tag == "PortalB") {
			portal = null;
		} else if (other.gameObject.tag == "Finish") {
			finish = false;
		}
		Debug.Log ("Exit portal");
	}
}
