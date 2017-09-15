using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControl : MonoBehaviour {
	
	const float HEADMOVE_AMPLITUDE		= 0.2f;
	const float HEADMOVE_SPEED			= 1.0f;
	const float	HEADMOVE_THETA_UPDATE_X	= 4.5f;
	const float	HEADMOVE_THETA_UPDATE_Y	= 2.25f;

	float fHeadmove_ThetaX				= 0.0f;
	float fHeadmove_ThetaY				= 0.0f;

	float fHeadmove_CurrentX			= 0.0f;
	float fHeadmove_CurrentY			= 0.0f;

	bool bHeadmove_Active = true;

	private void Headmove_update() {

		if ( bHeadmove_Active == false ) return;

		float fSpeed		= HEADMOVE_SPEED * Time.deltaTime * 1.0f /*fSpeedMul*/;
		float fAmplitude	= HEADMOVE_AMPLITUDE * 1.0f /*fAmplitudeMul*/;

		fHeadmove_ThetaX += HEADMOVE_THETA_UPDATE_X * fSpeed;
		fHeadmove_ThetaY += HEADMOVE_THETA_UPDATE_Y * fSpeed;

		if ( fHeadmove_ThetaX > 360 ) fHeadmove_ThetaX -= 360;
		if ( fHeadmove_ThetaY > 360 ) fHeadmove_ThetaY -= 360;

		fHeadmove_CurrentX = Mathf.Lerp( fHeadmove_CurrentX, Mathf.Cos( fHeadmove_ThetaX ) * fAmplitude, Time.deltaTime * 5f );
		fHeadmove_CurrentY = Mathf.Lerp( fHeadmove_CurrentY, Mathf.Cos( fHeadmove_ThetaY ) * fAmplitude, Time.deltaTime * 5f );
	
//		print( fBobbing_CurrentX );

		transform.Rotate( Vector3.up,	fHeadmove_CurrentX, Space.World );
		transform.Rotate( Vector3.left, fHeadmove_CurrentY, Space.Self  );

	}




}
