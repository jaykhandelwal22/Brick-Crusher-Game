using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
// Class : DestructableItem
// Desc	 : This is the base class from which all wall items are constructed or derived.
//		   The base class is used for all normal bricks.
// ----------------------------------------------------------------------------------------
public class DestructableItem : MonoBehaviour 
{	
	// Inspector Assigned
	public		int				Points				=   0;		// Points awarded for destroying this brick
	public  	int				Weight				=	100;	// Weight of brick in brick selection
	public    	GameObject		NextObject			=	null; 	// Object to instantiate in its place when brick is destroyed
	public      AudioClip		DestroySound		=	null;	// Sound to play when destroyed
	public      float			DestroySoundVolume	=	1.0f;	// Volume of the destroy sound
	// Reference Cache
	protected 	GameManager		MyGameManager	=	null;	// Game Manager
	protected 	Transform		MyTransform 	=	null;	// Transform
	protected 	GameObject		MyGameObject	=	null;	// Game Object
	
	// Internals
	protected   bool			Dying			=	false;	// Is this brick in the process of being destroyed
	
	
	// ------------------------------------------------------------------------------------
	// Name	:	Start
	// Desc	:	Called once at startup to cache component references, register the 
	//			Invicible timer and get search the scene for the audio source to play
	//			when destroyed.
	// ------------------------------------------------------------------------------------
	protected virtual void Start () 
	{
		// Cache references for efficiency
		MyGameManager 	=	GameManager.Instance;
		MyTransform		=	transform;
		MyGameObject	=	gameObject;
		
		// All objects derived from this class will be sensitive to the "Invicible" timer.
		// We register this timer with the game manager. If it is set to a positive non-zero
		// number this brick knows it should allow the ball to pass through it because the
		// ball must be in invincible mode.
		if(MyGameManager!=null)
			MyGameManager.RegisterTimer("Invincible");
	
	}

	// ------------------------------------------------------------------------------------
	// Name	:	Fixed Update
	// Desc	:	Called each tick of the physics system to update the position of the brick.
	// -------------------------------------------------------------------------------------
	protected virtual void FixedUpdate()
	{
		// If the Wall stop timer is not active and the game is currently in the Playing state
		// update the position by the current wall speed of the game manager.
		if (MyGameManager!=null && 
			MyTransform!=null && 
			MyGameManager.CurrentState == GameState.Playing && 
			MyGameManager.GetTimer("Wall Stop Timer")==0.0f)
				transform.position-= Vector3.up	* Time.deltaTime * MyGameManager.WallSpeed;
		
		// If the invincible timer is set then set the collider as a trigger otherwise
		// make it a normal collider so the ball bounces off it.
		if (MyGameManager.GetTimer("Invincible") > 0.0f) 	GetComponent<Collider>().isTrigger = true;
		else 												GetComponent<Collider>().isTrigger = false;
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	Update
	// Desc	:	Called every frame to check the Y position of the brick to see if it
	//			has passed the terminal line and if so ends the game. Also, if the
	//			Invincible timer is set it changes its collider to a trigger so the
	//			ball can pass right through it.
	// -------------------------------------------------------------------------------------
	protected virtual void Update()
	{
		// End game if brick has been allowed to get too low
		if (MyTransform.position.y<-0.546453f)
			MyGameManager.EndGame();	
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	OnCollisionEnter
	// Desc	:	Called when the ball enters the bricks collider
	// ------------------------------------------------------------------------------------
	protected virtual void OnCollisionEnter( Collision c )
	{
		
		// If the brick is really high and out of view ignore it
		if (MyTransform.position.y>13) return;
		
		// If we are already dying then return
		if (Dying) return;
		
		// If the next object reference is non null instantiate the new object
		// in the bricks location
		if (NextObject!=null)
		{
			Instantiate( NextObject, MyTransform.position, MyTransform.rotation );		
		}
					
		// And finally kill the brick game object
		KillBrick();
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	OnTriggerEnter
	// Desc	:	This is called when the ball is in invincible mode and enters the brick.
	// ------------------------------------------------------------------------------------
	protected virtual void OnTriggerEnter( Collider c )
	{
		// Don't do anything if brick is off the top of the screen and not yet showing
		if (MyTransform.position.y>13) return;
		
		// Don't do anything if this brick is already dying
		if (Dying) return;
		
		// Kill the brick
		KillBrick();
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	KillBrick
	// Desc	:	Destroys the brick
	// ------------------------------------------------------------------------------------
	public virtual void KillBrick()
	{
		// If we are already dying then return
		if (Dying) return;		
		
		// We are now dying
		Dying = true;
		
		// Play the brick destroy sound
		if (DestroySound) AudioSource.PlayClipAtPoint( DestroySound, MyTransform.position, DestroySoundVolume);
		
		// Award the points to the player
		if (MyGameManager!=null)
		{
			MyGameManager.AddPoints( Points );	
		}	
		
		// Destroy the game object
		DestroyObject( MyGameObject );
	}
}
