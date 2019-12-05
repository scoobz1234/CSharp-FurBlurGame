using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyScript : MonoBehaviour {
public GameObject Player,EnemyDeath;
public Transform player, Enemy;
public float walkingDistance = 500.0f, smoothTime =3.0f;
private Vector3 smoothVelocity = Vector3.zero;
public AudioClip EnemyDie;
AudioSource Audio;
 
void Start (){
    Audio = GetComponent<AudioSource>();
	Player = GameObject.FindWithTag ("Player");
	player = Player.transform;
}
 void Update() {
     //Calculate distance between player
     float distance = Vector3.Distance(transform.position, player.position);
     //If the distance is smaller than the walkingDistance
     if(distance < walkingDistance){
     //Move the enemy towards the player with smoothdamp
         transform.position = Vector3.SmoothDamp(transform.position, player.position, ref smoothVelocity, smoothTime);
     }
 }
 void OnTriggerExit2D (Collider2D other){
	 if (other.gameObject.tag == "Player" && player.position.y > Enemy.position.y){
         Instantiate(EnemyDeath, Enemy.position, Enemy.rotation);
         AudioSource.PlayClipAtPoint(EnemyDie, transform.position);
		 Destroy (gameObject);
	 }
 }
}
