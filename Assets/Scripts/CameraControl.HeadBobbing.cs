using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControl : MonoBehaviour {
	
	const float HEADBOB_AMPLITUDE		= 0.3f;
	const float HEADBOB_SPEED			= 1.0f;
	const float	HEADBOB_THETA_UPDATE	= 1.0f;

	float fHeadbob_ThetaX				= 0.0f;
	float fHeadbob_ThetaY				= 0.0f;

	float fHeadbob_CurrentX				= 0.0f;
	float fHeadbob_CurrentY				= 0.0f;

	bool bHeadbob_Active = true;

	private void Headbob_update() {

		if ( bHeadbob_Active == false ) return;

		float fSpeed		= HEADBOB_SPEED * Time.deltaTime * 1.0f /*fSpeedMul*/;
		float fAmplitude	= HEADBOB_AMPLITUDE * 1.0f /*fAmplitudeMul*/;

		fHeadbob_ThetaX += HEADBOB_THETA_UPDATE * fSpeed;
		fHeadbob_ThetaY += ( HEADBOB_THETA_UPDATE / 2f ) * fSpeed;

		if ( fHeadbob_ThetaX > 360 ) fHeadbob_ThetaX -= 360;
		if ( fHeadbob_ThetaY > 360 ) fHeadbob_ThetaY -= 360;

		fHeadbob_CurrentX = Mathf.Lerp( fHeadbob_CurrentX, Mathf.Cos( fHeadbob_ThetaX ) * fAmplitude, Time.deltaTime );
		fHeadbob_CurrentY = Mathf.Lerp( fHeadbob_CurrentY, Mathf.Cos( fHeadbob_ThetaY ) * fAmplitude, Time.deltaTime );
		
//		print( fBobbing_CurrentX );

		transform.Rotate( Vector3.up,	fHeadbob_CurrentX, Space.World );
		transform.Rotate( Vector3.left, fHeadbob_CurrentY, Space.Self  );

	}




}
