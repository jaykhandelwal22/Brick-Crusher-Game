using UnityEngine;
using System.Collections;

// --------------------------------------------------------------------------------------------
// Class : ForceApplicator
// Desc	 : Simply class to apply a one-off force to any game object in any direction at startup.
// --------------------------------------------------------------------------------------------
public class ForceApplicator : MonoBehaviour 
{
	public Vector3   ForceDirection	=	Vector3.zero;		// Local space direction vector of the force
	public ForceMode ForceType		=	ForceMode.Impulse;	// The type of force
		
	// ---------------------------------------------------------------------------------------
	// Name	:	Start
	// Desc	:	If the object has a rigid body apply the force at startup
	// ---------------------------------------------------------------------------------------
	void Start () 
	{
		if (GetComponent<Rigidbody>()) GetComponent<Rigidbody>().AddRelativeForce(ForceDirection, ForceType );
	}	
}
