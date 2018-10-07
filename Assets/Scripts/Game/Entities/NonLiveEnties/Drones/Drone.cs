
using UnityEngine;
using System.Collections;

public abstract class Drone : NonLiveEntity, IRespawn {

	[Header("Drone Properties")]

	[SerializeField]
	protected	float			m_MoveMaxSpeed				= 3f;

	[SerializeField]
	private		Bullet			m_Bullet					= null;

	[SerializeField]
	protected	float			m_ShotDelay					= 0.7f;

	[SerializeField]
	protected	float			m_DamageLongRangeMax		= 2f;

	[SerializeField]
	protected	float			m_DamageLongRangeMin		= 0.5f;

	protected	int				m_PoolSize					= 5;
	


	//////////////////////////////////////////////////////////////////////////

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
		}
		SetPoinToLookAt( bullet.StartPosition );

		// DAMAGE
		// Shield damage
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

	public		override	void	OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		print( name + " OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate )" );
	}


	//////////////////////////////////////////////////////////////////////////

	public		override	void	OnDestinationReached()
	{
		NavReset();

		print( "OnDestinationReached" );
		if ( m_TargetInfo.HasTarget == true )
		{
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_BodyTransform.position, m_TargetInfo.CurrentTarget.Transform.position );
			if ( ( transform.position - projectedPoint ).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance )
			{
				RequestMovement( projectedPoint );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////

	public		override	void	OnTargetAquired( TargetInfo_t targetInfo )
	{
		base.OnTargetAquired( targetInfo );

		// PathFinding
		Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_BodyTransform.position, m_TargetInfo.CurrentTarget.Transform.position );
		RequestMovement( projectedPoint );
	}


	//////////////////////////////////////////////////////////////////////////

	public		override	void	OnTargetUpdate( TargetInfo_t targetInfo )
	{
//		m_TargetInfo.Update( targetInfo );
	}


	//////////////////////////////////////////////////////////////////////////

	public		override	void	OnTargetChanged( TargetInfo_t targetInfo )
	{
		base.OnTargetChanged( targetInfo );

		// PathFinding
		Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_BodyTransform.position, m_TargetInfo.CurrentTarget.Transform.position );
		RequestMovement( projectedPoint );
	}


	//////////////////////////////////////////////////////////////////////////

	public		override	void	OnTargetLost( TargetInfo_t targetInfo )
	{
		// SEEKING MODE

		// TODO Set brain to SEKKER mode
		m_Brain.ChangeState( BrainState.SEEKER );

		// Destination
		{
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_BodyTransform.position, m_TargetInfo.CurrentTarget.Transform.position );
			RequestMovement( projectedPoint );
		}

		// Orientation
		{
			Vector3 newPointToLookAt = m_TargetInfo.CurrentTarget.Transform.position + m_TargetInfo.CurrentTarget.RigidBody.velocity.normalized;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_BodyTransform.position, newPointToLookAt );
			SetPoinToLookAt( projectedPoint );
		}

		base.OnTargetLost( targetInfo ); // m_TargetInfo.Reset();

		
	}


	//////////////////////////////////////////////////////////////////////////

	protected	override	void	OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );

		// Update internal timer
		m_ShotTimer -= deltaTime;

		// Update targeting
		if ( m_TargetInfo.HasTarget == true )
		{
			if ( m_Brain.State != BrainState.ATTACKER )
			{
				m_Brain.ChangeState( BrainState.ATTACKER );
			}

			SetPoinToLookAt( m_TargetInfo.CurrentTarget.Transform.position );

			// with a target, if gun alligned, fire
			if ( m_IsAllignedGunToPoint == true )
			{
				FireLongRange( deltaTime );
			}
		}

		// if has point to face, update entity orientation
		if ( m_HasLookAtObject )
		{
			FaceToPoint( deltaTime );   // m_PointToFace
		}


		m_NavCanMoveAlongPath = false;

		// Update PathFinding and movement along path
		if ( m_HasDestination && m_IsAllignedHeadToPoint )
		{
			float agentFinalSpeed = 0.0f;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_BodyTransform.position, m_PointToFace );
			bool IsNotUnderEngageDistance = ( transform.position - projectedPoint ).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance;
			if ( m_TargetInfo.HasTarget )
			{
				if ( IsNotUnderEngageDistance )
				{
					agentFinalSpeed = m_MoveMaxSpeed;
				}
				else
				{
					agentFinalSpeed = 0.0f;
				}
			}
			else
			{
				agentFinalSpeed = m_MoveMaxSpeed;
			}
			print(agentFinalSpeed);
			m_NavAgent.speed = agentFinalSpeed;
		}

		
	}


	//////////////////////////////////////////////////////////////////////////

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

			m_IsAllignedHeadToPoint = Vector3.Angle( m_HeadTransform.forward, dirToPosition ) < 12f;
			{
				float speed = m_HeadRotationSpeed * ( ( m_TargetInfo.HasTarget ) ? 3.0f : 1.0f );

				m_RotationToAllignTo.SetLookRotation( dirToPosition, m_BodyTransform.up );
				m_HeadTransform.rotation = Quaternion.RotateTowards( m_HeadTransform.rotation, m_RotationToAllignTo, speed * DeltaTime );
			}
		}

		// GUN
		{
			Vector3 pointToLookAt = m_PointToFace;
			if ( m_TargetInfo.HasTarget == true ) // PREDICTION
			{
				// Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity
				pointToLookAt = Utils.Math.CalculateBulletPrediction
				(
					shooterPosition:	m_GunTransform.position,
					shooterVelocity:	m_NavAgent.velocity,
					shotSpeed:			m_Pool.GetAsModel().Velocity,
					targetPosition:		m_TargetInfo.CurrentTarget.Transform.position,
					targetVelocity:		m_TargetInfo.CurrentTarget.RigidBody.velocity
				);
			}

			Vector3 dirToPosition = ( pointToLookAt - m_GunTransform.position );
			if ( m_IsAllignedHeadToPoint == true )
			{
				m_RotationToAllignTo.SetLookRotation( dirToPosition, m_BodyTransform.up );
				m_GunTransform.rotation = Quaternion.RotateTowards( m_GunTransform.rotation, m_RotationToAllignTo, m_GunRotationSpeed * DeltaTime );
			}
			m_IsAllignedGunToPoint = Vector3.Angle( m_GunTransform.forward, dirToPosition ) < 16f;
		}
	}
	

	//////////////////////////////////////////////////////////////////////////

	protected	override	void	FireLongRange( float deltaTime )
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
