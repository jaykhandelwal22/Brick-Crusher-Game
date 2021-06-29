using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ----------------------------------------------------------------------------------------
// Class:	Credit
// Desc	:	Contains a single credit in the credits sequence. Each credit is a two line
//			structure.
// Note	:	This class should be serializable so it can be set in the inspector.
// ----------------------------------------------------------------------------------------
[System.Serializable]
public class Credit
{
	public string Line1;
	public string Line2;
}

// ----------------------------------------------------------------------------------------
// Class :	SceneManager_Credits
// Desc	 :	Manages the credit data and the timed displaying of that data.
// ----------------------------------------------------------------------------------------
public class SceneManager_Credits : SceneManager_Base 
{
	// Two text meshes are reused and recycled to show each
	// two line credit entry.
	public TextMesh 	CreditMesh1	=	null;
	public TextMesh		CreditMesh2	=	null;
	
	// Am inspector assigned of Credit objects
	public List<Credit> CreditRoll 	= 	new List<Credit>();
	
	// ------------------------------------------------------------------------------------
	// Name	:	Awake
	// Desc	:	Make sure the audio listener volume is set to normal.
	// ------------------------------------------------------------------------------------
	void Awake()
	{
		AudioListener.volume = 1.0f;
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	Start
	// Desc	:	Called prior to the first update.
	// ------------------------------------------------------------------------------------
	void Start()
	{
		// Hide the cursor during the credits
		Cursor.visible = false;
		
		// Make sure the scene is fully faded in
		ScreenFade = 0.0f;
		
		// Start the coroutine that displays the credits
		StartCoroutine(DoCredits());
	}
	
	
	// ------------------------------------------------------------------------------------
	// Name	:	DoCredits
	// Desc	:	Shows the credit sequence
	// ------------------------------------------------------------------------------------
	private IEnumerator DoCredits()	
	{
		// Used to reference the materials of the two text meshes
		Material material1 = null, material2 = null;
		
		// Get the materials of the text meshes so we can fade them in
		// and out with each credit and set their colors.
		if (CreditMesh1 && CreditMesh1.GetComponent<Renderer>()) 
			material1 = CreditMesh1.GetComponent<Renderer>().material;
		
		if (CreditMesh2 && CreditMesh2.GetComponent<Renderer>())
			material2 = CreditMesh2.GetComponent<Renderer>().material;
		
		// Create a red color and a grey color for the first and
		// second lines of text for each created (initially with an
		// alpha of zero so they are ready to fade in).
		Color color1 = new Color(0.5f,0.0f,0.0f,0);
		Color color2= new Color(0.7f,0.7f,0.7f,0);
		
		// Set the colors in the materials
		material1.color = color1;
		material2.color = color2;
		
		// Wait for 3 seconds to give time for the smoke to rise
		// between we show the first credit.
		float timer=0;
		while (timer<3.0f) 
		{
			// Poll for any key press while we are waiting
			// and quite if anything is pressed.
			if (Input.anyKeyDown) Application.Quit();
			
			// Increment loop timer
			timer+=Time.deltaTime;
			
			// Yield control
			yield return null; 
		}
		
		// Now start showing the credits one at a time
		if (material1!=null && material2!=null)
		{
			// For each credit in the credit roll
			for(int i =0; i<CreditRoll.Count; i++)
			{
				// Get the next credit to display
				Credit credit = CreditRoll[i];
				
				// Set the text meshs' text for the next credit
				CreditMesh1.text = credit.Line1;
				CreditMesh2.text = credit.Line2;
				
				// Do a four second fade in of the credit
				timer = 0.0f;
				while (timer<4.0f)
				{
					// Remember to poll for keyboard input
					if (Input.anyKeyDown) Application.Quit();
					timer+=Time.deltaTime;
					
					// Adjust alpha of both colors based on timer
					color1.a = color2.a = timer/4.0f;
					
					// Set the material colors of the text meshes
					material1.color = color1;
					material2.color = color2;
					
					// done for this frame so yield execution
					yield return null;	
				}
				
				// Display the credit for 4 more seconds
				while (timer<4.0f) 
				{
					// Remember to poll for keyboard input
					if (Input.anyKeyDown) Application.Quit();
					timer+=Time.deltaTime; 
					yield return null; 
				}
				
				// Fade out the credit over the next 3 seconds
				timer = 0.0f;
				while (timer<3)
				{
					if (Input.anyKeyDown) Application.Quit();
					timer+=Time.deltaTime;
					color1.a = color2.a = 1.0f - (timer/3.0f);
					material1.color = color1;
					material2.color = color2;
					yield return null;	
				}
				
				// If this isn't the last credit in the roll then
				// show nothing for another 5 seconds
				if (i!=CreditRoll.Count-1) 
				{
					while (timer<5.0f) 
					{ 
						if (Input.anyKeyDown) Application.Quit();
						timer+=Time.deltaTime; 
						yield return null; 
					}
				}
			}
		}
		
		// All credits have been shown so now fade out the scene and the music
		// before exiting the app
		timer = 0.0f;
		
		// Do a four second final fade
		while (timer<=4.0f)
		{
			// Remember to poll for keyboard input
			if (Input.anyKeyDown) Application.Quit();
			timer+=Time.deltaTime;
			ScreenFade = timer/4.0f;
			
			// Fade out the audio based on timer
			AudioListener.volume = 1- (timer/4.0f);
			
			// Yield execution of the loop until the next frame
			yield return null;
		}
		
		// All done so quit the application
		Application.Quit();
	}
}
