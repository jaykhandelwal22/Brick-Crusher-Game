using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
// Class: TNT 
// Desc	: The script that is added to the TNT brick
// ----------------------------------------------------------------------------------------
public class TNT : DestructableItem 
{
	// Inspector Assigned
	public 	GameObject 	ExplosionPrefab = null;	// The Explosion game object prefab
	
	// Explosion Instance
	private GameObject  Explosion		= null; // The instance of the explosion object in the scene
	
	// ----------------------------------------------------------------------------------------
	// Name	: KillBrick
	// Desc	: Instantiates the explosion
	// ----------------------------------------------------------------------------------------
	public override void KillBrick()
	{
		// If already dying then return
		if (Dying) return;
		
		// Call base call version to kill the brick, play the sound and in this case
		// instantiate the TNT trigger object
		base.KillBrick();
		
		// Instantiate the explosion particle system
		if (ExplosionPrefab!=null && Explosion==null) 
			Explosion = Instantiate(ExplosionPrefab, MyTransform.position, Quaternion.identity) as GameObject;
	}
}
