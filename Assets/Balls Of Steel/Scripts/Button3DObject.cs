using UnityEngine;
using System.Collections;

// --------------------------------------------------------------------------------------------
// Class : Button3DObject
// Desc  : Used to provide a button behaviour to standard game objects
// --------------------------------------------------------------------------------------------
public class Button3DObject : MonoBehaviour 
{
	// Move towards camera when hovering over
	public float ZSelectOffset		=   -3.0f;
		
	// Cached Components
	private Collider 	MyCollider	=	null;	// Collider of this object (must have a collider for mouse pick tests)
	private Camera   	MainCamera	=	null;	// Main camera of the scene
	private GameObject	MyGameObject=	null;	// Game object this script is attached to
	private Transform	MyTransform	=	null;	// Transform component of the object this script is attached to
	public  AudioSource HoverSound	=   null;	// Sound to play when mouse is hovering over the button
	public  AudioSource SelectSound	= 	null;	// Sound to play when mouse selected the button
		
	// The scene manager of this scene
	private SceneManager_Base MySceneManager = null; 
	
	// Positional Vectors
	private Vector3 OriginalPosition=	Vector3.zero;	// The start position of the object
	private Vector3 OffsetPosition  =   Vector3.zero;	// The position of the object when fully offset (mouse over)
	private Vector3 CurrentPosition =	Vector3.zero;	// The current position of the object
	
	// Has the button been selected
	private bool	Selected		=	false;
	
	// ----------------------------------------------------------------------------------------
	// Name	:	Awake
	// Desc	:	Called when button object is first created to cache frequently used 
	//			components and calculate the starting and offset positions of the button.
	// ----------------------------------------------------------------------------------------
	void Awake()
	{
		// Get the Collider and the scene's Main Camera
		MyCollider 	= GetComponent<Collider>();
		MainCamera 	= Camera.main;
		MyGameObject= gameObject;
		MyTransform	= transform;
			
		// Set the position to start and the position to move to
		OriginalPosition 	= MyTransform.position;
		OffsetPosition		= OriginalPosition + new Vector3(0.0f, 0.0f, ZSelectOffset);
		
		// Current position will start at the original position
		CurrentPosition		= OriginalPosition;
		
		// Find the scene manager
		MySceneManager =	SceneManager_Base.Instance;
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	Update
	// Desc	:	Called each frame to detect whether the mouse is over the button. It does this 
	//			by casting a ray into the screen and testing it for intersection with the
	//			object's collider.
	// ----------------------------------------------------------------------------------------
	void Update () 
	{
		// If the scene manager is present and the user has already selected a buttons
		// then just return.
		if (MySceneManager && MySceneManager.ActionSelected ) return;
		
		// If this button has not already been clicked on by the user
		if (!Selected)
		{
			// Create a ray from the current mouse position into the screen
			Ray ray = MainCamera.ScreenPointToRay (Input.mousePosition);
	        RaycastHit hit;
			
			// Does the ray intersect our collider
	    	if (MyCollider && MyCollider.Raycast (ray, out hit, 1000.0f)) 
			{
				// Set the current position to the offset position
	        	CurrentPosition = OffsetPosition;
				
				// Make button go white
				GetComponent<Renderer>().material.color = Color.white;
				
				// If there is a scene manager present and the mouse button has been
				// pressed then notify the scene manager of the action
				if (MySceneManager)
				{
					// Was left mouse button pressed while howevering over us
					if (Input.GetMouseButtonDown(0))
					{
						// If we have been assigned a selection sound then play it
						if (SelectSound && !SelectSound.isPlaying) SelectSound.Play();
						
						// Set the color of its material to red
						GetComponent<Renderer>().material.color = Color.red;
						
						// Call the scene manager's OnButtonSelect method so it can do
						// what it wants with this action.
						MySceneManager.OnButtonSelect(MyGameObject.name);
						
						// We have been selected so we don't wish to further process this button
						Selected = true;
					}
					// If the mouse button isn't down then we are hovering over it
					else
					{
						// Play the hover sound if present
						if (HoverSound && !HoverSound.isPlaying) HoverSound.Play();
						
						// Inform the scene manager we are hovering
						MySceneManager.OnButtonHover(MyGameObject.name);
					}
				}
			}
			// The mouse is not hovering over this object so set its material back to yellow
			// and its position to the normal position
			else
			{
				GetComponent<Renderer>().material.color = Color.yellow;
				CurrentPosition = OriginalPosition;
			}
		}
				
		// Always perform a lerp from the current transform's position to our target position (target position)
		// which means when we adjust CurrentPosition, the test will smoothly move there,
		MyTransform.position = Vector3.Lerp( MyTransform.position, CurrentPosition, Time.deltaTime );
	}
}
