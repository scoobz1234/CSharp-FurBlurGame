using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class EndScreen : MonoBehaviour {

	public Text TimerText;
	float Timer;
	string formattedTime;

	// Use this for initialization
	void Start () {
	Timer = PlayerPrefs.GetFloat("Timer");
	formattedTime = (int)(Timer / 60) + ":" + (int)(Timer % 60);
	TimerText.text = "Your Time - " + formattedTime;	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void EndButton(){
		Application.Quit();
	}
	public void PlayAgain(){
		SceneManager.LoadScene (0);
	}
}
