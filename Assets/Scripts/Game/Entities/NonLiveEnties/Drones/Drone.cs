
using UnityEngine;
using System.Collections;

public abstract class Drone : NonLiveEntity, IRespawn {

	[Header("Drone Properties")]

	[SerializeField]
	private		Bullet			m_Bullet					= null;

	[SerializeField]
	protected	float			m_ShotDelay					= 0.7f;

	[SerializeField]
	protected	float			m_DamageLongRangeMax		= 2f;

	[SerializeField]
	protected	float			m_DamageLongRangeMin		= 0.5f;

	[SerializeField]
	protected	float			m_MoveMaxSpeed				= 3f;

	protected	int				m_PoolSize					= 5;

	protected	Vector3			m_ScaleVector				= new Vector3( 1.0f, 0.0f, 1.0f );


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			GameManager.Configs.GetSection( m_SectionName = gameObject.name, ref m_SectionRef );
			if ( m_SectionRef == null )
			{
				print( "Cannot find cfg section for entity " + name );
				Destroy( gameObject );
				return;
			}

			m_Health				= m_SectionRef.AsFloat( "Health",				30.0f );
			float shieldStatus		= m_SectionRef.AsFloat( "Shield",				60.0f );
			if ( m_Shield != null )
				( m_Shield as IShield ).Status = shieldStatus;

			m_MoveMaxSpeed			= m_SectionRef.AsFloat( "MoveMaxSpeed",			1.0f );

			m_DamageLongRangeMax	= m_SectionRef.AsFloat( "DamageLongRangeMax",	2.0f );
			m_DamageLongRangeMin	= m_SectionRef.AsFloat( "DamageLongRangeMin",	0.5f );

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
					o.Setup( damageMin : m_DamageLongRangeMin, damageMax : m_DamageLongRangeMax, canPenetrate : false, whoRef : this, weapon : null );
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
			return;
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
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public		override	void	OnTargetAquired( TargetInfo_t targetInfo )
	{
		base.OnTargetAquired( targetInfo );

		// PathFinding
		NavGoto( targetInfo.CurrentTarget.Transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetUpdate ( Override )
	public		override	void	OnTargetUpdate( TargetInfo_t targetInfo )
	{
		m_TargetInfo = targetInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public		override	void	OnTargetChanged( TargetInfo_t targetInfo )
	{
		base.OnTargetChanged( targetInfo );

		// PathFinding
		NavGoto( m_TargetInfo.CurrentTarget.Transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public		override	void	OnTargetLost( TargetInfo_t targetInfo )
	{
		base.OnTargetLost( targetInfo );

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

		// Update targeting
		if ( m_TargetInfo.HasTarget == true )
		{
			if ( m_Brain.State != BrainState.ATTACKING )
			{
				m_Brain.ChangeState( BrainState.ATTACKING );
			}

			SetPoinToFace( m_TargetInfo.CurrentTarget.Transform.position );

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

		m_NavCanMoveAlongPath = false;
		m_NavAgent.speed = 0.0f;

		// Update PathFinding and movement along path
		if ( m_HasDestination && ( transform.position - m_PointToFace ).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance )
		{
			if ( m_TargetInfo.HasTarget == true )
			{
				CheckForNewReachPoint( m_TargetInfo.CurrentTarget.Transform.position );
			}

			if ( m_IsAllignedHeadToPoint )
			{
				m_NavCanMoveAlongPath = true;
				m_NavAgent.speed = m_MoveMaxSpeed;
			}
		}
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
			m_RespawnPoint.Respawn( this, 2f );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FaceToPoint ( Override )
	protected	override	void	FaceToPoint( float deltaTime )
	{
		Vector3 pointOnThisPlane	= Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_HeadTransform.position, m_PointToFace );
		Vector3 dirToPosition		= ( pointOnThisPlane - transform.position );
		Vector3 dirGunToPosition	= ( m_PointToFace - m_GunTransform.position );

		// set direction to player
		Vector3 vHeadForward			= Vector3.Scale( dirToPosition, m_ScaleVector );
		m_HeadTransform.forward			= Vector3.RotateTowards( m_HeadTransform.forward, vHeadForward, m_HeadRotationSpeed * deltaTime, 0.0f );

		m_IsAllignedBodyToPoint	= Vector3.Angle( transform.forward, vHeadForward ) < 7f;
		if ( m_IsAllignedBodyToPoint )
		{
			m_GunTransform.forward		= Vector3.RotateTowards( m_GunTransform.forward, dirGunToPosition, m_GunRotationSpeed * deltaTime, 0.0f );
		}

		m_IsAllignedGunToPoint			= Vector3.Angle( m_GunTransform.forward, dirGunToPosition ) < 7f;
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
		m_IsAllignedBodyToPoint	= false;
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
