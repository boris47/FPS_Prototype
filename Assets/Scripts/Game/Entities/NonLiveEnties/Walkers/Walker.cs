
using UnityEngine;

public abstract class Walker : NonLiveEntity, IRespawn {

	[Header("Walker Properties")]

	[SerializeField]
	protected	float			m_MoveMaxSpeed				= 3f;

	[SerializeField]
	protected	Bullet			m_Bullet					= null;

	[SerializeField]
	protected	float			m_ShotDelay					= 0.7f;

	[SerializeField]
	protected	float			m_DamageMax					= 2f;

	[SerializeField]
	protected	float			m_DamageMin					= 0.5f;

	[SerializeField, ReadOnly]
	protected	int				m_PoolSize					= 5;


	//////////////////////////////////////////////////////////////////////////

	protected	override	void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			m_Health				= m_SectionRef.AsFloat( "Health", 60.0f );

			float shieldStatus		= m_SectionRef.AsFloat( "Shield", 0.0f );
			if ( m_Shield != null )
				( m_Shield as IShield ).Status = shieldStatus;

			m_DamageMax				= m_SectionRef.AsFloat( "DamageMax", 2.0f );
			m_DamageMin				= m_SectionRef.AsFloat( "DamageMin", 0.5f );
			m_PoolSize				= m_SectionRef.AsInt( "PoolSize", m_PoolSize );

			m_EntityType			= ENTITY_TYPE.ROBOT;
		}

		// BULLETS POOL CREATION
		if ( m_Pool == null )		// check for respawn
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
					this.SetCollisionStateWith( o.Collider, state: false );

					// this allow to receive only trigger enter callback
					Player.Instance.DisableCollisionsWith( o.Collider );
				}
			);
		}
		m_Pool.SetActive( true );
		m_ShotTimer = 0f;
		m_MaxAgentSpeed = m_MoveMaxSpeed;
	}

	
	//////////////////////////////////////////////////////////////////////////
	
	public		override	void	OnDestinationReached( Vector3 Destionation )
	{
		base.OnDestinationReached( Destionation );
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

		if ( m_RespawnPoint != null )
		{
			m_RespawnPoint.Respawn( this, 2f );
		}
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
		/*
		// HEAD
		{
			Vector3 pointOnThisPlane = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_HeadTransform.position, m_PointToFace );
			Vector3 dirToPosition = ( pointOnThisPlane - m_HeadTransform.position );

			m_IsAllignedHeadToPoint = Vector3.Angle( m_HeadTransform.forward, dirToPosition ) < 12f;
			{
				float speed = m_HeadRotationSpeed * ( ( m_TargetInfo.HasTarget ) ? 3.0f : 1.0f );

				m_RotationToAllignTo.SetLookRotation( dirToPosition, m_BodyTransform.up );
				m_HeadTransform.rotation = Quaternion.RotateTowards( m_HeadTransform.rotation, m_RotationToAllignTo, speed * Time.deltaTime );
			}
		}
		*/
		// GUN
		{
			Vector3 pointToLookAt = m_LookData.PointToLookAt;
			if ( m_TargetInfo.HasTarget == true )
			{
				Vector3 targetPosition = m_TargetInfo.CurrentTarget.Transform.position;
				Bullet model = m_Pool.GetAsModel();
				if ( model.MotionType == BulletMotionType.PARABOLIC )
				{
					// BALLISTIC TRAJECTORY
					float targetHeight = targetPosition.y;
					float angle = Utils.Math.CalculateFireAngle
					(
						alt:			0f,
						startPosition:	m_GunTransform.position,
						endPosition:	pointToLookAt,
						bulletVelocity:	model.Velocity,
						targetHeight:	targetHeight
					);
					Vector3 ballisticVelocity = Utils.Math.BallisticVelocity
					(
						startPosition:	m_GunTransform.position,
						destination:	targetPosition,
						angle:			angle
					);

					targetPosition += ballisticVelocity;
				}

				// PREDICTION
				pointToLookAt = Utils.Math.CalculateBulletPrediction
				(
					shooterPosition:	m_GunTransform.position,
					shooterVelocity:	m_NavAgent.velocity,
					shotSpeed:			m_Pool.GetAsModel().Velocity,
					targetPosition:		targetPosition,
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
		
		Vector3 direction = m_FirePoint.forward;
		{
			direction.x += Random.Range( -m_FireDispersion, m_FireDispersion );
			direction.y += Random.Range( -m_FireDispersion, m_FireDispersion );
			direction.z += Random.Range( -m_FireDispersion, m_FireDispersion );
		}
		direction.Normalize();
		bullet.Shoot( position: m_FirePoint.position, direction: direction );
		
		m_FireAudioSource.Play();
	}
	

	//////////////////////////////////////////////////////////////////////////

	void IRespawn.OnRespawn()
	{
		transform.position = m_RespawnPoint.transform.position;
		transform.rotation = m_RespawnPoint.transform.rotation;

		gameObject.SetActive( true );

		// Entity
		m_IsActive						= true;
		m_TargetInfo					= new TargetInfo();
//		m_NavHasDestination				= false;
//		m_HasFaceTarget					= false;
//		m_Destination					= Vector3.zero;
//		m_PointToFace					= Vector3.zero;
		m_NavCanMoveAlongPath						= false;
		m_IsAllignedBodyToPoint	= false;
//		m_DistanceToTravel				= 0f;

		// NonLiveEntity
		m_ShotTimer						= 0f;
		m_IsAllignedGunToPoint			= false;

		// Reinitialize properties
		Awake();

//		m_Brain.OnReset();
		if ( m_Shield != null )
			( m_Shield as IShield ).OnReset();
	}
	
}
