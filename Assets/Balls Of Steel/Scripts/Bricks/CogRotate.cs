using UnityEngine;
using System.Collections;

public class CogRotate : MonoBehaviour {
	
	public 	float		Speed		= 0.0f; 		
	private Transform 	MyTransform = null;
	
	// Use this for initialization
	void Start () 
	{
		MyTransform = transform;
	}
	
	// Update is called once per frame
	void Update () 
	{
		MyTransform.Rotate( Vector3.forward * Time.deltaTime * Speed );
	}
}
