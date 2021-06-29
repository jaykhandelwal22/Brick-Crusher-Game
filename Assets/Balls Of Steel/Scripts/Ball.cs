using UnityEngine;
using System.Collections;

// --------------------------------------------------------------------------------------------
// Class : 	Ball
// Desc	 : 	Represents the behaviour of a ball in the game. Each ball that is spawned will have
//		   	an instance of this script attached to it.
// Note	 : 	This class is sensitive to the following timers being set in the Game Manager.
//			Any object/script can set these timers and the ball will react appropriately.
//
//		   	1) Invicible Timer  - Changes its material and enables its glow particle system.
//			2) Big Ball Timer	- Changes its scale to the large size.
//			3) Speed Ball Timer - Sets its ball speed to the fast speed.
// --------------------------------------------------------------------------------------------
public class Ball : MonoBehaviour 
{
	public  float		BallSpeed				=	10.0f;	// The speed of the ball
	public  float		BallSpeedMultiplier		=	2.0f;	// How the speed should be scaled when in fast ball mode.
	public  GameObject  GlowballEffect			=	null; 	// Game Object (particle system) that is enabled when the ball
														  	// is in glow ball mode to create an effect.
	public  float		BigSizeMultiplier		=   3.0f; 	// A multiplier describing the increase applied
														  	// to the scale of the ball when in big ball mode.
	public  Material	NormalBallMaterial		=	null; 	// Material to use when ball is in normal mdoe
	public  Material	GlowBallMaterial		=	null; 	// Material to use when ball is in glow mode
	
	private int 		counter			=	0;			  	// Simple frame counter so we don't start any physics
														  	// unity everything has had an update
	private	GameManager	MyGameManager	=	null;		  	// Game Manager Reference	
	private Rigidbody   MyRigidbody		=	null;		  	// Cached Rigidbody attached to this ball object
	private GameObject	MyGameObject	=	null;		  	// Cached Game Object
	private Transform	MyTransform		=	null;		  	// Cached Transform
	
	private Vector3     OriginalScale   =   Vector3.zero; 	// Original scale of the ball at scene startup
	private Vector3		BigScale		=	Vector3.zero; 	// Target scale once multiplier is applied
	
	private float		OriginalSpeed	=	0.0f;		  	// The original ball speed
	private float		FastSpeed		=	0.0f;		  	// The fast ball speed
	
	// ----------------------------------------------------------------------------------------
	// Name	:	Awake
	// Desc	:	Called when the object si first created. It cached frquently used components
	//		    and pre-calculates the scales to use for normal and big size modes. The ball
	//			then registers itself with the Game Manager. 
	// ----------------------------------------------------------------------------------------
	void Awake()
	{
		// Cache frequently accessed components
		MyRigidbody 	= GetComponent<Rigidbody>();
		MyGameObject	= gameObject;
		MyTransform		= transform;
		MyGameManager 	= GameManager.Instance;
		
		// Record the original scale of the parent object
		// so we can reset it back once glow mode is over
		OriginalScale	= MyTransform.localScale;
		BigScale   		= MyTransform.localScale * BigSizeMultiplier;
		
		// Calculate the normal and fast ball speed
		OriginalSpeed 	= BallSpeed;
		FastSpeed		= OriginalSpeed * BallSpeedMultiplier;
		
		// Register the fact that this is a live -- in play --
		// ball with the game manager so the game manager can
		// keep track of how many balls are left
		if (MyGameManager!=null)
		{
			MyGameManager.RegisterBall();
		}
		
		// Turn off the ball and all its children which contains
		// things like the partcle effects used for glow mode be default.
		Utils.SetActiveRecursively(MyGameObject, false);
		
		// Now just enable the top level object, the ball itself.
		MyGameObject.SetActive(true);
		
		// Check/Set the initial appearance and speed of the ball
		CheckBallState();
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	FixedUpdate
	// Desc	:	Called with each tick of the physics engine. It does a few tests to make sure
	//			the balls y velocity is not too low and also unregisters and kills the ball
	//			if it has dropped passed the paddle.
	// ----------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		// Increment the counter
		counter++;
		
		// If an entire physics update has already happened then start doing
		// out ball move tests.
		if (counter>1 && MyRigidbody!=null)
		{
			// Get velocity of ball
			Vector3 vel = MyRigidbody.velocity;
			
			// If y velocity is to small clamp to 3
			// as a minimum in its current direction of travel.
			if (Mathf.Abs(vel.y) <3.0f)
			{
				if (vel.y<0) vel.y=   -3.0f;
				else 		 vel.y =  3.0f;
			
				// Set the new velocity
				MyRigidbody.velocity = vel;
			}		
			
			// If our y position is lower than the -5 it means
			// it has gone out of play
			if (MyTransform.position.y<-5)
			{
				// Unregister the ball with the game manager
				if(MyGameManager!=null) MyGameManager.UnregisterBall();
				
				// Destroy this object and all of its children
				Destroy(gameObject);
			}
		}
		
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	CheckBallState
	// Desc	:	Transitions the ball into Big/Normal size mode, normal/glow mode and normal/fast
	//			mode.
	// ----------------------------------------------------------------------------------------
	private void CheckBallState( )
	{	
		// If the game manager and the glowball effect have been setup
		if (MyGameManager!=null)
		{
			// If any brick has registered a SpeedBall timer with the game manager
			// and it is current non zero then it means the ball should be going
			// fast
			float t = MyGameManager.GetTimer("Speed Ball");
			if (t>0.0f) BallSpeed = FastSpeed;
			else 		BallSpeed = OriginalSpeed;
			
			// Now check whether either the invincible time and/or the
			// big ball timers exist and are set
			float invincibleTimer 	= MyGameManager.GetTimer("Invincible");
			float bigBallTimer		= MyGameManager.GetTimer("Big Ball");
			
			// If the Invicible timer is set we will render our ball
			// with the fire ball material
			if (invincibleTimer>0.0f)
			{
				// If the glowball effect isn't active that means the timer has
				// just been set and the ball object need to be setup for big ball mode
				if (GlowballEffect!=null && GlowballEffect.activeSelf==false)
				{
					// Enabled the glow effect object
					Utils.SetActiveRecursively(GlowballEffect, true);
					
					// Set the material of the ball to the glow ball material
					if (GetComponent<Renderer>().material && GlowBallMaterial) 
						GetComponent<Renderer>().material = GlowBallMaterial;
				}
			}
			else
			{
				// turn off glow ball effect
				if (GlowballEffect!=null && GlowballEffect.activeSelf==true)
				{
					// Disable the glow effect object
					Utils.SetActiveRecursively(GlowballEffect, false);
					
					// Set the material of the ball to the glow ball material
					if (GetComponent<Renderer>().material && NormalBallMaterial) 
						GetComponent<Renderer>().material = NormalBallMaterial;
				}
			}
					
			// If the big ball timer is set we will increase its scale otherwise
			// we will reset it to normal
			if (bigBallTimer>0.0f)
			{
				// Set the scale of the ball to the big size
				if (MyTransform.localScale != BigScale)
				{
					MyTransform.localScale = BigScale;
				}
			}
			else
			{
				// Set the scale of the ball to its original scale
				if (MyTransform.localScale != OriginalScale)
				{
					MyTransform.localScale = OriginalScale;
				}
			}
		}
		
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	LateUpdate
	// Desc	:	Called every frame after all Update methods have run
	// ---------------------------------------------------------------------------------------
	void LateUpdate()
	{
		// Check the ball timers and perform the graphical switches if necessary
		CheckBallState();
		
		// Normalize velocity vector to ball speed
		if (MyGameManager)
			MyRigidbody.velocity = MyRigidbody.velocity.normalized * BallSpeed;
	}
}
