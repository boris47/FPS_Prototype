using UnityEngine;
using System.Collections;

public class TurretGatling : Turret {

	private	const	float		FIRE_SPREAD					= 0.03f;
	private	const	float		MAX_ROTATION_SPEED			= 3000f;
	private	const	float		ROTATION_ACC				= 1200f;
	private	const	float		ROTATION_DEACC				= 400f;

	private		bool			m_IsActivated				= false;

	private		float			m_RotationSpeed				= 0.0f;

	private		Transform		m_GatlingTransform			= null;

	private		Vector3			m_DispersionVector			= Vector3.zero;


	//////////////////////////////////////////////////////////////////////////
	
	protected override void Awake()
	{
		this.m_SectionName = this.GetType().FullName;

		base.Awake();

		this.m_GatlingTransform = this.m_GunTransform.Find( "Gatling" );
	}


	//////////////////////////////////////////////////////////////////////////

	protected override void OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );

		// DECELLERATION
		if (this.m_TargetInfo.HasTarget == false )
		{
			this.m_IsActivated = false;
			this.m_RotationSpeed = Mathf.Max(this.m_RotationSpeed - ROTATION_DEACC * deltaTime, 0f );
		}

		// APPLY ROTATION
		if (this.m_RotationSpeed > 0.0f )
		{
			this.m_GatlingTransform.Rotate( Vector3.right, this.m_RotationSpeed * deltaTime, Space.Self );
		}

		// ACTIVATION
		if (this.m_TargetInfo.HasTarget == true )
		{
			this.m_RotationSpeed = Mathf.Clamp(this.m_RotationSpeed + ROTATION_ACC * deltaTime, 0f, MAX_ROTATION_SPEED );
			if (this.m_RotationSpeed >= MAX_ROTATION_SPEED )
			{
				this.m_IsActivated = true;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	
	public	override		void	FireLongRange()
	{
		if (this.m_IsActivated == false )
			return;

		if (this.m_ShotTimer > 0 )
				return;

		this.m_ShotTimer = this.m_ShotDelay;
		
		IBullet bullet = this.m_Pool.GetNextComponent();

		// Add some dispersion
		this.m_DispersionVector.Set
		(
			Random.Range( -FIRE_SPREAD, FIRE_SPREAD ),
			Random.Range( -FIRE_SPREAD, FIRE_SPREAD ),
			Random.Range( -FIRE_SPREAD, FIRE_SPREAD )
		);

		Vector3 direction = (this.m_FirePoint.forward + this.m_DispersionVector ).normalized;

		bullet.Shoot( position: this.m_FirePoint.position, direction: direction );

		this.m_FireAudioSource.Play();
	}

}
