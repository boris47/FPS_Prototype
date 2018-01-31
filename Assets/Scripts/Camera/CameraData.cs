using UnityEngine;
using System.Collections;

[ CreateAssetMenu( menuName = "Camera/Create Data", order = 1 ) ]
public class CameraData : ScriptableObject {
	
	[SerializeField][Range( 0f, 10f )]
	public	float	AmplitudeX					= 0.0f;

	[SerializeField][Range( 0f, 10f )]
	public	float	AmplitudeY					= 0.0f;

	[SerializeField][Range( 0f, 10f )]
	public	float	SpeedX						= 1.0f;
	[SerializeField][Range( 0f, 10f )]
	public	float	SpeedY						= 1.0f;
	
	[SerializeField][Range( 0f, 10f )]
	public	float	ThetaUpdateX				= 5f;

	[SerializeField][Range( 0f, 10f )]
	public	float	ThetaUpdateY				= 2.5f;

}
