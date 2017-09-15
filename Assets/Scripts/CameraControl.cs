using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControl : MonoBehaviour {

	public static CameraControl Instance				= null;

	const float MAX_CAMERA_OFFSET = 15f;
	const float MIN_CAMERA_OFFSET = 1.5f;

	[Tooltip("Camera Target")]
	public	Transform		pTarget						= null;

	[Range( 0.2f, 20.0f )]
	public	float			fMouseSensitivity			= 1.0f;

	public	bool			bTPSMode					= false;

	public	bool			bSmoothedRotation			= true;
	public	bool			bSmoothedPosition			= true;

	[Range( 1.0f, 10.0f )]
	public float			fSmoothFactor				= 1.0f;

	private float			fCurrentRotation_X_Delta	= 0.0f;
	private float			fCurrentRotation_Y			= 0.0f;
	private float			fCurrentRotation_Y_Delta	= 0.0f;

	private float			fCameraOffset				= 5.0f;
	private float			fCurrentCameraOffset		= 5.0f;

	// Use this for initialization
	void Start() {
		
		if ( Instance == null )
			Instance = this;
		else {
			Destroy( gameObject );
			return;
		}

		DontDestroyOnLoad( this );

	}

	private void Update() {

		if ( Input.GetKeyDown( KeyCode.V ) ) bTPSMode = !bTPSMode;

		if ( Input.GetAxis("Mouse ScrollWheel") > 0f && fCameraOffset > MIN_CAMERA_OFFSET )
			fCameraOffset -= 0.5f;

		if ( Input.GetAxis("Mouse ScrollWheel") < 0f && fCameraOffset < MAX_CAMERA_OFFSET )
			fCameraOffset += 0.5f;

	}

	void LateUpdate() {

		// Direction
		if ( bSmoothedRotation )
			fCurrentRotation_X_Delta = Mathf.Lerp( fCurrentRotation_X_Delta, Input.GetAxis ( "Mouse X" ) * fMouseSensitivity, Time.deltaTime * ( 100f / fSmoothFactor ) );
		else
			fCurrentRotation_X_Delta = Input.GetAxis ( "Mouse X" ) * fMouseSensitivity;

		transform.Rotate( Vector3.up, fCurrentRotation_X_Delta, Space.World );


		if ( bSmoothedRotation )
			fCurrentRotation_Y_Delta = Mathf.Lerp( fCurrentRotation_Y_Delta, Input.GetAxis ( "Mouse Y" ) * fMouseSensitivity, Time.deltaTime * ( 100f / fSmoothFactor ) );
		else
			fCurrentRotation_Y_Delta = Input.GetAxis ( "Mouse Y" ) * fMouseSensitivity;

		transform.Rotate( Vector3.left, fCurrentRotation_Y_Delta, Space.Self );

		// TODO CLAMP X AXIS



		fCurrentCameraOffset = Mathf.Lerp( fCurrentCameraOffset, fCameraOffset, Time.deltaTime * 6f );

		// Position
		if ( bTPSMode ) {
			if ( bSmoothedPosition )
				transform.position = Vector3.Lerp( transform.position, pTarget.position - ( transform.forward * fCurrentCameraOffset ), Time.deltaTime * 8f );
			else
				transform.position = pTarget.position - ( transform.forward * fCurrentCameraOffset );
		}
		else {
			transform.position = pTarget.position;
		}


		// Effects


//		if ( PlayerController.IsMoving )
//			this.Headbob_update();
//		else
			this.Headmove_update();

    }
}
