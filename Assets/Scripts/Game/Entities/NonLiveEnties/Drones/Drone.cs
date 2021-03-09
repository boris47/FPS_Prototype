
using UnityEngine;

public abstract class Drone : NonLiveEntity {

	[Header("Drone Properties")]

	[SerializeField]
	protected			float						m_MoveMaxSpeed							= 3f;

	protected	override ERotationsMode				m_LookTargetMode						=> ERotationsMode.WITH_BODY;
	protected 	override EEntityType				m_EntityType							=> EEntityType.ROBOT;
	protected	override EntityComponentContainer[] m_RequiredComponents					=> new EntityComponentContainer[]
	{
		new EntityComponentContainer_Memory<Memory_Common>(),
		new EntityComponentContainer_Motion<Motion_Common>(),
		new EntityComponentContainer_Behaviours<Behaviours_Common>(),
		new EntityComponentContainer_Navigation<Navigation_Common>(),
	};

	/*	[SerializeField]
		private		Bullet			m_Bullet					= null;

		[SerializeField]
		protected	float			m_ShotDelay					= 0.7f;

		[SerializeField]
		protected	float			m_DamageLongRangeMax		= 2f;

		[SerializeField]
		protected	float			m_DamageLongRangeMin		= 0.5f;

		[SerializeField, ReadOnly]
		protected	uint			m_PoolSize					= 5;
	*/
	//////////////////////////////////////////////////////////////////////////

	protected	override	void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			m_Health				= m_SectionRef.AsFloat( "Health",				30.0f );
			
			if (m_Shield != null )
			{
				float shieldStatus	= m_SectionRef.AsFloat( "Shield",				60.0f );
				m_Shield.Setup( shieldStatus, EShieldContext.ENTITY );
			}

			m_MoveMaxSpeed			= m_SectionRef.AsFloat( "MoveMaxSpeed",			1.0f );

	//		m_DamageLongRangeMax	= m_SectionRef.AsFloat( "DamageLongRangeMax",	2.0f );
	//		m_DamageLongRangeMin	= m_SectionRef.AsFloat( "DamageLongRangeMin",	0.5f );

	//		m_PoolSize				= m_SectionRef.AsUInt( "PoolSize", m_PoolSize );
		}

		// BULLETS POOL CREATION
	/*	if (m_Pool == null )		// check for respawn
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
					o.OverrideDamages(m_DamageLongRangeMin, m_DamageLongRangeMax );

					// this allow to receive only trigger enter callback
					//		Player.Instance.DisableCollisionsWith( o.Collider, bAlsoTriggerCollider: false );
				}
			};
			m_Pool = new GameObjectsPool<Bullet>( data );
		}
		m_Pool.SetActive( true );
		m_ShotTimer = 0f;
	*/
		Navigation.MaxAgentSpeed = m_MoveMaxSpeed;
	}

	
	//////////////////////////////////////////////////////////////////////////

	protected	override	void	OnKill()
	{
		base.OnKill();
	//	m_Pool.SetActive( false );
		gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	public override void LookAt(in Vector3 worldpoint, in float bodyRotationSpeed, in float headRotationSpeed, in Vector2? clampsHoriz, in Vector2? clampsVert, out bool isBodyAlligned, out bool isHeadAlligned)
	{
		isBodyAlligned = default;
		isHeadAlligned = default;
	}


	//////////////////////////////////////////////////////////////////////////

	//	protected	override	void	UpdateHeadRotation()
	//	{
	//		base.UpdateHeadRotation();
	// ORIENTATION
	// BODY
	//		{
	// Nothing, rotation not allowed here
	//		}

	// GUN
	/*		{
				Vector3 pointToLookAt = m_LookData.PointToLookAt;
				if (m_TargetInfo.HasTarget == true ) // PREDICTION
				{
					// Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity
					pointToLookAt = Utils.Math.CalculateBulletPrediction
					(
						shooterPosition: m_GunTransform.position,
						shooterVelocity: m_NavAgent.velocity,
						shotSpeed: m_Pool.TryPeekComponentAs<IBullet>().Velocity,
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
			}
	*///	}


	//////////////////////////////////////////////////////////////////////////
	/*	
	public	override		void	FireLongRange()
	{
		if (m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;

		IBullet bullet = m_Pool.GetNextComponent();
		
		Vector3 direction = m_FirePoint.forward;
		{
			direction.x += Random.Range( -m_FireDispersion, m_FireDispersion );
			direction.y += Random.Range( -m_FireDispersion, m_FireDispersion );
			direction.z += Random.Range( -m_FireDispersion, m_FireDispersion );
		}
		direction.Normalize();
		bullet.Shoot( position: m_FirePoint.position, direction: direction, velocity: null );

		m_FireAudioSource.Play();
	}
	*/

}

