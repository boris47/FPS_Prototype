using UnityEngine;
using System.Collections;

public class TurretGatling : Turret {

	private	const	float	MAX_ROTATION_SPEED		= 20f;
	private	const	float	ROTATION_ACC			= 0.2f;
	private	const	float	ROTATION_DEACC			= 0.1f;

	private     bool        m_IsActivating			= false;
	private		bool		m_IsActivated			= false;

	private		float		m_RotationSpeed			= 0f;

	private		Transform	m_GatlingTransform		= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();

		m_GatlingTransform = m_GunTransform.Find( "Gatling" );
	}



	//////////////////////////////////////////////////////////////////////////
	// Update
	private void Update()
	{
		// DECELLERATION
		if ( m_TargetInfo.HasTarget == false )
		{
			m_IsActivated = false;
			m_RotationSpeed = Mathf.Max( m_RotationSpeed - ROTATION_DEACC, 0f );
		}

		// APPLY ROTATION
		if ( m_RotationSpeed > 0f )
			m_GatlingTransform.Rotate( Vector3.right, m_RotationSpeed, Space.Self );

		// ACTIVATION
		if ( m_TargetInfo.HasTarget == true)
		{
			m_RotationSpeed = Mathf.Clamp( m_RotationSpeed + ROTATION_ACC, 0f, MAX_ROTATION_SPEED );
			if ( m_RotationSpeed < MAX_ROTATION_SPEED )
			{
				return;
			}
			m_IsActivated = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireLongRange ( Override )
	protected override void FireLongRange( float deltaTime )
	{
		if ( m_IsActivated == false )
			return;

		base.FireLongRange( deltaTime );

		if ( m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;
		
		IBullet bullet = m_Pool.GetComponent();
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		
		m_FireAudioSource.Play();
	}

}
