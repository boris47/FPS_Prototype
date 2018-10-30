
using System;
using UnityEngine;


public abstract class Turret : NonLiveEntity {

	[Header("Turret Properties")]

	[SerializeField]
	private		Bullet			m_Bullet					= null;

	[SerializeField]
	protected	float			m_ShotDelay					= 0.7f;

	[SerializeField]
	protected	float			m_DamageMax					= 2f;

	[SerializeField]
	protected	float			m_DamageMin					= 0.5f;

	[SerializeField, ReadOnly]
	protected	int				m_PoolSize					= 5;

	private		Laser			m_Laser						= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			GameManager.Configs.GetSection( m_SectionName, ref m_SectionRef );
			if ( m_SectionRef == null )
			{
				print( "Cannot find cfg section for entity " + name );
				Destroy( gameObject );
				return;
			}

			m_Health				= m_SectionRef.AsFloat( "Health", 60.0f );

			float shieldStatus		= m_SectionRef.AsFloat( "Shield", 0.0f );
			if ( m_Shield != null )
				( m_Shield as IShield ).Status = shieldStatus;

			m_DamageMax				= m_SectionRef.AsFloat( "DamageMax", 2.0f );
			m_DamageMin				= m_SectionRef.AsFloat( "DamageMin", 0.5f );
			m_PoolSize				= m_SectionRef.AsInt( "PoolSize", m_PoolSize );

			m_EntityType			= ENTITY_TYPE.ROBOT;
		}

		m_Laser = GetComponentInChildren<Laser>();
		if ( m_Laser != null )
		{
			m_Laser.LaserLength = m_FieldOfView.Distance;
			m_Laser.LayerMaskToExclude = LayerMask.NameToLayer("Bullets");
		}

		// BULLETS POOL CREATION
		{
			GameObject	bulletGO		= m_Bullet.gameObject;
			m_Pool = new GameObjectsPool<Bullet>
			(
				model			: bulletGO,
				size			: ( uint ) m_PoolSize,
				containerName	: name + "BulletPool",
				actionOnObject	: ( Bullet o ) =>
				{
					o.SetActive( false );
					o.Setup
					(
						canPenetrate: false,
						whoRef: this,
						weaponRef: null,
						damageMin: m_DamageMin,
						damageMax: m_DamageMin
					);
					this.SetCollisionStateWith( o.Collider, false );

					// this allow to receive only trigger enter callback
					Player.Instance.DisableCollisionsWith( o.Collider );
				}
			);
		}

		m_Pool.SetActive( true );
		m_ShotTimer = 0f;

		// AI BEHAVIOURS
		{	m_Behaviours = new AIBehaviour[ 5 ] { null, null, null,null, null };
			SetBehaviour( BrainState.EVASIVE,	m_SectionRef.AsString( "BehaviourEvasive"	), false );
			SetBehaviour( BrainState.NORMAL,	m_SectionRef.AsString( "BehaviourNormal"	), true  );
			SetBehaviour( BrainState.ALARMED,	m_SectionRef.AsString( "BehaviourAlarmed"	), false );
			SetBehaviour( BrainState.SEEKER,	m_SectionRef.AsString( "BehaviourSeeker"	), false );
			SetBehaviour( BrainState.ATTACKER,	m_SectionRef.AsString( "BehaviourAttacker"	), false );

			ChangeState( BrainState.NORMAL );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	
	protected	override	void	OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	
	protected		override	void	OnKill()
	{
		base.OnKill();
//		m_Pool.SetActive( false );
		gameObject.SetActive( false );
	}
	
	//////////////////////////////////////////////////////////////////////////

	protected	override		void	UpdateHeadRotation()
	{
		base.UpdateHeadRotation();
		// ORIENTATION
		// BODY
		{
			// Nothing, rotation not allowed here
		}

		// GUN
		{
			Vector3 pointToLookAt = m_LookData.PointToLookAt;
			if ( m_TargetInfo.HasTarget == true ) // PREDICTION
			{
				// Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity
				pointToLookAt = Utils.Math.CalculateBulletPrediction
				(
					shooterPosition:	m_GunTransform.position,
					shooterVelocity:	Vector3.zero,
					shotSpeed:			m_Pool.GetAsModel().Velocity,
					targetPosition:		m_TargetInfo.CurrentTarget.Transform.position,
					targetVelocity:		m_TargetInfo.CurrentTarget.RigidBody.velocity
				);
			}

			Vector3 dirToPosition = ( pointToLookAt - m_GunTransform.position );
			if ( m_IsAllignedHeadToPoint == true )
			{
				m_RotationToAllignTo.SetLookRotation( dirToPosition, m_BodyTransform.up );
				m_GunTransform.rotation = Quaternion.RotateTowards( m_GunTransform.rotation, m_RotationToAllignTo, m_GunRotationSpeed * Time.deltaTime );
			}
			m_IsAllignedGunToPoint = Vector3.Angle( m_GunTransform.forward, dirToPosition ) < 16f;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	
	public	override		void	FireLongRange()
	{
		if ( m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;
		
		IBullet bullet = m_Pool.GetComponent();
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		
		m_FireAudioSource.Play();
	}
}
