using UnityEngine;
using System.Collections;

// --------------------------------------------------------------------------------------------
// Class : SceneManager_Base
// Desc	 : Abtract base class that all our scene managers are derived from. It specifies the
//		   interface and implements the singleton design pattern for finding any objects
//		   derived from this type in the scene.
// --------------------------------------------------------------------------------------------
public abstract class SceneManager_Base : MonoBehaviour 
{
	// Has an action been selected (like a button)
	public bool		  ActionSelected  = false;
	
	// How faded in or out is the scene (1.0f = the black quad is fully opaque)
	protected float ScreenFade	=	1.0f;
	
	// Static Singleton Instance
	protected static SceneManager_Base _Instance		= null;
	
	// Property to get instance
	public static SceneManager_Base Instance
	{
		get { 
				// If we don't an instance yet find it in the scene hierarchy
				if (_Instance==null) { _Instance = (SceneManager_Base)FindObjectOfType(typeof(SceneManager_Base)); }
				
				// Return the instance
				return _Instance;
			}
	}
	
	// Methods that can be implented in derived classes to provide custom behaviors
	// in response to mouse events over buttons and elements.
	public virtual void OnButtonHover ( string buttonName ){}
	public virtual void OnButtonSelect( string buttonName ){}
	
	// Gets/Sets the current screen fade value
	public float		GetScreenFade () 				   { return ScreenFade; }
	public void			SetScreenFade ( float screenFade)  { ScreenFade = screenFade; }
}
