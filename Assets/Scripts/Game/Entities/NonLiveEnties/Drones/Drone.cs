
using UnityEngine;

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

	[SerializeField, ReadOnly]
	protected	int				m_PoolSize					= 5;

	//////////////////////////////////////////////////////////////////////////

	protected	override	void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			this.m_Health				= this.m_SectionRef.AsFloat( "Health",				30.0f );
			
			if (this.m_Shield != null )
			{
				float shieldStatus	= this.m_SectionRef.AsFloat( "Shield",				60.0f );
				this.m_Shield.Setup( shieldStatus, EShieldContext.ENTITY );
			}

			this.m_MoveMaxSpeed			= this.m_SectionRef.AsFloat( "MoveMaxSpeed",			1.0f );

			this.m_DamageLongRangeMax	= this.m_SectionRef.AsFloat( "DamageLongRangeMax",	2.0f );
			this.m_DamageLongRangeMin	= this.m_SectionRef.AsFloat( "DamageLongRangeMin",	0.5f );

			this.m_PoolSize				= this.m_SectionRef.AsInt( "PoolSize", this.m_PoolSize );

			this.m_EntityType			= EEntityType.ROBOT;
		}

		// BULLETS POOL CREATION
		if (this.m_Pool == null )		// check for respawn
		{
			GameObject	bulletGO		= this.m_Bullet.gameObject;
			GameObjectsPoolConstructorData<Bullet> data = new GameObjectsPoolConstructorData<Bullet>()
			{
				Model					= bulletGO,
				Size					= ( uint )this.m_PoolSize,
				ContainerName			= this.name + "BulletPool",
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
					o.OverrideDamages(this.m_DamageLongRangeMin, this.m_DamageLongRangeMax );

					// this allow to receive only trigger enter callback
					//		Player.Instance.DisableCollisionsWith( o.Collider, bAlsoTriggerCollider: false );
				}
			};
			this.m_Pool = new GameObjectsPool<Bullet>( data );
		}
		this.m_Pool.SetActive( true );
		this.m_ShotTimer = 0f;
		this.m_MaxAgentSpeed = this.m_MoveMaxSpeed;
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

	protected	override	void	OnKill()
	{
		base.OnKill();
		//		m_Pool.SetActive( false );
		this.gameObject.SetActive( false );
	}
	

	//////////////////////////////////////////////////////////////////////////

	protected	override	void	UpdateHeadRotation()
	{
		base.UpdateHeadRotation();
		// ORIENTATION
		// BODY
		{
			// Nothing, rotation not allowed here
		}

		// GUN
		{
			Vector3 pointToLookAt = this.m_LookData.PointToLookAt;
			if (this.m_TargetInfo.HasTarget == true ) // PREDICTION
			{
				// Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity
				pointToLookAt = Utils.Math.CalculateBulletPrediction
				(
					shooterPosition: this.m_GunTransform.position,
					shooterVelocity: this.m_NavAgent.velocity,
					shotSpeed: this.m_Pool.PeekComponent<IBullet>().Velocity,
					targetPosition: this.m_TargetInfo.CurrentTarget.AsEntity.transform.position,
					targetVelocity: this.m_TargetInfo.CurrentTarget.RigidBody.velocity
				);
			}

			Vector3 dirToPosition = ( pointToLookAt - this.m_GunTransform.position );
			if (this.m_IsAllignedHeadToPoint == true )
			{
				this.m_RotationToAllignTo.SetLookRotation( dirToPosition, this.m_BodyTransform.up );
				this.m_GunTransform.rotation = Quaternion.RotateTowards(this.m_GunTransform.rotation, this.m_RotationToAllignTo, this.m_GunRotationSpeed * Time.deltaTime );
			}
			this.m_IsAllignedGunToPoint = Vector3.Angle(this.m_GunTransform.forward, dirToPosition ) < 16f;
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	
	public	override		void	FireLongRange()
	{
		if (this.m_ShotTimer > 0 )
				return;

		this.m_ShotTimer = this.m_ShotDelay;

		IBullet bullet = this.m_Pool.GetNextComponent();
		
		Vector3 direction = this.m_FirePoint.forward;
		{
			direction.x += Random.Range( -this.m_FireDispersion, this.m_FireDispersion );
			direction.y += Random.Range( -this.m_FireDispersion, this.m_FireDispersion );
			direction.z += Random.Range( -this.m_FireDispersion, this.m_FireDispersion );
		}
		direction.Normalize();
		bullet.Shoot( position: this.m_FirePoint.position, direction: direction );

		this.m_FireAudioSource.Play();
	}
	
	
	//////////////////////////////////////////////////////////////////////////

	void IRespawn.OnRespawn()
	{
		this.transform.position = this.m_RespawnPoint.transform.position;
		this.transform.rotation = this.m_RespawnPoint.transform.rotation;

		this.gameObject.SetActive( true );

		// Entity
		this.m_IsActive						= true;
		this.m_TargetInfo					= new TargetInfo();
		//		m_NavHasDestination				= false;
		//		m_HasFaceTarget					= false;
		//		m_Destination					= Vector3.zero;
		//		m_PointToFace					= Vector3.zero;
		this.m_IsAllignedBodyToPoint	= false;
		//		m_DistanceToTravel				= 0f;

		// NonLiveEntity
		this.m_ShotTimer						= 0f;
		this.m_IsAllignedGunToPoint			= false;

		// Reinitialize properties
		this.Awake();



		this.Brain_OnReset();

		if (this.m_Shield != null )
		{
			this.m_Shield.OnReset();
		}
	}

}

