using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sun : MonoBehaviour {
	
	public		Vector3 vRotation	= Vector3.zero;

	void FixedUpdate()  {

		transform.Rotate( vRotation );

	}
}
