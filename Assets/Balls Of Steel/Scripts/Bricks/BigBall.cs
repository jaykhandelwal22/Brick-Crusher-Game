using UnityEngine;
using System.Collections;

public class BigBall : DestructableItem 
{
	public float	BigTime = 15;
	
	protected override void Start()
	{
		base.Start();
		MyGameManager.RegisterTimer("Big Ball");
	}
	

	
	public override void KillBrick()
	{
		if (Dying) return;
		if (MyGameManager)
		{
			MyGameManager.UpdateTimer("Big Ball", BigTime);
		}
		base.KillBrick();
	}
}
