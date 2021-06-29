using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
// Class :	WallStopper
// Desc	 :	This is the behaviour that we attach to the COGs brick that stops the wall
//			from moving down.
// Note	 :	We don't have to register the "Wall Stop Timer" because this is a timer that
//			is internally supported by the game manager itself so will always exist.
// ----------------------------------------------------------------------------------------
public class WallStopper : DestructableItem 
{
	// Inspector Assigned
	public float	StopTime = 15;	// How long should this brick stop the wall for
	
	// ------------------------------------------------------------------------------------
	// Name	:	KillBrick
	// Desc	:	This is called when the brick is destroyed.
	// ------------------------------------------------------------------------------------
	public override void KillBrick()
	{
		// If already dying return
		if (Dying) return;
		
		// Set the Wall Stop Timer in the Game Manager
		if (MyGameManager)
		{
			MyGameManager.UpdateTimer("Wall Stop Timer", StopTime);
		}
		
		// Call base class version so we also get all the normal
		// processing
		base.KillBrick();
	}
}
