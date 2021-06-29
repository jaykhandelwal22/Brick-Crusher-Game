using UnityEngine;
using System.Collections;

// --------------------------------------------------------------------------------------------
// Class : SceneCamera
// Desc	 : Hooks into the OnPostRender event to render a quad over the screen for 
//		   fade-in/fade outs. WIll search for an object derived from SceneManager_Base
//		   and will set its transparency according to its current screen fade value.
// Use	 : MUST be attached to a game object with a camera component. 
// --------------------------------------------------------------------------------------------
public class SceneCamera : MonoBehaviour 
{
	// The material that we will use to draw our transparent screen quad
	private Material 			FadeMaterial 	= null;
	
	// The scene manager object which contains the current screen fade value
	private SceneManager_Base	MySceneManager	=	null;
	
	// ----------------------------------------------------------------------------------------
	// Name	:	Awake
	// Desc	:	Called when the game object is first created. Searches for an instance of
	//			a SceneManager_Base derived object in the scene.
	// ----------------------------------------------------------------------------------------
	void Awake()
	{
		MySceneManager = SceneManager_Base.Instance;	
	}
	
	// ----------------------------------------------------------------------------------------
	// Name	:	OnPostRender
	// Desc	:	Called after the camera attached to this game object has rendered the scene.
	//			We use this to draw a transparent quad over the final image that is rendered.
	// ----------------------------------------------------------------------------------------
	void OnPostRender() 
	{
		// If we have a valid Scene Manager object and its screen fade strength is not
		// zero.
    	if (MySceneManager!=null && MySceneManager.GetScreenFade()!=0.0f)
		{
			
			// If we have not created our material/shader yet, create ashader that renders a 
			// coloured quad over the screen with depth testing disabled.
		    if(!FadeMaterial) 
			{
		        FadeMaterial = new Material( "Shader \"Hidden/CameraFade\" {" +
		             "Properties { _Color (\"Main Color\", Color) = (1,1,1,0) }" +
					"SubShader {" +
		            "    Pass {" +
		            "        ZTest Always Cull Off ZWrite Off "+
					"		 Blend SrcAlpha OneMinusSrcAlpha "+
		            "        Color [_Color]" +
		            "    }" +
		            "}" +
		            "}"
		        );
		    }
   			
			// Set the color to black and the alpha value to whatever the scene manager gave us as the current
			// fade intensity.
			FadeMaterial.SetColor("_Color", new Color(0.0f,0.0f,0.0f, MySceneManager.GetScreenFade()) );
			
			// Draw a quad over the whole screen with the above shader using the GL class.
			// First save the state of the cameras view/projection matrix so we can restore it
			// before we leave
		    GL.PushMatrix ();
			
			// Create an Orthographic projection matrix
		    GL.LoadOrtho ();
			
			// For each pass in the material (only one in ours)
		    for (var i = 0; i < FadeMaterial.passCount; ++i) 
			{
				// Set the material for the current pass
		        FadeMaterial.SetPass (i);
				
				// We want to draw a quad so interpret the next
				// four vertex3 statements as the corners of this quad
		        GL.Begin( GL.QUADS );
				
				// The quad corner points
		        GL.Vertex3( 0, 0, 0.1f );
		        GL.Vertex3( 1, 0, 0.1f );
		        GL.Vertex3( 1, 1, 0.1f );
		        GL.Vertex3( 0, 1, 0.1f );
				
				// We are done rendering quads
		        GL.End();
		    }
			
			// Restore the camera's view/projection matrix
		    GL.PopMatrix (); 
		}
	}
}
