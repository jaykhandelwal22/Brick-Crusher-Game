using UnityEngine;
using System.Collections;

// ---------------------------------------------------------------------------------------
// Class : SceneManager_TitleScreen
// Desc	 : Manages the sequence of events on the title screen and reacts to button 
//		   button selections
// ---------------------------------------------------------------------------------------
public class SceneManager_TitleScreen : SceneManager_Base 
{
	// Inspector assigned
	public Transform  CamWaypoint	=	null;		// World position the camera should move too when PLAY is selected
	public GameObject BallObject	=	null;		// The Game Object containing the "BALLS" mesh
	public GameObject OfObject		=	null;		// The Game Object containing the "Of" mesh
	public GameObject SteelObject	=	null;		// The Game Object Containing the "STEEL" mesh
	public GameObject MenuObject	=	null;		// The parent menu object (containing the PLay and Quite text meshes as children)
	public GameObject WallCover		=	null;		// The Game Object of the wall hole cover that is disabled when the ball smashes through
	public float	  BOSInitialDelay = 1.11f;		// The initial delay in second between Balls of Steel text reveals start
	public float      BOSWordDelay	  = 0.75f;		// Delay between each word Balls - Of - Steel
	
	// Private
	private Vector3   OriginalCamPosition 	= Vector3.zero;	// Transform of the camera at scene startup
	private Vector3	  TargetCamPosition		= Vector3.zero;	// Transform the camera should move to
	private Transform CameraTransform		= null;			// Current Transform fo the camera
		
	// -----------------------------------------------------------------------------------
	// Name	:	Awake
	// Desc	:	Assures that the audio listener's volume is at its default and fetches 
	//			the main camera. If then stores a reference to the camera's transform
	//			and stores its original position at scene startup.
	//			It then stores the final transform the the camera should move to.
	// -----------------------------------------------------------------------------------
	void Awake()
	{
		// Make sure AudioListener has volume
		AudioListener.volume = 1.0f;
		
		// Get the main camera
		if (Camera.main)
		{
			// Store a reference to its transform so we can update it
			CameraTransform 	= Camera.main.transform; 
			
			// Store the original position vector of the camera at scene startup
			OriginalCamPosition = CameraTransform.position;
		}
		
		// Store the final position vector we would like the camera to move to
		if (CamWaypoint) TargetCamPosition = CamWaypoint.transform.position;
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	Start
	// Desc	:	Called once prior to the first update
	// ------------------------------------------------------------------------------------
	void Start()
	{
		// No screen fade at startup. We want the image to pop straight in.
		ScreenFade = 0.0f;
		
		// Enabel the showing of the mouse cursor so we can select menu items
		Cursor.visible = true;
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	OnTriggerEnter
	// Desc	:	This is called when the big ball first enters the trigger behind the hole
	//			on the wall. This means the ball is just about to smash through.
	// ------------------------------------------------------------------------------------
	void OnTriggerEnter( Collider c)
	{	
		// Play the first audio source attached to this object which should be the wall
		// smashign sound
		if (GetComponent<AudioSource>()) GetComponent<AudioSource>().Play();
		
		// Turn off the wall cover as we are about to see the bricks behind it shatter into
		// view and reveal the hole.
		if (WallCover) WallCover.SetActive ( false );
		
		// What is the initial delay we wish to wait before we begin to reveal the first word
		// of the balls of steel effect
		float accum = BOSInitialDelay;
		
		// Reveal the "Balls" text in that many seconds.
		Invoke( "RevealBalls", 	accum);
		
		// Add on the word delay and reveal the "Of" mesh in that many seconds.
		accum+=BOSWordDelay;
		Invoke( "RevealOf", 	accum);
		
		// Add on the word delay and reveal the "Steel" mesh in that many seconds.
		accum+=BOSWordDelay;
		Invoke( "RevealSteel", 	accum);
		
		// Wait a further word delay and reveal the Menu game object.
		accum+=BOSWordDelay;
		Invoke( "RevealMenu", 	accum);
	}
	
	// Methods that are invoked in OnTriggerEnter in a timed sequence. Each of these
	// methods simply enables the associated game object.
	// Note: The text meshes have the "Stab" Audio Sources attached to them with
	// "Play on Awake" enabled so when we enable the game object the sound auto-plays
	private void RevealBalls()	{ if (BallObject) 	BallObject.SetActive	(true); }
	private void RevealOf() 	{ if (OfObject)		OfObject.SetActive 		(true); }
	private void RevealSteel()	{ if (SteelObject)	SteelObject.SetActive 	(true); }
	private void RevealMenu()	{ if (MenuObject)	Utils.SetActiveRecursively(MenuObject, true);  }
	
	// ------------------------------------------------------------------------------------
	// Name	:	OnButtonSelect
	// Desc	:	Called by the Button3DObject's when they are selected with the mouse. The
	//			name of the button is passed.
	// ------------------------------------------------------------------------------------
	public override void OnButtonSelect( string buttonName )
	{
		// If this has been called and we are already processing an action return
		if (ActionSelected) return;
		
		// If the play buttton has been selected start the coroutine that animates the
		// camera through the hole in the wall and then load the next scene.
		if (buttonName=="Play")
		{
			ActionSelected = true;
			StartCoroutine(	LoadGameScene() );
		}
		// Otherwise, if the Quit button is pressed start the Coroutine that fades
		// the scene and then loads in the Closing Credits scene.
		else if (buttonName=="Quit")
		{
			ActionSelected = true;
			StartCoroutine( QuitGame() );
		}
	} 
	
	// ------------------------------------------------------------------------------------
	// Name	:	LoadGameScene
	// Desc	:	Fades out the menu, animates the camera through the hole in the wall, and
	//			then load in the main game.
	// ------------------------------------------------------------------------------------
	private IEnumerator LoadGameScene()
	{
		// Get all the renderers of the menu object
		Renderer[] renderers = MenuObject.GetComponentsInChildren<Renderer>();	
		
		// Perform a 1 second fade-out of the menu
		float timer = 1.0f;
		
		// While the 1 second has still not expired
		while (timer>0.0f)
		{
			// Update the timer
			timer-=Time.deltaTime;
			
			// Loop through each renderer in the menu object
			foreach( Renderer r in renderers )
			{
				if (r && r.material)
				{
					// Fetch the material of the renderer
					Color col = r.material.color;
					
					// set the alpha of the material to the timer value
					col.a = timer;
					r.material.color = col;	
				}
			}
			
			// Yield control
			yield return null;
		}
		
		// Now do a 1.5 second animation of the camera
		timer = 1.5f;
		
		// While the timer has not expired
		while (timer>0.0f)
		{
			// Decrement timer
			timer-=Time.deltaTime;
			
			// Set the screen fade
			if (timer>=0.0f) ScreenFade = 1.0f-(timer/1.5f);
			
			// Generate the camera position by lerping between the original and target camera position using the timer
			// as the interpolator.
			if (timer>=0.0f) CameraTransform.position = Vector3.Lerp( OriginalCamPosition, TargetCamPosition, 1-(timer/1.5f));
			
			// Fade out volume of listener based on timer countdown
			AudioListener.volume = timer/1.5f;
			
			// Yield control
			yield return null;
		}
		
		// We have now faded out the menu and moved the camera between the hole in the wall
		// so we are now in a black-out and can load the game scene.
		Application.LoadLevel("Game Scene");
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	QuitGame (IEnumerator)
	// Desc	:	Performs a 2.5 second fade-out of the scene and then loads the closing
	//			credits scene.
	// ------------------------------------------------------------------------------------
	private IEnumerator QuitGame()
	{
		// Set the initial timer to 2.5 seconds
		float timer = 2.5f;
		
		// While the timer has not reached zero
		while (timer>0.0f)
		{
			// Deduct elapsed time from timer
			timer-=Time.deltaTime;
			
			// Map current timer into 0.1 range and invert to
			// set the current screen fade strength.
			if (timer>=0.0f) ScreenFade = 1.0f-(timer/2.5f);
			
			// Map the timer into 0.0 to 1.0 range and use
			// it to set the volume of the game sound.
			AudioListener.volume = timer/2.5f;
			
			// Yield control
			yield return null;
		}	
		
		// Fade out complete so load in the closing credits.
		Application.LoadLevel("Credits Scene");
	}
}
