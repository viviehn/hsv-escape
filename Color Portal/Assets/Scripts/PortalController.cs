using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour {

	// Use this for initialization
	GameObject otherPortal;

	void Awake() {
		GameObject parent = this.transform.parent.gameObject;
		if (gameObject.tag == "PortalA") {
			otherPortal = parent.transform.FindChild ("PortalB").gameObject;
		} else {
			otherPortal = parent.transform.FindChild ("PortalA").gameObject;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Vector3 getOtherPortalLoc () {
		return otherPortal.transform.position;
	}

	public GameObject getOtherPortal () {
		return otherPortal;
	}
}

