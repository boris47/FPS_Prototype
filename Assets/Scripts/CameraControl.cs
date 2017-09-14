using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

	public static CameraControl Instance	= null;

	public Transform pPlayer				= null;

	bool bTPSMode = false;


	float fCurrentRotation_X_Delta;
	float fCurrentRotation_Y;
	float fCurrentRotation_Y_Delta;
	


	// Use this for initialization
	void Awake() {
		
		Instance = this;

	}

	private void Update() {
		
		if ( Input.GetKeyDown( KeyCode.V ) ) bTPSMode = !bTPSMode;

	}

	void LateUpdate() {

		// Direction
		fCurrentRotation_X_Delta = Mathf.Lerp( fCurrentRotation_X_Delta, Input.GetAxis ( "Mouse X" ) * 4.5f, Time.deltaTime * 6f );
		transform.Rotate( Vector3.up, fCurrentRotation_X_Delta, Space.World );

		fCurrentRotation_Y_Delta = Mathf.Lerp( fCurrentRotation_Y_Delta, Input.GetAxis ( "Mouse Y" ) * 4.5f, Time.deltaTime * 6f );
		transform.Rotate( Vector3.left, fCurrentRotation_Y_Delta, Space.Self );

		// TODO CLAMP X AXIS

		// Position
		if ( bTPSMode )
			transform.position = Vector3.Lerp( transform.position, pPlayer.transform.position - ( transform.forward * 10f ), Time.deltaTime * 8f );
		else {
			transform.position = pPlayer.transform.position;
		}

    }
}
