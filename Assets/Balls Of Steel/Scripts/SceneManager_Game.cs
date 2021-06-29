using UnityEngine;
using System.Collections;

// --------------------------------------------------------------------------------------------
// Class : SceneManager_Game
// Desc	 : Scene manager used by our main game. Simply sets the initials screen fade to fully
//		   faded out (quad is fully opaque) at startup
// --------------------------------------------------------------------------------------------
public class SceneManager_Game : SceneManager_Base 
{	
	// ----------------------------------------------------------------------------------------
	// Name	:	Start
	// Desc	:	Sets the current screen fade to 1.0 (fully opaque) which means the scene starts
	//			fully in darkness
	// ----------------------------------------------------------------------------------------
	void Start()
	{
		ScreenFade = 1.0f;	
	}
}
