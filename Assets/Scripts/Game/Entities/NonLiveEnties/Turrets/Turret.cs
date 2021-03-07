
using System;
using UnityEngine;


public abstract class Turret : NonLiveEntity {

//	[Header("Turret Properties")]

/*	[SerializeField]
	private		Bullet			m_Bullet					= null;

	[SerializeField]
	protected	float			m_ShotDelay					= 0.7f;

	[SerializeField]
	protected	float			m_DamageMax					= 2f;

	[SerializeField]
	protected	float			m_DamageMin					= 0.5f;

	[SerializeField, ReadOnly]
	protected	uint			m_PoolSize					= 5;

	private		WPN_WeaponAttachment_LaserPointer			m_Laser						= null;
*/

	protected 	override EEntityType				m_EntityType			=> EEntityType.ROBOT;

	protected	override EntityComponentContainer[] m_RequiredComponents	=> new EntityComponentContainer[]
	{
		new EntityComponentContainer_Memory<Memory_Common>(),
		new EntityComponentContainer_Behaviours<Behaviours_Common>(),
	};

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			if (!GlobalManager.Configs.TryGetSection(m_SectionName, out m_SectionRef))
			{
				print( "Cannot find cfg section for entity " + name );
				Destroy(gameObject );
				return;
			}

			m_Health				= m_SectionRef.AsFloat( "Health", 60.0f );

			if (m_Shield != null )
			{
				float shieldStatus	= m_SectionRef.AsFloat( "Shield", 0.0f );
				m_Shield.Setup( shieldStatus, EShieldContext.ENTITY );
			};
/*
			m_DamageMax				= m_SectionRef.AsFloat( "DamageMax", 2.0f );
			m_DamageMin				= m_SectionRef.AsFloat( "DamageMin", 0.5f );
			m_PoolSize				= m_SectionRef.AsUInt( "PoolSize", m_PoolSize );

			m_EntityType			= EEntityType.ROBOT;
		}

		// BULLETS POOL CREATION
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
//					Player.Instance.DisableCollisionsWith( o.Collider, bAlsoTriggerCollider: false );
				}
			};
			m_Pool = new GameObjectsPool<Bullet>( data );
*/		}
//		m_Pool.SetActive( true );
//		m_ShotTimer = 0f;
	}

	//////////////////////////////////////////////////////////////////////////

	protected		override	void	OnKill()
	{
		base.OnKill();
		//		m_Pool.SetActive( false );
		gameObject.SetActive( false );
	}
	
	//////////////////////////////////////////////////////////////////////////
//	protected virtual		void	UpdateHeadRotation()
//	{
		// ORIENTATION
		// BODY
//		{
			// Nothing, rotation not allowed here
//		}

		// GUN
//		{
	/*		Vector3 pointToLookAt = m_LookData.PointToLookAt;
			if (m_TargetInfo.HasTarget == true ) // PREDICTION
			{
				// Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity
				pointToLookAt = Utils.Math.CalculateBulletPrediction
				(
					shooterPosition: m_GunTransform.position,
					shooterVelocity:	Vector3.zero,
					shotSpeed: 0, // TODO handle this m_Pool.TryPeekComponentAs<IBullet>().Velocity,
					targetPosition: m_TargetInfo.CurrentTarget.transform.position,
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
//		}
//	}


	//////////////////////////////////////////////////////////////////////////
	/*
	public	override		void	FireLongRange()
	{
		if (m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;
		
		IBullet bullet = m_Pool.GetNextComponent();
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward, velocity: null );

		m_FireAudioSource.Play();
	}
	*/
}
