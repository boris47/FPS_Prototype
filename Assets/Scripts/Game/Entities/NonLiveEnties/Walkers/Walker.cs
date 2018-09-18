
using UnityEngine;


public abstract class Walker : NonLiveEntity, IRespawn {

	[Header("Walker Properties")]

	[SerializeField]
	protected	Bullet			m_Bullet					= null;

	[SerializeField]
	protected	float			m_ShotDelay					= 0.7f;

	[SerializeField]
	protected	float			m_MoveMaxSpeed				= 3f;

	[SerializeField]
	protected	float			m_DamageMax					= 2f;

	[SerializeField]
	protected	float			m_DamageMin					= 0.5f;

	[SerializeField, ReadOnly]
	protected	int				m_PoolSize					= 5;



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
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
					o.Setup( m_DamageMin, m_DamageMax, canPenetrate : false, whoRef : this, weapon : null );
					o.Setup( whoRef: this, weapon: null );
					this.SetCollisionStateWith( o.Collider, state: false );

					// this allow to receive only trigger enter callback
					Player.Instance.DisableCollisionsWith( o.Collider );
				}
			);
		}
		m_Pool.SetActive( true );
		m_ShotTimer = 0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public override void OnHit( IBullet bullet )
	{
		// Avoid friendly fire
		if ( bullet.WhoRef is NonLiveEntity )
			return;

		// BRAIN
		// Hit event, set ALARMED State if actual is NORMAL
		if ( m_Brain.State == BrainState.NORMAL )
		{
			m_Brain.ChangeState( BrainState.ALARMED );
			SetPoinToFace( bullet.StartPosition );
		}

		// DAMAGE
		// Shiled damage
		if ( m_Shield != null && m_Shield.Status > 0f && m_Shield.IsUnbreakable == false )
		{
			m_Shield.OnHit( bullet );
		}
		// Direct damage
		else
		{
			float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
			m_Health -= damage;

			if ( m_Health <= 0f )
			{
				OnKill();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public override void OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		print( name + " OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate )" );
		/*
		base.OnHit( startPosition, whoRef, 0f );

		if ( m_Brain.State != BrainState.ATTACKING )
		{
			SetPoinToFace( startPosition );
		}

		if ( m_Shield != null && m_Shield.Status > 0f && m_Shield.IsUnbreakable == false )
		{
			m_Shield.OnHit( damage );
			return;
		}

		m_Health -= damage;

		if ( m_Health <= 0f )
			OnKill();
		*/
	}
	


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Override )
	public override void OnKill()
	{
		base.OnKill();
//		m_Pool.SetActive( false );
		gameObject.SetActive( false );

		if ( m_RespawnPoint != null )
		{
//			m_RespawnPoint.Respawn( this, 2f );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public override void OnTargetAquired( TargetInfo_t targetInfo )
	{
		base.OnTargetAquired( targetInfo );

		// PathFinding
		CheckForNewReachPoint( m_TargetInfo.CurrentTarget.Transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public override void OnTargetChanged( TargetInfo_t targetInfo )
	{
		base.OnTargetChanged( targetInfo );

		// PathFinding
		CheckForNewReachPoint( m_TargetInfo.CurrentTarget.Transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public override void OnTargetLost( TargetInfo_t targetInfo )
	{
		base.OnTargetLost( targetInfo );

		// Stop moving
		m_Brain.Stop(); // temp, cheasing feature awaiting

		// now point to face is target position
		SetPoinToFace( targetInfo.CurrentTarget.Transform.position );

		m_TargetNodeIndex = -1;

		m_Brain.TryToReachPoint( targetInfo.CurrentTarget.Transform.position );

		// SEEKING MODE

		// TODO Set brain to SEKKER mode
		m_Brain.ChangeState( BrainState.SEEKER );

	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	protected override void OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );

		// Update internal timer
		m_ShotTimer -= deltaTime;


		if ( m_NavHasDestination == true )
			NavUpdate( deltaTime, Speed: ( m_IsAllignedBodyToDestination ) ? m_MoveMaxSpeed : ( m_MoveMaxSpeed * 0.5f ) );


		if ( m_TargetInfo.HasTarget == true )
		{
			if ( m_Brain.State != BrainState.ATTACKING )
				m_Brain.ChangeState( BrainState.ATTACKING );

			SetPoinToFace( m_TargetInfo.CurrentTarget.Transform.position );

			// PathFinding
			CheckForNewReachPoint( m_TargetInfo.CurrentTarget.Transform.position );

			if ( m_NavHasDestination && ( transform.position - m_TargetInfo.CurrentTarget.Transform.position ).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance )
			{
				m_NavCanMoveAlongPath = true;;
			}
			else
			{
				m_NavCanMoveAlongPath = false;
			}
		}
		
		// if has target point to face at set
		if ( m_HasPointToFace )
		{
			FaceToPoint( deltaTime );   // m_PointToFace
		}
		
		// if body is alligned with target start moving
		if ( m_IsAllignedBodyToDestination )
		{
//			m_NavCanMoveAlongPath = true;
		}
		else
		{
//			m_NavCanMoveAlongPath = false;
		}

	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Override )
	protected override void FaceToPoint( float deltaTime )
	{
		/*
		m_FootsTransform = transform
		m_BodyTransform = transform.Find( "Body" );
		m_HeadTransform = m_BodyTransform.Find( "Head" );
		m_GunTransform = m_HeadTransform.Find( "Gun" );
		m_FirePoint = m_GunTransform.Find( "FirePoint" );
		*/

		//		Vector3 pointOnThisPlane		= Utils.Math.ProjectPointOnPlane( transform.up, transform.position, m_PointToFace );

		//		Vector3 dirToPosition			= ( pointOnThisPlane - transform.position );
		//		Vector3 dirGunToPosition		= ( m_PointToFace - m_HeadTransform.position );

		// FOOTS
		{
			Vector3 pointOnThisPlane = Utils.Math.ProjectPointOnPlane( transform.up, m_FootsTransform.position, m_PointToFace );
			Vector3 dirToPosition = ( pointOnThisPlane - m_FootsTransform.position );

			m_IsAllignedBodyToDestination = Vector3.Angle( m_FootsTransform.forward, dirToPosition ) < 30.0f;
//			if ( m_IsAllignedBodyToDestination == false )
			{
				m_FootsTransform.Rotate( m_BodyTransform.up, Vector3.SignedAngle( m_FootsTransform.forward, dirToPosition, m_BodyTransform.up ) * m_BodyRotationSpeed * deltaTime, Space.Self );

//				Quaternion bodyRotation = Quaternion.LookRotation( dirToPosition, transform.up );
//				m_FootsTransform.rotation = Quaternion.RotateTowards( transform.rotation, bodyRotation, m_BodyRotationSpeed * deltaTime );
			}
		}
		// BODY
		{
			// Nothing
		}
		// HEAD
		{
			Vector3 pointOnThisPlane = Utils.Math.ProjectPointOnPlane( transform.up, m_HeadTransform.position, m_PointToFace );
			Vector3 dirToPosition = ( pointOnThisPlane - m_HeadTransform.position );

			m_IsAllignedHeadToPoint = Vector3.Angle( m_HeadTransform.forward, dirToPosition ) < 2f;
			if ( m_IsAllignedBodyToDestination == true )
			{

				float speed = m_HeadRotationSpeed * ( m_TargetInfo.HasTarget ? 5.0f : 1.0f );
				m_HeadTransform.Rotate( m_BodyTransform.up, Vector3.SignedAngle( m_HeadTransform.forward, dirToPosition, m_BodyTransform.up ) * speed * deltaTime, Space.Self );

//				Quaternion bodyRotation = Quaternion.LookRotation( dirToPosition, transform.up );
//				m_HeadTransform.rotation = Quaternion.RotateTowards( transform.rotation, bodyRotation, m_BodyRotationSpeed * deltaTime );
			}
		}
		// GUN
		{
			Vector3 dirToPosition = ( m_PointToFace - m_HeadTransform.position );

			if ( m_IsAllignedHeadToPoint == true )
			{
//				m_GunTransform.forward		= Vector3.RotateTowards( m_GunTransform.forward, dirToPosition, m_GunRotationSpeed * deltaTime, 0.0f );

				m_GunTransform.Rotate( m_HeadTransform.up, Vector3.SignedAngle( m_GunTransform.forward, dirToPosition, m_HeadTransform.up ) * m_GunRotationSpeed * deltaTime, Space.Self );
			}
			m_IsAllignedGunToPoint = Vector3.Angle( m_GunTransform.forward, dirToPosition ) < 2f;
		}








		/*
		m_IsAllignedBodyToDestination	= Vector3.Angle( transform.forward, dirToPosition ) < 2f;
		if ( m_IsAllignedBodyToDestination == false )
		{
			Quaternion bodyRotation = Quaternion.LookRotation( dirToPosition, transform.up );
			transform.rotation = Quaternion.RotateTowards( transform.rotation, bodyRotation, m_BodyRotationSpeed * deltaTime );
		}

		{
			m_HeadTransform.forward		= Vector3.RotateTowards( m_HeadTransform.forward, dirGunToPosition, m_GunRotationSpeed * deltaTime, 0.0f );
		}

		m_IsAllignedHeadToPoint			= Vector3.Angle( m_HeadTransform.forward, dirGunToPosition ) < 3f;
		if ( m_IsAllignedHeadToPoint == true )
		{
			m_GunTransform.forward		= Vector3.RotateTowards( m_HeadTransform.forward, dirGunToPosition, m_GunRotationSpeed * deltaTime, 0.0f );
		}
		*/
	}
	
	/*
	//////////////////////////////////////////////////////////////////////////
	// Stop ( Virtual )
	protected	virtual	void	Stop()
	{
		if ( m_Brain.State == BrainState.NORMAL )
		{
			m_HasFaceTarget					= false;
			m_PointToFace					= Vector3.zero;
			m_IsAllignedBodyToDestination	= false;
		}
		m_NavHasDestination				= false;
		m_Destination					= Vector3.zero;
		m_IsMoving						= false;
		m_RigidBody.velocity			= Vector3.zero;
		m_RigidBody.angularVelocity		= Vector3.zero;
		m_StartMovePosition				= Vector3.zero;
		m_DistanceToTravel				= 0f;
	}
	*/
	/*
	//////////////////////////////////////////////////////////////////////////
	// GoAtPoint ( Override )
	protected override	void	GoAtPoint( float deltaTime )
	{
		if ( m_NavHasDestination == false )
			return;

		if ( m_DistanceToTravel < m_MinEngageDistance * m_MinEngageDistance )
		{
			Stop();
			return;
		}

		Vector3 dirToPosition	 = ( m_PointToFace - transform.position );
		float	travelledDistance = ( m_StartMovePosition - transform.position ).sqrMagnitude;
		if ( ( m_Destination - transform.position ).sqrMagnitude < 4f || travelledDistance > m_DistanceToTravel )   // point reached
		{
			if ( m_Brain.State != BrainState.NORMAL )
				m_Brain.ChangeState( BrainState.NORMAL );

			Stop();
			return;
		}

		m_RigidBody.velocity	= dirToPosition.normalized * m_MoveMaxSpeed * deltaTime * m_RigidBody.drag;
	}
	*/

	//////////////////////////////////////////////////////////////////////////
	// FireLongRange ( Override )
	protected override void FireLongRange( float deltaTime )
	{
		if ( m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;
		
		IBullet bullet = m_Pool.GetComponent();
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward );
		
		m_FireAudioSource.Play();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnRespawn
	void IRespawn.OnRespawn()
	{
		transform.position = m_RespawnPoint.transform.position;
		transform.rotation = m_RespawnPoint.transform.rotation;

		gameObject.SetActive( true );

		// Entity
		m_IsActive						= true;
		m_TargetInfo					= default( TargetInfo_t );
//		m_NavHasDestination				= false;
//		m_HasFaceTarget					= false;
//		m_Destination					= Vector3.zero;
//		m_PointToFace					= Vector3.zero;
		m_NavCanMoveAlongPath						= false;
		m_IsAllignedBodyToDestination	= false;
		m_StartMovePosition				= Vector3.zero;
//		m_DistanceToTravel				= 0f;

		// NonLiveEntity
		m_ShotTimer						= 0f;
		m_IsAllignedGunToPoint			= false;

		// Reinitialize properties
		Awake();

		m_Brain.OnReset();
		if ( m_Shield != null )
			( m_Shield as IShield ).OnReset();
	}
	
}
