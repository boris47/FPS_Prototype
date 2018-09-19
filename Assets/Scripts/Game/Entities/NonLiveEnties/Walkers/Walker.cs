
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
	public		override	void	OnHit( IBullet bullet )
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
	public		override	void	OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
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
	// OnTargetAquired ( Override )
	public		override	void	OnTargetAquired( TargetInfo_t targetInfo )
	{
		base.OnTargetAquired( targetInfo );

		// PathFinding
		CheckForNewReachPoint( m_TargetInfo.CurrentTarget.Transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetUpdate ( Override )
	public		override	void	OnTargetUpdate( TargetInfo_t targetInfo )
	{
		m_TargetInfo = targetInfo;

		SetPoinToFace( m_TargetInfo.CurrentTarget.Transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public		override	void	OnTargetChanged( TargetInfo_t targetInfo )
	{
		base.OnTargetChanged( targetInfo );

		// PathFinding
		CheckForNewReachPoint( m_TargetInfo.CurrentTarget.Transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public		override	void	OnTargetLost( TargetInfo_t targetInfo )
	{
		base.OnTargetLost( targetInfo );

		// Stop moving
		m_Brain.Stop(); // temp, cheasing feature awaiting

		// now point to face is target position
		SetPoinToFace( targetInfo.CurrentTarget.Transform.position );

		m_NavTargetNodeIndex = -1;

		m_Brain.TryToReachPoint( targetInfo.CurrentTarget.Transform.position );

		// SEEKING MODE

		// TODO Set brain to SEKKER mode
//		m_Brain.ChangeState( BrainState.SEEKER );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFrame ( Override )
	protected	override	void	OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );

		// Update internal timer
		m_ShotTimer -= deltaTime;

		// Update navigation
		if ( m_HasDestination == true )
		{
			NavUpdate( Speed: m_MoveMaxSpeed, DeltaTime: deltaTime );
		}

		// Update targeting
		if ( m_TargetInfo.HasTarget == true )
		{
			if ( m_Brain.State != BrainState.ATTACKING )
			{
				m_Brain.ChangeState( BrainState.ATTACKING );
			}

			// Update PathFinding and movement along path
			if ( m_HasDestination && ( transform.position - m_TargetInfo.CurrentTarget.Transform.position ).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance )
			{
				CheckForNewReachPoint( m_TargetInfo.CurrentTarget.Transform.position );
				m_NavCanMoveAlongPath = true;;
			}
			else
			{
				m_NavCanMoveAlongPath = false;
			}

			// with a target, if gun alligned, fire
			if ( m_IsAllignedGunToPoint == true )
			{
				FireLongRange( deltaTime );
			}
		}
		
		// if has point to face, update entity orientation
		if ( m_HasPointToFace )
		{
			FaceToPoint( deltaTime );   // m_PointToFace
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// NavUpdate ( Override )
	protected	override	void	NavUpdate( float Speed, float DeltaTime )
	{
		Speed = ( m_IsAllignedFootsToDestination ) ? m_MoveMaxSpeed : ( m_MoveMaxSpeed * 0.5f );

		base.NavUpdate( Speed, DeltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	// NavMove ( Override )
	protected	override	void	NavMove( Vector3 CurrentDestination, float Speed, float DeltaTime )
	{
		// NAVIGATION
		// FOOTS
		Vector3 pointOnThisPlane = Utils.Math.ProjectPointOnPlane( m_FootsTransform.up, m_FootsTransform.position, CurrentDestination );
		Vector3 dirToPosition = ( pointOnThisPlane - m_FootsTransform.position );

		m_IsAllignedFootsToDestination = Vector3.Angle( m_FootsTransform.forward, dirToPosition ) < 3.0f;
		if ( m_IsAllignedFootsToDestination == false )
		{
			float angle = Vector3.SignedAngle( m_FootsTransform.forward, dirToPosition, m_FootsTransform.up );
			m_FootsTransform.Rotate( m_FootsTransform.up, angle * m_FeetsRotationSpeed * DeltaTime, Space.Self );
		}

		m_RigidBody.velocity = transform.forward * Speed * 10f * DeltaTime + transform.up * 0.1f;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Override )
	public		override	void	OnKill()
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
	// FaceToPoint ( Override )
	protected	override	void	FaceToPoint( float DeltaTime )
	{
		// ORIENTATION
		// BODY
		{
			// Nothing, rotation not allowed here
		}
		// HEAD
		{
			Vector3 pointOnThisPlane = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_HeadTransform.position, m_PointToFace );
			Vector3 dirToPosition = ( pointOnThisPlane - m_HeadTransform.position );

			m_IsAllignedHeadToPoint = Vector3.Angle( m_HeadTransform.forward, dirToPosition ) < 2f;
			if ( m_IsAllignedHeadToPoint == false )
			{
				float angle = Vector3.SignedAngle( m_HeadTransform.forward, dirToPosition, m_BodyTransform.up );
				float speed = m_HeadRotationSpeed * (float)m_Brain.State;// ( m_Brain.State == BrainState.ATTACKING ? 10.0f : 1.0f );
				m_HeadTransform.Rotate( m_BodyTransform.up, angle * speed * DeltaTime, Space.Self );
			}
		}
		// GUN
		{
			Vector3 dirToPosition = ( m_PointToFace - m_GunTransform.position );

			if ( m_IsAllignedHeadToPoint == true )
			{
				float angle = Vector3.SignedAngle( m_GunTransform.forward, dirToPosition, m_HeadTransform.up );
				m_GunTransform.Rotate( m_HeadTransform.up, angle * m_GunRotationSpeed * DeltaTime, Space.Self );
			}
			m_IsAllignedGunToPoint = Vector3.Angle( m_GunTransform.forward, dirToPosition ) < 6f;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireLongRange ( Override )
	protected	override	void	FireLongRange( float deltaTime )
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
		m_IsAllignedBodyToPoint	= false;
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
