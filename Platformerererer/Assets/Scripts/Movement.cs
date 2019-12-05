using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum Swipe { None, Up, Down, Left, Right };
public class Movement : MonoBehaviour {

public float moveSpeed, jumpHeight, minSwipeLength =200f,test = 5.5f;
public GameObject Player, BloodSplatter,SkullDeath;
public Transform PlayerTrans, PortalExit;
private bool isGrounded;
Vector2 firstPressPos, secondPressPos, currentSwipe;
public static Swipe swipeDirection;

public AudioClip Jump,SpikeDeath,LavaDeath,KilledByEnemy;
AudioSource audio;

 

 void Start(){
	 audio = GetComponent<AudioSource>();
	 BloodSplatter.SetActive (false);
	 SkullDeath.SetActive (false);
	 test = Mathf.Round(5.5f);
 }
    void Update (){
#if UNITY_ANDROID
	DetectSwipe();
	
#else
	if (isGrounded == true && Input.GetKeyDown (KeyCode.W)) {
			//if (isGrounded == true && Input.GetMouseButtonDown (0)){
			audio.PlayOneShot(Jump);
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (GetComponent<Rigidbody2D>().velocity.x, jumpHeight);
			isGrounded = false;
		}
		if (Input.GetKey (KeyCode.D)) {
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (moveSpeed, GetComponent<Rigidbody2D>().velocity.y);
		}
		if (Input.GetKey (KeyCode.A)) {
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (-moveSpeed, GetComponent<Rigidbody2D>().velocity.y);
		}
		if(Input.GetKeyDown(KeyCode.S)) {
   			GetComponent<Rigidbody2D>().velocity = Vector3.zero;
   			GetComponent<Rigidbody2D>().angularVelocity = 0f;
   		 }
     
#endif


}
	void OnTriggerExit2D (Collider2D other){
		GameObject Enemy = GameObject.FindWithTag ("Enemy");
		Transform EnemyTrans = Enemy.transform;
		if (other.gameObject.tag == "Enemy" && EnemyTrans.position.y > PlayerTrans.position.y){
			Renderer Rend;
			Rend = GetComponent<Renderer>();
			Rend.enabled = false;
			audio.PlayOneShot (KilledByEnemy);
			SkullDeath.SetActive (true);
			gameObject.GetComponent<SpriteTrail>().enabled = (false);
			StartCoroutine (GameOver ());
		}
	}
	void OnTriggerEnter2D (Collider2D other){
		Renderer Rend;
		Rend = GetComponent<Renderer>();
		if (other.gameObject.tag =="Lava"){
			audio.PlayOneShot (LavaDeath);
			Rend.enabled = false;
			gameObject.GetComponent<SpriteTrail>().enabled = (false);
			StartCoroutine (GameOver ());
		}
		else if (other.gameObject.tag =="Portal"){
			transform.position = PortalExit.position;
		}
		else if (other.gameObject.tag =="Spikes"){
			audio.PlayOneShot(SpikeDeath);
			Rend.enabled = false;
			BloodSplatter.SetActive (true);
			gameObject.GetComponent<SpriteTrail>().enabled = (false);
			StartCoroutine (GameOver());
		}
	}
   
    void OnCollisionStay2D(Collision2D coll){
		isGrounded = true;
		Debug.Log ("Grounded");
	}
    
	public void DetectSwipe ()
    {
        if (Input.touches.Length > 0) {
             Touch t = Input.GetTouch(0);
 
        	if (t.phase == TouchPhase.Began) {
                 firstPressPos = new Vector2(t.position.x, t.position.y);
             }
 
            if (t.phase == TouchPhase.Ended) {
                secondPressPos = new Vector2(t.position.x, t.position.y);
                currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);
           
			// make sure its not a tap
           if (currentSwipe.magnitude < minSwipeLength) {
            	swipeDirection = Swipe.None;
                return;
             }
           
                currentSwipe.Normalize();
 
                // Swipe up
                if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f && isGrounded == true) {
					swipeDirection = Swipe.Up;
					GetComponent<Rigidbody2D> ().velocity = new Vector2 (GetComponent<Rigidbody2D>().velocity.x, jumpHeight);
					isGrounded = false;
                // Swipe down
                } else if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f) {
                    swipeDirection = Swipe.Down;
					GetComponent<Rigidbody2D>().velocity = Vector3.zero;
   					GetComponent<Rigidbody2D>().angularVelocity = 0f;
                // Swipe left
                } else if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f) {
                    swipeDirection = Swipe.Left;
					GetComponent<Rigidbody2D> ().velocity = new Vector2 (-moveSpeed, GetComponent<Rigidbody2D>().velocity.y);
                // Swipe right
                } else if (currentSwipe.x > 0 &&  currentSwipe.y > -0.5f && currentSwipe.y < 0.5f) {
                    swipeDirection = Swipe.Right;
					GetComponent<Rigidbody2D> ().velocity = new Vector2 (moveSpeed, GetComponent<Rigidbody2D>().velocity.y);
                }
             }
        }
		else {
        	swipeDirection = Swipe.None;
        }
    }
	 IEnumerator GameOver (){
     Debug.Log ("StartedCoRoutine");
   yield return new WaitForSeconds (3f);
   Debug.Log ("waited");
    SceneManager.LoadScene (3);
 }

}

