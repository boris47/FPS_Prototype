using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public static CameraControl Instance = null;

	public GameObject pPlayer = null;

	float fPosition_X = 0.0f;
	float fPosition_Y = 0.0f;
	float fPosition_Z = 0.0f;


	float fCurrentRotation_X = 0.0f;
	float fCurrentRotation_Y = 0.0f;


	// Use this for initialization
	void Start () {
		
		Instance = this;

	}
	
	// Update is called once per frame
	void Update () {

		// Position
		transform.position = pPlayer.transform.position -  ( new Vector3 ( 0.0f, 0.0f, -10.0f ) );

		fCurrentRotation_X = Mathf.Lerp( fCurrentRotation_X, Input.GetAxis ( "Mouse X" ) * 4.5f, Time.deltaTime * 50f );
		fCurrentRotation_Y = Mathf.Lerp( fCurrentRotation_Y, Input.GetAxis ( "Mouse Y" ) * 4.5f, Time.deltaTime * 50f );

		//  Horizzontal Rotation
		transform.Rotate ( new Vector3( -fCurrentRotation_Y, 0.0f, 0.0f ), Space.Self );
		
		//  Vertical Rotation
		transform.Rotate ( new Vector3( 0.0f, fCurrentRotation_X, 0.0f ), Space.World );

//		PlayerController.Instance.transform.Rotate( 0.0f, fCurrentRotation_X, 0.0f );

//		transform.rotation = PlayerController.Instance.transform.rotation;


	}
}
