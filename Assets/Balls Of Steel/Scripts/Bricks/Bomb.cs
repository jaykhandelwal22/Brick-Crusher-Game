using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
// Class: Bomb
// Desc	: This is the script that is attached to the Bomb brick. 
// ----------------------------------------------------------------------------------------
public class Bomb : DestructableItem 
{
	// Inspector Assigned
	public 	GameObject 	ExplosionPrefab = null;	// Game Object reference to the explosion object that should be instantiated when hit.
	public 	float	  	DamageRadius	= 4.0f;	// The radius of a sphere in which all bricks will be destroyed
	
	// Internals
	private int			BrickLayer		= 0;	// Used to cache the index of the Brick layer for use in physics tests
	private GameObject  Explosion		= null; // A reference to the explosion instance that we create in the scene
	
	// ----------------------------------------------------------------------------------------
	// Name	: Start
	// Desc	: Calls the base class implementation and then caches the index of the Brick layer
	// ----------------------------------------------------------------------------------------
	protected override void Start()
	{	
		// Call base class version (Important)
		base.Start();
		
		// Get the index of the layer called "Brick"
		BrickLayer = LayerMask.NameToLayer("Brick");	
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	: KillBrick
	// Desc	: Called when the ball collides with the bomb
	// ----------------------------------------------------------------------------------------
	public override void KillBrick()
	{
		// If we are already dying then don't try and do all this again
		// if another ball collides with it before its gone
		if (Dying) return;
		
		// Call base class version which will kill the brick and play the sound etc
		base.KillBrick();
		
		// If we have a reference to an explosion prefab and we have not already created the explosion (paranioa check)
		// then instantiate the explosion
		if (ExplosionPrefab!=null && Explosion==null) 
			Explosion = Instantiate(ExplosionPrefab, MyTransform.position, Quaternion.identity) as GameObject;
			
		// Before brick is destroyed we do a sphere test and kill any bricks
		// within its radius. We only test for objects assigned to the brick layer. This will return
		// an array of brick colliders.
		Collider [] colliders = Physics.OverlapSphere( MyTransform.position, DamageRadius, 1<<BrickLayer);
		
		// Loop through all collides returned
		for (int i=0; i<colliders.Length; i++)
		{
			// Try and fetch a DestructableItem (or derived) script
			DestructableItem script = colliders[i].GetComponent<DestructableItem>();
			
			// If the script was found
			if (script!=null)
			{
				// Call its kill brick function
				script.KillBrick();	
			}	
		
		}
	}
}
