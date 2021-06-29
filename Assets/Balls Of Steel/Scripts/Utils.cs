using UnityEngine;
using System.Collections;

public class Utils 
{
	
	public static void SetActiveRecursively(GameObject rootObject, bool active)
    {
       rootObject.SetActive(active);
 
       foreach (Transform childTransform in rootObject.transform)
       {
          SetActiveRecursively(childTransform.gameObject, active);
       }
    }
}
