using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public static PlayerController Instance = null;

	Rigidbody pRigidBody = null;

	// Use this for initialization
	void Start () {
		
		CameraControl.Instance.pPlayer = gameObject;

		pRigidBody = GetComponent<Rigidbody>();

		Instance = this;

	}
	
	// Update is called once per frame
	void Update () {
		
		float 	fMove 			= Input.GetAxis ( "Vertical" );
		float 	fStrafe			= Input.GetAxis ( "Horizontal" );

		Vector3 vTargetVelocity = new Vector3( fStrafe, 0.0f, fMove );

		if ( ( fMove != 0.0f ) && ( fStrafe != 0.0f  ) ) vTargetVelocity *= 0.707f;

		pRigidBody.velocity = CameraControl.Instance.transform.TransformDirection( vTargetVelocity );



	}

}
