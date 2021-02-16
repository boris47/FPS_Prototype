
using UnityEngine;

public abstract class Walker : NonLiveEntity {

	[Header("Walker Properties")]

	[SerializeField]
	protected	float			m_MoveMaxSpeed				= 3f;
/*
	[SerializeField]
	protected	Bullet			m_Bullet					= null;

	[SerializeField]
	protected	float			m_ShotDelay					= 0.7f;

	[SerializeField]
	protected	float			m_DamageMax					= 2f;

	[SerializeField]
	protected	float			m_DamageMin					= 0.5f;

	[SerializeField, ReadOnly]
	protected	uint			m_PoolSize					= 5;
*/
	protected 	override EEntityType			m_EntityType { get { return EEntityType.ROBOT; } }

	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			m_Health				= m_SectionRef.AsFloat( "Health", 60.0f );

			if (m_Shield.IsNotNull())
			{
				float shieldStatus	= m_SectionRef.AsFloat( "Shield", 0.0f );
				m_Shield.Setup( shieldStatus, EShieldContext.ENTITY );
			}

	//		m_DamageMax				= m_SectionRef.AsFloat( "DamageMax", 2.0f );
	//		m_DamageMin				= m_SectionRef.AsFloat( "DamageMin", 0.5f );
	//		m_PoolSize				= m_SectionRef.AsUInt( "PoolSize", m_PoolSize );
		}
		


	/*	// BULLETS POOL CREATION
		if (m_Pool == null )		// check for respawn
		{
			GameObject	bulletGO		= m_Bullet.gameObject;
			GameObjectsPoolConstructorData<Bullet> data = new GameObjectsPoolConstructorData<Bullet>(bulletGO, m_PoolSize)
			{
				ContainerName			= name + "BulletPool",
				CoroutineEnumerator		= null,
				IsAsyncBuild			= true,
				ActionOnObject			= ( Bullet o ) =>
				{
					o.SetActive( false );
					o.Setup
					(
						whoRef: this,
						weaponRef: null
					);

					// this allow to receive only trigger enter callback
				//	Player.Instance.DisableCollisionsWith( o.Collider, bAlsoTriggerCollider: false );
				}
			};
			m_Pool = new GameObjectsPool<Bullet>( data );
		}

		m_Pool.SetActive( true );
		m_ShotTimer = 0f;
	*/	m_MaxAgentSpeed = m_MoveMaxSpeed;
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
/*			Vector3 pointToLookAt = m_LookData.PointToLookAt;
			if (m_TargetInfo.HasTarget == true )
			{
				Vector3 targetPosition = m_TargetInfo.CurrentTarget.AsEntity.transform.position;
				IBullet model = m_Pool.TryPeekComponentAs<IBullet>();
				if ( model.MotionType == EBulletMotionType.PARABOLIC )
				{
					// BALLISTIC TRAJECTORY
					float targetHeight = targetPosition.y;
					float angle = Utils.Math.CalculateFireAngle
					(
					//	alt:			0f,
						startPosition: m_GunTransform.position,
						endPosition:	pointToLookAt,
						bulletVelocity:	model.Velocity,
						targetHeight:	targetHeight
					);
					Vector3 ballisticVelocity = Utils.Math.BallisticVelocity
					(
						startPosition: m_GunTransform.position,
						destination:	targetPosition,
						angle:			angle
					);

					targetPosition += ballisticVelocity;
				}

				// PREDICTION
				pointToLookAt = Utils.Math.CalculateBulletPrediction
				(
					shooterPosition: m_GunTransform.position,
					shooterVelocity: m_NavAgent.velocity,
					shotSpeed:			model.Velocity,
					targetPosition:		targetPosition,
					targetVelocity: m_TargetInfo.CurrentTarget.RigidBody.velocity
				);
			}


			Vector3 dirToPosition = ( pointToLookAt - m_GunTransform.position );
			if (m_IsAllignedHeadToPoint == true )
			{
				m_RotationToAllignTo.SetLookRotation( dirToPosition, m_BodyTransform.up );
				m_GunTransform.rotation = Quaternion.RotateTowards(m_GunTransform.rotation, m_RotationToAllignTo, m_GunAllignmentSpeed * Time.deltaTime );
			}
			m_IsAllignedGunToPoint = Vector3.Angle(m_GunTransform.forward, dirToPosition ) < 16f;
*/		}
	}

	//////////////////////////////////////////////////////////////////////////
	/*
	public	override		void	FireLongRange()
	{
		if (m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;

		IBullet bullet = m_Pool.GetNextComponent();
		
		Vector3 direction = m_FirePoint.forward;
		{
			direction.x += Random.Range( -m_FireDispersion, m_FireDispersion );
			direction.y += Random.Range( -m_FireDispersion, m_FireDispersion );
			direction.z += Random.Range( -m_FireDispersion, m_FireDispersion );
		}
		direction.Normalize();
		bullet.Shoot( position: m_FirePoint.position, direction: direction, velocity: null );

		m_FireAudioSource.Play();
	}
	*/
}
