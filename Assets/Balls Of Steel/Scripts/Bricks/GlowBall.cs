using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
// Class: GlowBall 
// Desc	: This is the script that is attached to the GlowBall brick that makes the balls
//		  big and invicible with respect to bricks.
// ----------------------------------------------------------------------------------------
public class GlowBall : DestructableItem 
{
	// Inspector Assigned
	public float	GlowTime = 15;	// The effect should last for 15 seconds
	
	// ----------------------------------------------------------------------------------------
	// Name	: Start
	// Desc	: Calls the base class version and then registers an additional timer (Big Ball)
	//		   with the game manager if it does not already exist. We don't have to register 
	//		   the Invincible timer because all DestructableItems support this and thus this
	//		   timer is registered in the base class Start method.
	// ----------------------------------------------------------------------------------------
	protected override void Start()
	{
		// Do base class startup processing
		base.Start();
		
		// Register the Big Ball timer (if not registered already)
		MyGameManager.RegisterTimer("Big Ball");
	}
	
	
	// ----------------------------------------------------------------------------------------
	// Name	: KillBrick
	// Desc	: Called when this brick's collider/trigger is hit by a ball
	// ----------------------------------------------------------------------------------------
	public override void KillBrick()
	{
		// If already dying just return
		if (Dying) return;
	
		// Add the glow time to both the Invincible and Big Ball timers
		if (MyGameManager)
		{
			MyGameManager.UpdateTimer("Invincible", GlowTime);
			MyGameManager.UpdateTimer("Big Ball", GlowTime);
		}
		
		// Call base class to do the killing
		base.KillBrick();
	}
}
