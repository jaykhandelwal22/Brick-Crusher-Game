// --------------------------------------------------------------------------------------
// Class : Timer
// Desc	 : Simpler timer class that will always count down to zero if set to
//		   a non-zero amount
// --------------------------------------------------------------------------------------		   
public class Timer
{
	// Internal Timer
	private float timer = 0.0f;
	
	// ----------------------------------------------------------------------------------
	// Constructor
	// Initializes timer to zero
	// ----------------------------------------------------------------------------------
	public Timer()
	{
		timer = 0.0f;
	}
	
	// ----------------------------------------------------------------------------------
	// Name : Tick
	// Desc	:	Perform an update of the camera passing the number of seconds to update
	// ----------------------------------------------------------------------------------
	public void Tick ( float seconds )
	{
		// Decrement the timer by seconds passed
		timer -= seconds;
		
		// Clamp timer to zero
		if (timer<0.0f) timer = 0.0f;
	}
	
	// ----------------------------------------------------------------------------------
	// Name	:	AddTime
	// Desc	:	Add seconds to timer
	// ----------------------------------------------------------------------------------
	public void AddTime ( float seconds)
	{
		timer+=seconds;
	}
	
	// ----------------------------------------------------------------------------------
	// Name	:	GetTime
	// Desc	:	Get value fo timer
	// ----------------------------------------------------------------------------------
	public float GetTime ()
	{
		return timer;	
	}
}
