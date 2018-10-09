using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TextWriter : MonoBehaviour {

	public float letterPause = 0.2f;

	string message;
	Text text;

	private bool display = true;

	// Use this for initialization
	void Start () {
		text = GetComponent<Text>();
		message = text.text;
		text.text = "";
		StartCoroutine(TypeText ());
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Return)) {
			display = false;
			if (SceneManager.GetActiveScene ().name.Equals ("StartState")) {
				SceneManager.LoadScene ("InfoState");
			}
			else if (SceneManager.GetActiveScene ().name.Equals ("InfoState")) {
				SceneManager.LoadScene ("PlayState");
			}
			else if (SceneManager.GetActiveScene ().name.Equals ("EndState")) {
				SceneManager.LoadScene ("PlayState");
			}
		}
		else if (Input.GetKeyDown (KeyCode.Escape)) {
			if (SceneManager.GetActiveScene().name.Equals("EndState")) {
				SceneManager.LoadScene ("StartState");
			}
		}
	}

	IEnumerator TypeText () {
		while (display) {
			foreach (char letter in message.ToCharArray()) {
				text.text += letter;
				yield return 0;
				yield return new WaitForSeconds (letterPause);
			}		
			for (int i = 0; i < 10; i++) {
				text.text = text.text.Substring (0, text.text.Length - 1) + " ";
				yield return 0;
				yield return new WaitForSeconds (letterPause);
				text.text = text.text.Substring (0, text.text.Length - 1) + '|';
				yield return 0;
				yield return new WaitForSeconds (letterPause * 3);
			}
			text.text = "";
		}

	}

}
