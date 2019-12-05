using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Flag : MonoBehaviour {
	public GameObject FlagDown, FlagUp, KeyEnabled, Enemy;
	public Transform EnemySpawn;
	public AudioClip Win;

	public float Timer=0.0f;
	public Text TimerText;
	public string formattedTime;
	


	void Start () {
		Instantiate (Enemy,EnemySpawn.position, Quaternion.identity);
		FlagDown.SetActive (true);
		FlagUp.SetActive (false);	
	}

	void Update (){
		if (GameObject.FindWithTag ("Enemy") != null){
			Debug.Log ("Better Run");
		}
		else{
			Instantiate (Enemy,EnemySpawn.position, Quaternion.identity);
		}
	}

	void FixedUpdate(){
		Timer = Timer + Time.deltaTime;
		formattedTime = (int)(Timer / 60) + ":" + (int)(Timer % 60);
		TimerText.text = "Time - " +formattedTime;
		
	}

	void OnTriggerEnter2D (Collider2D other){
		if (other.gameObject.tag == "Player"){
			if (KeyEnabled.activeInHierarchy){
				FlagDown.SetActive (false);
				AudioSource.PlayClipAtPoint(Win, transform.position);
				FlagUp.SetActive (true);
				int HighTime = PlayerPrefs.GetInt ("Timer");
				if(Timer>HighTime){
					SetNewHighScore();
				}
				else {

				}
				SceneManager.LoadScene (2);
			}
		}
	}
	void SetNewHighScore(){
		PlayerPrefs.SetFloat ("Timer",Timer);
	}

	public void Restart (){
		SceneManager.LoadScene (0);
	}

}
