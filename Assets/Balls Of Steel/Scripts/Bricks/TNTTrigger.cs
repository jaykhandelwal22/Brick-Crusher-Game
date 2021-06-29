using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
// Class : TNTTrigger
// Desc	 : This script is attached to a game object with a collider attached that is
//		   configured as a trigger. The object is instantiated when the TNT brick is
//		   destroyed which will cause all bricks within the range of thr trigger
//		   to destroy themselves also.
// ----------------------------------------------------------------------------------------
public class TNTTrigger : MonoBehaviour 
{
	// Inspector Assigned (the time to live before self destruction)
	public  float DestructionTime = 0.1f;
	
	// Used for recording duration of life
	private float Timer			  = 0.0f;
	
	// ------------------------------------------------------------------------------------
	// Name	:	Update
	// Desc	:	Called once per frame to update the timer and destroy itself if the duration
	//			of its life has been exceeded
	// ------------------------------------------------------------------------------------
	void Update () 
	{
		// Increment timer with elapsed time
		Timer+=Time.deltaTime;
		
		// If timer is larger than our intended lifetime
		// then we destroy the game object
		if (Timer>DestructionTime)
			Destroy(gameObject);
	}
}
