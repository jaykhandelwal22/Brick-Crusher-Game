using UnityEngine;
using System.Collections;

// --------------------------------------------------------------------------------------------
// Class	:	Controller
// Desc		:	Should be attached to the paddle. It manages moving the paddle in response to 
//				mouse input.
// --------------------------------------------------------------------------------------------
public class Controller : MonoBehaviour 
{	
	public  Color		PaddleColor			= Color.black;	
	public  float		MouseSensitivity 	= 0.6f;
	private GameManager MyGameManager 		= null;
	private Transform	MyTransform			= null;
	
	
	// ----------------------------------------------------------------------------------------
	// Name	:	Awake
	// Desc	:	Called when the object is first created
	// ----------------------------------------------------------------------------------------
	void Awake ()
	{
		// Cache frequently used components
		MyGameManager 	= GameManager.Instance;		
		MyTransform		= transform;	
		
		
		// Set the material color of the bat
		GetComponent<Renderer>().material.color = PaddleColor;
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	Update
	// Desc	:	Called every frame. Gets the horizontal mouse movement and uses it to move
	//			the paddle. It also clamps the paddle's position to the extents of the game area.
	// ----------------------------------------------------------------------------------------
	void Update () 
	{
		// If we have a game manager and it is not paused
		if (MyGameManager!=null && MyGameManager.CurrentState!=GameState.Paused)
		{			
			// Get mouse movement and scale down to get
			float delta =Input.GetAxis("Mouse X")*MouseSensitivity;
			MyTransform.position+=new Vector3(delta,0,0);
				
			// If the left button is hit then fire out another ball
			if (Input.GetMouseButtonDown(0)) MyGameManager.ReleaseBall();
				
			// If we have moved too far left then set position of paddle to left extent
			if (MyTransform.position.x<-9.966174f)
			{
				MyTransform.position = new Vector3( -9.966174f, MyTransform.position.y, MyTransform.position.z);	
			}
			else
			// If paddle has moved too far right then set position to right extent
			if (MyTransform.position.x>9.966174f) 
			{
				MyTransform.position = new Vector3( 9.966174f, MyTransform.position.y, MyTransform.position.z);	
			}
		}
	}
	
	
}
