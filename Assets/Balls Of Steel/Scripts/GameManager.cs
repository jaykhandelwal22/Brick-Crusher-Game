using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The various states the game manager can be in 
public enum GameState { GetReady , Playing , GameOver, Paused }

// --------------------------------------------------------------------------------------------
// Class : GameManager
// Desc	 : Manages the main Game Manager
// --------------------------------------------------------------------------------------------
public class GameManager : MonoBehaviour 
{
	// GUI Skin to use for pause menu
	public GUISkin GameSkin									=	null;
	
	// Prefabs and Objects
	public List<GameObject> 				BrickPrefabs	=	new List<GameObject>();			// A list of Brick Objects
	public List<GameObject>					BonusPrefabs	=	new List<GameObject>();			// A list of Bonus Brick Objects
	//public List<float>						Timers			=	new List<float>();				// A list of timers
	public Dictionary<string, Timer>		Timers			=	new Dictionary<string, Timer>();	// A list of names used by the timers

	public GameObject			BallPrefab		=	null;					// Prefab to use for the ball
	public Transform			BallSpawnPoint  =   null;					// Ball Spawn Point 
	
	// Text Mesh Objects to use for the Scores and Ball Counts
	public TextMesh				CurrentScoreText=	null;	
	public TextMesh				HighScoreText	=	null;
	public TextMesh				BallsText		=	null;
	public TextMesh				GetReadyText	=	null;
	public TextMesh				GameOverText	=	null;
	 
	// Game Mechanics
	public float      			WallSpeed			=	0.0f;			// Initial Speed of Wall
	public float				MaxWallSpeed        =   0.3f;			// Maximum speed the wall should ever scroll
	public float				BonusChance			=	10.0f;			// % Change a Bonus Brick will be generated
	
	// Game Manager Sound Effects
	public AudioSource			LoweringSound		=	null;			// Audio SOurce to use for the wall lowering sound
	private float				LoweringSoundVolume	=	0.0f; 			// Current volume of the sound
	
	// Internals
	private int					BallsInPlay		=   0;				// Number of balls currently in play								
	private int					BallCount		=	6;				// Number of balls pending play
	private int					CurrentScore	=   0;				// Current Score of the player
	private int					HiScore			=	0;				// Current High Score
	
	// Brick Generation
	private float[]				BrickWeightTable=	null;			// Brick Spawn Probability Table
	private float[]				BonusWeightTable=	null;			// Bonus Spawn Probability Table
	private int [] 				HalfRow			=	new int[5];		// Temp to generate a hald row of bricks
	
	// Wall Movement Interpolator
	private float 				RowInterpolator =   0.0f;			// When this gets to one we need to generate
																	// another row of bricks
	
	// Current State  (Start in GetReady State)
	public GameState 			CurrentState	=	GameState.GetReady;
	
	// Scene Manager
	private SceneManager_Base	MySceneManager	=	null;
	
	// GUI regions
	private Rect	PauseBox_ScreenRect				=	new Rect( 700, 450, 520, 320);
	private Rect    Continue_ScreenRect				=	new Rect( 800, 550, 320, 60 );
	private Rect    Quit_ScreenRect					=	new Rect( 800, 650, 320, 60);
		 
	// Static Singleton Instance
	private static GameManager _Instance		= null;
	
	// Property to get instance
	public static GameManager Instance
	{
		get { 
				// If we don't an instance yet find it in the scene hierarchy
				if (_Instance==null) { _Instance = (GameManager)FindObjectOfType(typeof(GameManager)); }
				
				// Return the instance
				return _Instance;
			}
	}
	
	
	// ----------------------------------------------------------------------------------------
	// Name	:	Awake
	// Desc	:	Builds the brick probability tables and creates the initial wall of bricks.
	//			It also puts the current text in to the text meshes
	// ----------------------------------------------------------------------------------------
	void Awake () 
	{
		// Cache reference to the scene manager
		MySceneManager = SceneManager_Base.Instance;
		
		int i=0,bi=0;
		
		// Build the probability tables
		BuildBrickWeightTable();
		BuildBonusWeightTable();
		
		// If we have some bricks defined
		if (BrickPrefabs.Count>0)
		{
			// We are going to create 9 rows from world space
			// Y position 14 down to 6
			for( int row=14; row>5; row--)
			{
				// We want rows to be symmetrical so generate
				// the first 5 bricks and then we will mirror
				// them about the center
				for (i=0; i<5;i++)	
				{
					// Fill array with 5 brick indices
					HalfRow[i] = GetNextBrickIndex();
				}
				
				// Uses for mirroring the above array
				int direction = 1;
				
				// the index in the half row we are current processing
				i = 0;
				
				// Now create 11 bricks on the current row
				for (int col =-10; col<12; col+=2)
				{
					// if we have reached the center brick of the row
					// just generate any random brick for the center and 
					// then flip the direction we are stepping through HalfRow
					if (i==5)
					{
						bi = GetNextBrickIndex();
						direction = -1;
					}
					else  
					{
						// Otherwise fetch the brick index from the HalfRow array
						bi = HalfRow[i];
						
					}
					
					// Roll the dice to see if we should generate a bonus brick
					float chanceOfBonus = Random.Range(0.0f, 100.0f);
					
					// If we should and bonus bricks exist instantiate one randomly
					if (chanceOfBonus<BonusChance && BonusPrefabs.Count>0)
					{
						// Create the bonus brick from our prefabs array
						Instantiate( BonusPrefabs[GetNextBonusIndex()], new Vector3( (float)col, (float)row, 0.0f ), Quaternion.identity);	
					}
					else
					{
						// Create the normal brick from our prefabs array
						Instantiate( BrickPrefabs[bi], new Vector3( (float)col, (float)row, 0.0f ), Quaternion.identity);
					}
					
					// Increase/Decrease I based on current direction that we are stepping through HalfRow
					i+=direction;	
				}			
			}
		}
		
		// Fetch the current Hi Score from the player prefs (or set to zero if it does not exist)
		HiScore 	= PlayerPrefs.GetInt("HiScore", 0);		
		
		// Set the text property of our Score, HiScore and Balls Text Meshes
		if (CurrentScoreText!=null) CurrentScoreText.text="Score : "+CurrentScore.ToString();
		if (HighScoreText!=null)	HighScoreText.text="HiScore : "+HiScore.ToString();
		if (BallsText!=null) 		BallsText.text="Balls : "+BallCount.ToString();
		
		// Initially disable the GameOver and GetReady text game objects
		if (GameOverText)			GameOverText.gameObject.SetActive (false);
		if (GetReadyText)			GetReadyText.gameObject.SetActive (false);	
		
		// The game manager has one timer that it creates itself and listens too to control its
		// wall slide speed. Any other script (bonus brick) can set this to a positive value to stop
		// the wall sliding for that number of seconds
		RegisterTimer("Wall Stop Timer");
		
		// Fetch and store the current volue assigned to the audio source of the wall lowering sound.
		// This will have been set by the developer to the proper value for the wall so we need to be
		// able to restore this value when we restart the wall sliding sound (after it has been stopped).
		if (LoweringSound) LoweringSoundVolume = LoweringSound.volume;
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	: Start
	// Desc	: Lock the cursor (which also turns it off) so that if we are running in a window
	//		  we keep the focus even if the mouse is clicked outside it. This keeps the mouse
	//		  invisibly centered in our application window while we have focus.
	// ----------------------------------------------------------------------------------------
	void Start()
	{
		// Make sure cursor is locked at start
		Screen.lockCursor = true;
		
		// Start a coroutine that will fade in the Get Ready text and will transition
		// the game state to Playing over a period of seconds. Only when this function
		// has completed will we be in the playing state and thus any updates (or fixed updates)
		// will pretty much do nothing until we are in the Playing state.
		StartCoroutine(StartGame());	
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	Fixed Update
	// Desc	:	Called each tick of the physics system
	// ----------------------------------------------------------------------------------------
	void FixedUpdate () 
	{
		// If we are not in the Playing state then return...we are either paused, in Get Ready state
		// or in the Game Over state in which case we don't want to generate any more rows of bricks.
		if (CurrentState!=GameState.Playing) return;	
		
		// If the wall stop timer is set then the wall has stopped scrolling so we turn down the
		// wall lowering sound and then return (we don't want to generate new rows while the wall
		// is not scrolling.
		if (GetTimer("Wall Stop Timer")>0.0f)
		{
			if (LoweringSound) LoweringSound.volume = 0.0f; 
			return; 
		}
		else
		{
			// Increase the wall speed by a small amount
			WallSpeed *= 1.0001f;
			if (LoweringSound) LoweringSound.volume = LoweringSoundVolume;
		}
			
		// Increase our row interpolator by the fixed time step multiplied by our wall speed.
		// As this goes from 0.0 to 1.0, the bricks all move down one row.
		RowInterpolator += Time.deltaTime * WallSpeed;
		
		// If we breached the 1.0 the bricks have all finished moving down a whole row
		// so time to generate a new row at the top fo the game board (14.0f up)
		if (RowInterpolator>1.0f)
		{
			// Calculkate the Y offset of the new bricks...this will probably not be exactly 14.0f
			// as we may have surpassed the 1.0 between updates. Thus, we will to factor that in and
			// offset out new row from the 14 mark by the correct amount.
			float ypos = 14.0f + (RowInterpolator - 1.0f);
			
			// Reset the row interpolator so we can start timing when the next row is due
			RowInterpolator = 0.0f;
			
			// We want rows to be symmetrical so generate
			// the first 5 bricks and then we will mirror
			// them about the center
			int i=0, bi =0;
			for (i=0; i<5;i++)	
			{
				// Fill array with 5 random brick indices (based on their weights)
				HalfRow[i] = GetNextBrickIndex();
			}
				
			// Uses for mirroring the above array
			int direction = 1;
				
			// the index in the half row we are current processing
			i = 0;
			
			// Create the 11 bricks from -10 to +10 on the X axis
			for (int col =-10; col<12; col+=2)
			{
				// if we have reached the center brick of the row
				// just generate any random brick and then flip the direction
				// we are stepping through HalfRow
				if (i==5)
				{
					bi = GetNextBrickIndex();
					direction = -1;
				}
				else  
				{
					// Otherwise fetch the brick index from the HalfRow array we filled out above
					bi = HalfRow[i];
				}
				
				// What is the chance of a bonus brick being generated here
				float chanceOfBonus = Random.Range(0.0f, 100.0f);		
				
				// Its within the tolerance so create a bonus brick here
				if (chanceOfBonus<BonusChance)
				{
					// Create from bonus array
					Instantiate( BonusPrefabs[GetNextBonusIndex()], new Vector3( (float)col, ypos, 0.0f ), Quaternion.identity);
				
				}
				// otherwise instantiate the random brick (bi) we calculaetd above
				else
				{
					// Create the object from our prefabs array
					Instantiate( BrickPrefabs[bi], new Vector3( (float)col, ypos, 0.0f ), Quaternion.identity);
				}	
			
				// Increase/Decrease direction based on current direction that we are stepping through HalfRow array
				i+=direction;
			}		
		}
		
		// Clamp the wall speed to our maximum wall speed.
		if (WallSpeed>MaxWallSpeed) WallSpeed = MaxWallSpeed;
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	Update
	// Desc	:	Called every frame of the game. 
	// ----------------------------------------------------------------------------------------
	void Update()
	{
		// If the game is not playing yet don't animate anything
		if (CurrentState!=GameState.Playing) return;
	
		// Make sure the cursor is locked
		Screen.lockCursor = true;
		
		// If we have no balls in play and none left in the ball bank
		// end the game.
		if (BallCount+BallsInPlay<1)
		{
			EndGame();
			return;
		}
		
		// If the ESC key is pressed  we need to enter pause mode
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// Set the time scale to zero so all time based animation
			// and rigidbodies are frozen
			Time.timeScale = 0.0f;
			
			// Set game state to paused (this will cause OnGUI to display the pause menu)
			CurrentState = GameState.Paused;
			
			// Unlock the screen cursor so we can see and select things with the mouse pointer
			Screen.lockCursor = false;
			
			// Set the screen managers fade to 0.7 so we darken down the scene to make the
			// pause menu stand out better.
			if (MySceneManager) MySceneManager.SetScreenFade( 0.7f );
			
			// All animations are frozen so lets stop the wall lowering sound too
			if (LoweringSound) LoweringSound.Stop();
		}
					
		// Update the timers
		foreach (KeyValuePair<string, Timer> entry in Timers)
		{
			entry.Value.Tick(Time.deltaTime);	
		}
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	ReleaseBall
	// Desc	:	Can be called by other scripts to force the manager to release a ball from
	//			from the ball back into play. Current this is called only by our controller
	//			in response to a mouse click but you could have a nasty bonus brick that
	//			fires another of your balls off.
	// ----------------------------------------------------------------------------------------
	public void ReleaseBall()
	{
		// If we have balls left in the ball back and we are in playing mode
		if (BallCount>0 && CurrentState==GameState.Playing && BallSpawnPoint!=null)
		{
			// Decrease ball count
			BallCount--;
			
			// Instantiate out ball prefab at the spawn point
			Instantiate( BallPrefab , BallSpawnPoint.position, BallSpawnPoint.rotation );
			
			// Update the text that shows the new number of balls we have left in the bank
			if (BallsText!=null) BallsText.text="Balls : "+BallCount.ToString();
		}
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	EndGame
	// Desc	:	Called when you have no balls left (from Update) or called externally from
	//			brick scripts when a brick has made it passed the minimum line.
	// ----------------------------------------------------------------------------------------
	public void EndGame( float fadeTime = 2.0f )
	{
		// Only do this if we are not already ending the game
		if (CurrentState!=GameState.GameOver)
		{
			// Set state to game over
			CurrentState = GameState.GameOver;
			
			// If the current score is higher than the previous high score
			// store the new high score in the player prefs
			if (CurrentScore>HiScore) PlayerPrefs.SetInt("HiScore", CurrentScore);
			
			// Start the coroutine that fades in the Game Over text and loads in the
			// Main Menu scene.
			StartCoroutine(GotoMenu( fadeTime ));
		}	
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	AddPoint
	// Desc	:	Called by bricks when they are hit to add their points to the player's score.
	// ----------------------------------------------------------------------------------------
	public void AddPoints( int points)
	{		
		// If we are playing 
		if (CurrentState == GameState.Playing)
		{
			// Add the points to the score
			CurrentScore+= points;
			
			// Update the text
			if (CurrentScoreText!=null) CurrentScoreText.text="Score : "+CurrentScore.ToString();
			
			// If the current score is now larger than the high score print then display
			// the current score as the high score as well.
			if (CurrentScore>HiScore) HighScoreText.text="HiScore : "+CurrentScore.ToString();
		}
	}
	
	// ------------------------------------------------------------------------------------------
	// Name	:	RegisterTimer
	// Desc	:	Can be called by any object to register a special timer with the game manager
	//			with the specified name.
	//	-----------------------------------------------------------------------------------------
	public void RegisterTimer( string key )
	{
		// If a timer with this key does not
		// already exist in our dictionary
		if (!Timers.ContainsKey(key))
		{
			// Add the new timer to the dictionary
			Timers.Add( key, new Timer());	
		}
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	GetTimer
	// Desc	:	If a timer exists with the passed name fetch its Timer object from the
	//			Dictionary and return its time.
	// ------------------------------------------------------------------------------------
	public float GetTimer( string key )
	{
		Timer timer = null;
		
		// Does a timer exist with the requested name
		if (Timers.TryGetValue( key, out timer))
		{
			// Return the time
		    return timer.GetTime ();
		}
		
		// No timer found
		return -1.0f;
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	UpdateTimer
	// Desc	:	Called by bonus bricks to add time onto a timer with the passed name.
	// ------------------------------------------------------------------------------------
	public void UpdateTimer( string key, float t)
	{
	 	Timer timer = null;
		if (Timers.TryGetValue( key, out timer))
		{
		    timer.AddTime(t);
		}
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	RegisterBall
	// Desc	:	Called by each ball as it is fired into play
	// ------------------------------------------------------------------------------------
	public void RegisterBall()
	{
		BallsInPlay++;
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	UnregisterBall
	// Desc	:	Called by a ball before it is destroyed
	// ------------------------------------------------------------------------------------
	public void UnregisterBall()
	{
		BallsInPlay --;
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	AddBall
	// Desc	:	Adds more balls to the player's ball bank
	// ------------------------------------------------------------------------------------
	public void AddBall( int noBalls)
	{
		BallCount+=noBalls;
		if (BallsText!=null) BallsText.text="Balls : "+BallCount.ToString();
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	RemoveBall
	// Desc	:	Removes balls from the player's ball bank
	// ------------------------------------------------------------------------------------
	public void RemoveBall( int noBalls)
	{
		BallCount-=noBalls;
		if (BallCount<0) BallCount = 0;
		if (BallsText!=null) BallsText.text="Balls : "+BallCount.ToString();
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	BuildBrickWeightTable
	// Desc	:	Normalizes all the brick weights into the 0.0 to 1.0 range so that each
	//			brick represents the corrected weighted region of that range.
	// ------------------------------------------------------------------------------------
	private void BuildBrickWeightTable()
	{
		// Get number of bricks available
		int noOfPrefabs = BrickPrefabs.Count;
		int sum = 0, i=0;
		
		// Allocate the table to hold a weight for each brick
		BrickWeightTable = new float[ BrickPrefabs.Count ]; 
		
		// Pass 1 add up probabilities
		for (i=0; i<noOfPrefabs; i++)
		{
			// Get the script from the brick
			DestructableItem script = BrickPrefabs[i].GetComponent<DestructableItem>();
			if (script!=null) 
			{
				// Fetch its assigned weight and add it to the current sum of weights
				sum+=script.Weight;
				
				// Store the weight in the weight table
				BrickWeightTable[i] = (float) script.Weight;
			}
		}
		
		// Weight table now stores all the weights for each brick as assigned int he inspector
		// and we have the sum of all weights. Now we do a second pass through the weight
		// table and divide each weight by the sum. This will map all weights into the 0-1 range.

		// Pass 2 normalize probabilities
		for (i=0; i<noOfPrefabs; i++)
		{
			BrickWeightTable[i]/=sum;
		}	
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	BuildBonusWeightTable
	// Desc	:	Read description of above method as this is exactly the same but for
	//			the bonus bricks
	// ------------------------------------------------------------------------------------
	private void BuildBonusWeightTable()
	{
		int noOfPrefabs = BonusPrefabs.Count;
		int sum = 0, i=0;
		BonusWeightTable = new float[ BonusPrefabs.Count ]; 
		
		// Pass 1 add up probabilities
		for (i=0; i<noOfPrefabs; i++)
		{
			DestructableItem script = BonusPrefabs[i].GetComponent<DestructableItem>();
			if (script!=null) 
			{
				sum+=script.Weight;
				BonusWeightTable[i] = (float) script.Weight;
			}
		}
		
		// Pass 2 normalize probabilities
		for (i=0; i<noOfPrefabs; i++)
		{
			BonusWeightTable[i]/=sum;
		}	
	}

	// ------------------------------------------------------------------------------------
	// Name	:	GetNextBrickIndex
	// Desc	:	Fetches a random brick index into the brick prefabs array but factors
	//			in the weights of each brick into the selection so ones with heigher
	//			weights will get chosen more often.
	// ------------------------------------------------------------------------------------
	private int GetNextBrickIndex()
	{
		// Choose a random number between 0 & 1
		float t = Random.value;
		
		// q is used to record how far we have searched 
		// through the 0 to 1 range
		float q = 0.0f;
		
		// Loop through all our prefabs until we find the
		// one that out t value maps to
		for(int i=0; i<BrickPrefabs.Count; i++)
		{
			// Increment q with the normalized weight of 
			// the current brick
			q+= BrickWeightTable[i];
			
			// if t is smaller (or equal) to q then we have
			// found the brick whose weight is mapped to this
			// region of the zero to one range.
			if (t<=q) return i;
		}		
		return 0;
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	GetNextBonusIndex
	// Desc	:	Same as above function but selects a random bonus brick index.
	// ------------------------------------------------------------------------------------
	private int GetNextBonusIndex()
	{
		// Choose a random number between 0 & 1
		float t = Random.value;
		float q = 0.0f;
		
		for(int i=0; i<BonusPrefabs.Count; i++)
		{
			q+= BonusWeightTable[i];
			if (t<=q) return i;
		}		
		return 0;
	}
	
	// -----------------------------------------------------------------------------------
	// Name	:	StartGame
	// Desc	:	This function controls the timed flow of the Start Game sequence and the
	//			transition of the game from the Get Ready state into the Playing state.
	//			As all other method essentially do nothing if not in the Playing state,
	//			it is this method that starts the game after it has displayed its
	//			fade-ins and shown the GetReady text.
	// -----------------------------------------------------------------------------------
	private IEnumerator StartGame()
	{
		// Do a 2 second fade in of the scene
		float timer = 2.0f;
		
		// While counting down from 2 seconds
		while (timer>=0.0f)
		{
			// Decrement timer
			timer-=Time.deltaTime;
			
			// Set the screen fade of the scene manager
			if (MySceneManager) MySceneManager.SetScreenFade( timer/2.0f);
			
			// Fade in the audio listener
			AudioListener.volume  = 1.0f-(timer/4.0f);
			
			// yield control
			yield return null;
		}
		
		// Scene and sound is now fully faded in so lets start to
		// fade in the GetReady text.
		Color 		fadeColor   = Color.yellow;	// We will make the GetReady text yellow
		Material	fadeMaterial= null;			// Will store a reference to the material used by the GetReady text
	
		// If the get ready text exists and has a renderer, activate its game object (enable the text) and fetch
		// a reference to its material.
		if (GetReadyText && GetReadyText.GetComponent<Renderer>()) 
		{			
			GetReadyText.gameObject.SetActive (true);
			fadeMaterial = GetReadyText.GetComponent<Renderer>().material;
		}
		
		// Fade in the alpha of the text over 2 seconds
		timer = 2.0f;
		while (timer>0.0f)
		{
			timer-=Time.deltaTime;
			if (GetReadyText)
			{
				if(fadeMaterial)
				{
					fadeColor.a = 2.0f-timer;
					fadeMaterial.color = fadeColor;
				}
			}
			yield return null;
		}
		
		// Now fade the alpha of the text back out over 2 seconds
		timer = 2.0f;
		while (timer>=0.0)
		{
			timer-=Time.deltaTime;
			if (fadeMaterial)
			{
				fadeColor.a = timer/2.0f;
				fadeMaterial.color = fadeColor;
			}
			
			yield return null;	
		}
		
		// Get Ready text is fully faded out so its time to play.
		// Transition into a playing state
		CurrentState = GameState.Playing;
		
		// And start the wall lowering sound
		if (LoweringSound) LoweringSound.Play();		
	}
	
	// ------------------------------------------------------------------------------------
	// Name	:	GotoMenu
	// Desc	:	This is called when the Game is over either because the final ball has been
	//			lost or because the bricks have gotten to low. Can be passed the duration
	//			over which to execute each section.
	// ------------------------------------------------------------------------------------
	private IEnumerator GotoMenu( float duration = 2.0f)
	{
		// If we wish to do a fading Game Over animation
		if (duration>0.0f)
		{
			// We will use a yellow color for the text
			Color 		fadeColor   = Color.yellow;
			Material	fadeMaterial= null;
			
			// Activate the Game Over game object and fetch
			// a reference to its material
			if (GameOverText && GameOverText.GetComponent<Renderer>()) 
			{
				GameOverText.gameObject.SetActive( true );
				fadeMaterial = GameOverText.GetComponent<Renderer>().material;
			}
			
			// Fade in the alpha of the Game Over text whilst
			// fading out the volume of the listener.
			float timer = duration;
			while (timer>0.0f)
			{
				timer-=Time.deltaTime;
				if (GetReadyText)
				{
					if(fadeMaterial)
					{
						fadeColor.a = duration-timer;
						fadeMaterial.color = fadeColor;
					}
				}
				
				AudioListener.volume = timer/duration;
				yield return null;
			}
			
			// Game Over text is fully faded in and audio listener is complete turned down
			// so now lets fade out the scene.
			timer = 0.0f;
			while (timer<=duration)
			{
				timer+=Time.deltaTime;		
				if (MySceneManager) MySceneManager.SetScreenFade( timer/duration);
				yield return null;	
			}
		}
		
		// In blackout now so unlock the cursor and load in the Main Menu scene.
		Screen.lockCursor = false;
		Application.LoadLevel("Menu Scene");
	}
	
	// -----------------------------------------------------------------------------
	// Name	:	OnGUI
	// Desc	:	Called automatically by Unity whenever the GUI for the scene needs
	//			repainting. 
	// -----------------------------------------------------------------------------
	void OnGUI()
	{	
		// If we are not paused return
		if (CurrentState!=GameState.Paused) return;
		
		// Assign the custom GUI Skin used by our game
		if (GameSkin) GUI.skin = GameSkin;
		
		//Set up scaling matrix so we can work in EASY coordinates
		float rx = Screen.width / 1920.0f;
		float ry =  Screen.height/1280.0f;
		
		// Backup the current GUI matrix as we are about to change it so we can work
		// in standard coordinates for all resolutions
		Matrix4x4 oldMat = GUI.matrix;

		// Set up the scaling matrix
		GUI.matrix = Matrix4x4.TRS (new Vector3(0, 0, 0), Quaternion.identity, new Vector3 (rx, ry, 1));	
	
		
		// Create the transparent options box below the header
		GUI.Box(PauseBox_ScreenRect, "Game Paused");
		
		// Create and react to the Continue button being pressed
		if (GUI.Button( Continue_ScreenRect, "Continue"))		
		{
			// Make sure time scale is resumed to normal as this
			// was set to zero for pause mode
			Time.timeScale = 1.0f;
			
			// Transition from pause mode back into playing mode
			CurrentState = GameState.Playing;
			
			// Lock the cursor again
			Screen.lockCursor = true;
			
			// Play the Lowering sound
			if (LoweringSound) LoweringSound.Play();
			
			// Get rid of any screen fade
			if (MySceneManager) MySceneManager.SetScreenFade( 0.0f );
		}
		
		// Create and react to the Quit button
		if (GUI.Button( Quit_ScreenRect, "Quit")) 
		{
			// Renable Timescale
			Time.timeScale = 1.0f;
			
			// Call EndGame method to load Main Menu
			EndGame(0.0f);
		};
		
		// Restore the matrix
		GUI.matrix = oldMat;	
		
	
	}
}
