
using UnityEngine;

public abstract partial class Drone {

	protected	abstract	class Behaviour_Base : AIBehaviour {
	
		protected				Drone			m_ThisEntity		= null;

		public override void Setup( Entity ThisEntity )
		{
			m_ThisEntity = ThisEntity as Drone;
		}

		public		override	void			Enable()
		{
			m_ThisEntity.Behaviour_OnSave			= OnSave;
			m_ThisEntity.Behaviour_OnLoad			= OnLoad;

			m_ThisEntity.Behaviour_OnHitWithBullet	= OnHit;
			m_ThisEntity.Behaviour_OnHitWithDetails	= OnHit;

			m_ThisEntity.Behaviour_OnThink			= OnThink;
			m_ThisEntity.Behaviour_OnPhysicFrame	= OnPhysicFrame;
			m_ThisEntity.Behaviour_OnFrame			= OnFrame;

			m_ThisEntity.Behaviour_OnTargetAcquired	= OnTargetAcquired;
			m_ThisEntity.Behaviour_OnTargetUpdate	= OnTargetUpdate;
			m_ThisEntity.Behaviour_OnTargetChange	= OnTargetChange;
			m_ThisEntity.Behaviour_OnTargetLost		= OnTargetLost;

			m_ThisEntity.Behaviour_OnDestinationReached = OnDestinationReached;

			m_ThisEntity.OnKilled					= OnKilled;

		}

		public		override	void			Disable()
		{
			m_ThisEntity.Behaviour_OnSave			= null;
			m_ThisEntity.Behaviour_OnLoad			= null;

			m_ThisEntity.Behaviour_OnHitWithBullet	= null;
			m_ThisEntity.Behaviour_OnHitWithDetails	= null;

			m_ThisEntity.Behaviour_OnThink			= null;
			m_ThisEntity.Behaviour_OnPhysicFrame	= null;
			m_ThisEntity.Behaviour_OnFrame			= null;

			m_ThisEntity.Behaviour_OnTargetAcquired	= null;
			m_ThisEntity.Behaviour_OnTargetUpdate	= null;
			m_ThisEntity.Behaviour_OnTargetChange	= null;
			m_ThisEntity.Behaviour_OnTargetLost		= null;

			m_ThisEntity.Behaviour_OnDestinationReached = null;

			m_ThisEntity.OnKilled					= null;

		}

		public		abstract	StreamUnit		OnSave( StreamData streamData );

		public		abstract	StreamUnit		OnLoad( StreamData streamData );

		public		abstract	void			OnHit( IBullet bullet );

		public		abstract	void			OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false );

		public		abstract	void			OnThink();

		public		abstract	void			OnPhysicFrame( float FixedDeltaTime );

		public		abstract	void			OnFrame( float DeltaTime );

		public		abstract	void			OnPauseSet( bool isPaused );

		public		abstract	void			OnTargetAcquired( TargetInfo_t targetInfo );

		public		abstract	void			OnTargetUpdate( TargetInfo_t targetInfo );

		public		abstract	void			OnTargetChange( TargetInfo_t targetInfo );

		public		abstract	void			OnTargetLost( TargetInfo_t targetInfo );

		public		abstract	void			OnDestinationReached( Vector3 Destination );

		public		abstract	void			OnKilled();
	}

	protected	partial		class Drone_AI_Beaviour_Evasive : Behaviour_Base {}

	protected	partial		class Drone_AI_Beaviour_Normal : Behaviour_Base {}

	protected	partial		class Drone_AI_Beaviour_Alarmed : Behaviour_Base {}

	protected	partial		class Drone_AI_Beaviour_Seeker : Behaviour_Base {}

	protected	partial		class Drone_AI_Beaviour_Attacker : Behaviour_Base {}
}

public abstract partial class Drone : NonLiveEntity, IRespawn {

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

	[SerializeField, ReadOnly]
	protected	int				m_PoolSize					= 5;

	//////////////////////////////////////////////////////////////////////////

	protected	override	void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
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
					o.Setup
					(
						canPenetrate: false,
						whoRef: this,
						weaponRef: null,
						damageMin: m_DamageLongRangeMax,
						damageMax: m_DamageLongRangeMin
					);
					this.SetCollisionStateWith( o.Collider, state: false );

					// this allow to receive only trigger enter callback
					Player.Instance.DisableCollisionsWith( o.Collider );
				}
			);
		}
		m_Pool.SetActive( true );
		m_ShotTimer = 0f;


		// AI BEHAVIOURS
		{	m_Behaviours = new AIBehaviour[ 5 ];
			SetBehaviour( BrainState.EVASIVE,	m_SectionRef.AsString( "BehaviourEvasive"	), false, this );
			SetBehaviour( BrainState.NORMAL,	m_SectionRef.AsString( "BehaviourNormal"	), true , this );
			SetBehaviour( BrainState.ALARMED,	m_SectionRef.AsString( "BehaviourAlarmed"	), false, this );
			SetBehaviour( BrainState.SEEKER,	m_SectionRef.AsString( "BehaviourSeeker"	), false, this );
			SetBehaviour( BrainState.ATTACKER,	m_SectionRef.AsString( "BehaviourAttacker"	), false, this );

			ChangeState( BrainState.NORMAL );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////

	public		override	void	OnHit( IBullet bullet )
	{
		Behaviour_OnHitWithBullet( bullet );
	}
	

	//////////////////////////////////////////////////////////////////////////

	public		override	void	OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		Behaviour_OnHitWithDetails( startPosition, whoRef, damage, canPenetrate );
	}


	//////////////////////////////////////////////////////////////////////////
	
	protected		override	void	OnDestinationReached( Vector3 Destionation )
	{
		base.OnDestinationReached( Destionation );
	}

	
	//////////////////////////////////////////////////////////////////////////

	protected		override	void	OnTargetAquired( TargetInfo_t targetInfo )
	{
		base.OnTargetAquired( targetInfo );
	}


	//////////////////////////////////////////////////////////////////////////

	protected		override	void	OnTargetUpdate( TargetInfo_t targetInfo )
	{
		base.OnTargetUpdate( targetInfo );
	}


	//////////////////////////////////////////////////////////////////////////

	protected		override	void	OnTargetChanged( TargetInfo_t targetInfo )
	{
		base.OnTargetChanged( targetInfo );
	}


	//////////////////////////////////////////////////////////////////////////

	protected		override	void	OnTargetLost( TargetInfo_t targetInfo )
	{
		base.OnTargetLost( targetInfo );
	
	}

	
	//////////////////////////////////////////////////////////////////////////

	protected	override	void	OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );
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

	protected		void	FaceToPoint( float DeltaTime )
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
	
	protected		void	FireLongRange( float deltaTime )
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

		

		Brain_OnReset();
		if ( m_Shield != null )
			( m_Shield as IShield ).OnReset();
	}

}

