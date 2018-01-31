
using UnityEngine;

[System.Serializable]
public class HeadMovement {

	const	float			STEP_VALUE					= 0.8f;

	[SerializeField][ReadOnly]
	private float			m_EditorSpeed				= 0f;
	[SerializeField][ReadOnly]
	private float			m_EditorAmplitude			= 0f;

	private	bool			m_IsActive					= true;
	public	bool			IsActive
	{
		get; set;
	}
	private	bool			m_CanPlaySteps				= false;
	public	bool			CanPlaySteps
	{
		set { m_CanPlaySteps = value; }
	}

	private	Vector3			m_Direction					= Vector3.zero;
	public	Vector3			Direction
	{
		get { return m_Direction; }
	}

	public	CameraData		CurrentData
	{
		get; set;
	}

	private float			m_SpeedX					= 0f;
	private float			m_SpeedY					= 0f;
	private float			m_AmplitudeX				= 0f;
	private float			m_AmplitudeY				= 0f;
	private	float			m_ThetaUpdateX				= 0f;
	private	float			m_ThetaUpdateY				= 0f;
	private float			m_ThetaX					= 0f;
	private float			m_ThetaY					= 0f;
	private	bool			m_StepDone					= false;



	private	void	GetSpeed( LiveEntity liveEntity, ref float speed_X, ref float speed_Y  )
	{
		float	stamina		= liveEntity.Stamina;
		bool	isRunning	= liveEntity.IsRunning;
		bool	isCrouched	= liveEntity.IsCrouched;

		float speedX	= m_SpeedY * Time.deltaTime;
//		speedX			*= ( isRunning )	?	1.70f : 1.00f );
		speedX			*= ( isCrouched )	?	0.80f : 1.00f;
		speedX			*= ( 4.0f - ( stamina * 2.0f ) );
		speed_X			= speedX;

		float speedY	= m_SpeedX * Time.deltaTime;
//		speedY			*= ( isRunning )	?	1.70f : 1.00f );
		speedY			*= ( isCrouched )	?	0.80f : 1.00f;
		speedY			*= ( 4.0f - ( stamina * 2.0f ) );
		speed_Y			= speedY;
	}


	private	void	GetAmplitude( LiveEntity liveEntity, ref float amplitude_X, ref float amplitude_Y )
	{
		float	stamina		= liveEntity.Stamina;
		bool	isRunning	= liveEntity.IsRunning;
		bool	isCrouched	= liveEntity.IsCrouched;

		float amplitudeX	= m_AmplitudeX;
		amplitudeX			*= ( ( isRunning )	?	2.00f : 1.00f );
		amplitudeX			*= ( ( isCrouched )	?	0.80f : 1.00f );
		amplitudeX			*= ( 5.0f - ( stamina * 4.0f ) );
		amplitude_X = amplitudeX;

		float amplitudeY	= m_AmplitudeY;
		amplitudeY			*= ( ( isRunning )	?	2.00f : 1.00f );
		amplitudeY			*= ( ( isCrouched )	?	0.80f : 1.00f );
		amplitudeY			*= ( 5.0f - ( stamina * 4.0f ) );
		amplitude_Y = amplitudeY;
	}


	public void Update( LiveEntity pLiveEntity )
	{

		if ( m_IsActive == false )
			return;

		m_AmplitudeX	= Mathf.Lerp( m_AmplitudeX,		CurrentData.AmplitudeX,		Time.deltaTime * 2f );
		m_AmplitudeY	= Mathf.Lerp( m_AmplitudeY,		CurrentData.AmplitudeY,		Time.deltaTime * 2f );

		m_SpeedX		= Mathf.Lerp( m_SpeedX,			CurrentData.SpeedX,			Time.deltaTime * 2f );
		m_SpeedY		= Mathf.Lerp( m_SpeedY,			CurrentData.SpeedY,			Time.deltaTime * 2f );
		m_ThetaUpdateX	= Mathf.Lerp( m_ThetaUpdateX,	CurrentData.ThetaUpdateX,	Time.deltaTime * 2f );
		m_ThetaUpdateY	= Mathf.Lerp( m_ThetaUpdateY,	CurrentData.ThetaUpdateY,	Time.deltaTime * 2f );

		float speedX = 0f;
		float speedY = 0f;
		float amplitudeX = 0f;
		float amplitudeY = 0f;

		GetSpeed( pLiveEntity, ref speedX, ref speedY );
		GetAmplitude( pLiveEntity, ref amplitudeX, ref amplitudeY );

		m_ThetaX += m_ThetaUpdateY * speedX;
		m_ThetaY += ( m_ThetaUpdateX + Random.Range( 0.0f, 0.03f ) ) * speedY;

		if ( m_ThetaX > 360f ) m_ThetaX -= 360f;
		if ( m_ThetaY > 360f ) m_ThetaY -= 360f;

		m_EditorSpeed = speedX + speedY;
		m_EditorAmplitude = amplitudeX + amplitudeY;

		m_Direction.Set
		(
			-Mathf.Cos( m_ThetaX ) * amplitudeY,
			 Mathf.Cos( m_ThetaY ) * amplitudeX,
			 0f
		);


		if ( m_CanPlaySteps )
		{
			// Steps
			if ( Mathf.Abs( Mathf.Cos( m_ThetaY ) ) > ( STEP_VALUE ) )
			{
				if ( m_StepDone == false )
				{
					( pLiveEntity.Foots as IFoots ).PlayStep();
					m_StepDone = true;
				}
			}
			else
			{
				m_StepDone = false;
			}

		}
	}



	public void Reset( bool bInstantly = false )
	{
		if ( bInstantly )
			m_Direction = Vector3.zero;
		else
		{
			m_Direction = Vector3.Lerp ( Direction, Vector3.zero, Time.deltaTime / 3f );
		}
	}

}
