using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
// Class: BonusBalls 
// Desc	: The script that should be attached to the Bonus Balls brick. It simply overrides
//		  the KillBrick function of the bass class to add more balls to the players ball
//		  bank just before it is destroyed.
// ----------------------------------------------------------------------------------------
public class BonusBalls : DestructableItem {
	
	// ----------------------------------------------------------------------------------------
	// Name	: KillBrick
	// Desc	: Adds 3 balls to the player's ball bank as it is destroyed
	// ----------------------------------------------------------------------------------------
	public override void KillBrick()
	{
		// If already dying then return
		if (Dying) return;
		
		// Add 3 more balls to the game manager
		MyGameManager.AddBall(3);
		
		// Call the base class version to do the actual killing
		base.KillBrick();
	}
}
