
using UnityEngine;


public abstract class Walker : NonLiveEntity, IRespawn {

	[SerializeField]
	protected	GameObject		m_BulletGameObject			= null;

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

	protected	Vector3			m_ScaleVector				= new Vector3( 1.0f, 0.0f, 1.0f );


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
			GameObject	bulletGO		= m_BulletGameObject;
			m_Pool = new GameObjectsPool<Bullet>
			(
				model			: ref bulletGO,
				size			: ( uint ) m_PoolSize,
				containerName	: name + "BulletPool",
				actionOnObject	: ( Bullet o ) =>
				{
					o.SetActive( false );
					o.Setup( damageMin : m_DamageMin, damageMax : m_DamageMax, canPenetrate : false, whoRef : this, weapon : null );
					o.Setup( whoRef: this, weapon: null );
					Physics.IgnoreCollision( o.Collider, m_PhysicCollider, ignore : true );
					Physics.IgnoreCollision( o.Collider, Player.Entity.PhysicCollider );
					Physics.IgnoreCollision( o.Collider, Player.Instance.PlayerNearAreaTrigger );
					Physics.IgnoreCollision( o.Collider, Player.Instance.PlayerFarAreaTrigger );
					if ( m_Shield != null )
						Physics.IgnoreCollision( o.Collider, m_Shield.Collider, ignore : true );
				}
			);
		}
		m_Pool.SetActive( true );
		m_ShotTimer = 0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public override void OnHit( ref IBullet bullet )
	{
		// Avoid friendly fire
		if ( bullet.WhoRef is NonLiveEntity )
			return;
		
		base.OnHit( ref bullet ); // set start bullet position as point to face at if not attacking

		if ( m_TargetInfo.HasTarget == false && bullet is GranadeBase )
		{
			m_PointToFace = bullet.Transform.position;
		}

		m_DistanceToTravel	= ( transform.position - m_PointToFace ).sqrMagnitude;
		m_Destination = bullet.Transform.position;
		m_HasDestination = true;

		if ( m_Shield != null && m_Shield.Status > 0f && m_Shield.IsUnbreakable == false )
		{
			m_Shield.OnHit( ref bullet );
			return;
		}

		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;

		if ( m_Health <= 0f )
			OnKill();
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
			m_RespawnPoint.Respawn( this, 2f );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public override void OnTargetLost( TargetInfo_t targetInfo )
	{
		// SEEKING MODE

		// now point to face is target position
		m_PointToFace = m_TargetInfo.CurrentTarget.transform.position;
		m_HasFaceTarget = true;

		// now point to reach is target position
		m_Destination = m_TargetInfo.CurrentTarget.transform.position;
		m_HasDestination = true;

		// Set brain to SEKKER mode
		m_Brain.ChangeState( BrainState.SEEKER );

		// Reset internal ref to target
		base.OnTargetLost( targetInfo );		// m_TargetInfo = default( TargetInfo_t );
	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Override )
	protected override void FaceToPoint( float deltaTime )
	{
		Vector3 dirToPosition			= ( m_PointToFace - transform.position );
		Vector3 dirGunToPosition		= ( m_PointToFace - m_GunTransform.position );

		Vector3 vBodyForward			= Vector3.Scale( dirToPosition,	m_ScaleVector );
		transform.forward				= Vector3.RotateTowards( transform.forward, vBodyForward, m_BodyRotationSpeed * deltaTime, 0.0f );
		
		m_IsAllignedBodyToDestination	= Vector3.Angle( transform.forward, vBodyForward ) < 7f;
		if ( m_IsAllignedBodyToDestination && m_TargetInfo.HasTarget == true )
		{
			m_GunTransform.forward		=  Vector3.RotateTowards( m_GunTransform.forward, dirGunToPosition, m_GunRotationSpeed * deltaTime, 0.0f );
		}

		m_AllignedGunToPoint			= Vector3.Angle( m_GunTransform.forward, dirGunToPosition ) < 3f;
	}


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
		m_HasDestination				= false;
		m_Destination					= Vector3.zero;
		m_IsMoving						= false;
		m_RigidBody.velocity			= Vector3.zero;
		m_RigidBody.angularVelocity		= Vector3.zero;
		m_StartMovePosition				= Vector3.zero;
		m_DistanceToTravel				= 0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// GoAtPoint
	protected override	void	GoAtPoint( float deltaTime )
	{
		if ( m_HasDestination == false )
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
	// EnterSimulationState ( Override )
	public override void EnterSimulationState()
	{
		base.EnterSimulationState();
	}


	//////////////////////////////////////////////////////////////////////////
	// SimulateMovement ( Override )
	public override	bool	SimulateMovement( SimulationMovementType movementType, Vector3 destination, Transform target, float deltaTime, float interpolant = 0f )
	{
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	// ExitSimulationState ( Override )
	public override void ExitSimulationState()
	{
		base.ExitSimulationState();
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
		m_HasDestination				= false;
		m_HasFaceTarget					= false;
		m_Destination					= Vector3.zero;
		m_PointToFace					= Vector3.zero;
		m_IsMoving						= false;
		m_IsAllignedBodyToDestination	= false;
		m_StartMovePosition				= Vector3.zero;
		m_DistanceToTravel				= 0f;

		// NonLiveEntity
		m_ShotTimer						= 0f;
		m_AllignedGunToPoint			= false;

		// Reinitialize properties
		Awake();

		m_Brain.OnReset();
		if ( m_Shield != null )
			( m_Shield as IShield ).OnReset();
	}
	
}
