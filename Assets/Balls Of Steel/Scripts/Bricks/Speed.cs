using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
// Class: Speed 
// Desc	: This is the script that is attached to the speed brick that makes the ball going
//		  faster for a period of time.
// ----------------------------------------------------------------------------------------
public class Speed : DestructableItem 
{
	// Inspector Assigned
	public float	SpeedTime = 15;	// Duration of ball speed increase
	
	// ----------------------------------------------------------------------------------------
	// Name	: Start
	// Desc	: Makes sure that the Speed Ball timer is registered with the game manager. The 
	//		  Ball object is always listening to this timer.
	// ----------------------------------------------------------------------------------------
	protected override void Start()
	{
		base.Start();
		MyGameManager.RegisterTimer("Speed Ball");
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	: KillBrick
	// Desc	: Adds time to the speed ball timer on death.
	// ----------------------------------------------------------------------------------------
	public override void KillBrick()
	{
		if (Dying) return;
		if (MyGameManager)
		{
			MyGameManager.UpdateTimer("Speed Ball", SpeedTime);
		}
		base.KillBrick();
	}
}
