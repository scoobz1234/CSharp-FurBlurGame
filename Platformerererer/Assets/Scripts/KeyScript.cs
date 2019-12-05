using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyScript : MonoBehaviour {
	
	public GameObject Key, KeyEnabled, KeyDisabled;

	void Start(){
		KeyEnabled.SetActive (false);
		KeyDisabled.SetActive (true);
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.gameObject.tag == "Player"){
			Destroy (Key);
			KeyEnabled.SetActive (true);
			KeyDisabled.SetActive (false);
		}
	}
}
