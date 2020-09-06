
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
			GlobalManager.Configs.GetSection(this.m_SectionName, ref this.m_SectionRef );
			if (this.m_SectionRef == null )
			{
				print( "Cannot find cfg section for entity " + this.name );
				Destroy(this.gameObject );
				return;
			}

			this.m_Health				= this.m_SectionRef.AsFloat( "Health", 60.0f );

			if (this.m_Shield != null )
			{
				float shieldStatus	= this.m_SectionRef.AsFloat( "Shield", 0.0f );
				this.m_Shield.Setup( shieldStatus, EShieldContext.ENTITY );
			};

			this.m_DamageMax				= this.m_SectionRef.AsFloat( "DamageMax", 2.0f );
			this.m_DamageMin				= this.m_SectionRef.AsFloat( "DamageMin", 0.5f );
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

					// this allow to receive only trigger enter callback
//					Player.Instance.DisableCollisionsWith( o.Collider, bAlsoTriggerCollider: false );
				}
			};
			this.m_Pool = new GameObjectsPool<Bullet>( data );
		}
		this.m_Pool.SetActive( true );
		this.m_ShotTimer = 0f;


	}

	protected override void OnEnable()
	{
		base.OnEnable();

		this.m_Laser = this.GetComponentInChildren<Laser>();
		if (this.m_Laser != null )
		{
			this.m_Laser.LaserLength = this.Brain.FieldOfView.Distance;
//			m_Laser.LayerMaskToExclude = LayerMask.NameToLayer("Bullets");
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
		this.gameObject.SetActive( false );
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
			Vector3 pointToLookAt = this.m_LookData.PointToLookAt;
			if (this.m_TargetInfo.HasTarget == true ) // PREDICTION
			{
				// Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity
				pointToLookAt = Utils.Math.CalculateBulletPrediction
				(
					shooterPosition: this.m_GunTransform.position,
					shooterVelocity:	Vector3.zero,
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
		bullet.Shoot( position: this.m_FirePoint.position, direction: this.m_FirePoint.forward );

		this.m_FireAudioSource.Play();
	}
}
