
using UnityEngine;

public abstract class Turret : NonLiveEntity
{
/*	[SerializeField]
	private				Bullet						m_Bullet				= null;

	[SerializeField]
	protected			float						m_ShotDelay				= 0.7f;

	[SerializeField]
	protected			float						m_DamageMax				= 2f;

	[SerializeField]
	protected			float						m_DamageMin				= 0.5f;

	[SerializeField, ReadOnly]
	protected			uint						m_PoolSize				= 5;
	
	private				WPN_WeaponAttachment_LaserPointer m_Laser			= null;
*/

	protected	override ERotationsMode				m_LookTargetMode		=> ERotationsMode.HEAD_ONLY;
	protected 	override EEntityType				m_EntityType			=> EEntityType.ROBOT;
	protected	override EntityComponentContainer[] m_RequiredComponents	=> new EntityComponentContainer[]
	{
		new EntityComponentContainer_Memory<Memory_Common>(),
		new EntityComponentContainer_Behaviours<Behaviours_Common>(),
	};

	[Header("Turret Properties")]

	[SerializeField, ReadOnly]
	protected			float						m_CurrentAngle_X				= 0.0f;
	[SerializeField, ReadOnly]
	protected			float						m_CurrentAngle_Y				= 0.0f;


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

			if (m_Shield.IsNotNull())
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
	public override void LookAt(in Vector3 worldpoint, in float bodyRotationSpeed, in float headRotationSpeed, in Vector2? clampsHoriz, in Vector2? clampsVert, out bool isBodyAlligned, out bool isHeadAlligned)
	{
		Entity.GetRotationsToPoint(m_BodyTransform, m_HeadTransform, worldpoint, out float horizontalRotation, out float verticalRotation);

		// Horizontal rotation
		{
			float newAngle_Y = m_CurrentAngle_Y + horizontalRotation;
			if (clampsHoriz.HasValue)
			{
				float min = clampsHoriz.Value.x;
				float max = clampsHoriz.Value.y;
				newAngle_Y = Utils.Math.Clamp(newAngle_Y, min, max);
			}
			m_CurrentAngle_Y = Mathf.MoveTowards(m_CurrentAngle_Y, newAngle_Y, Time.deltaTime * bodyRotationSpeed);
			m_BodyTransform.localRotation = Quaternion.Euler(Vector3.up * m_CurrentAngle_Y);
		}
		
		// Vertical rotation
		{
			float newAngle_X = m_CurrentAngle_X - verticalRotation;
			if (clampsVert.HasValue)
			{
				float min = clampsVert.Value.x;
				float max = clampsVert.Value.y;
				newAngle_X = Utils.Math.Clamp(newAngle_X, min, max);
			}
			m_CurrentAngle_X = Mathf.MoveTowards(m_CurrentAngle_X, newAngle_X, Time.deltaTime * headRotationSpeed);
			m_HeadTransform.localRotation = Quaternion.Euler(Vector3.right * m_CurrentAngle_X);
		}

		isBodyAlligned = m_CurrentAngle_Y <= 4f;
		isHeadAlligned = m_CurrentAngle_X <= 4f;
	}


	//////////////////////////////////////////////////////////////////////////
	protected		override	void	OnKill()
	{
		base.OnKill();
	//	m_Pool.SetActive( false );
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
