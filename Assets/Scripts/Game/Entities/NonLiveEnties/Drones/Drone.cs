
using UnityEngine;
using System.Collections;

public abstract class Drone : NonLiveEntity {

	[SerializeField]
	protected	float	m_DamageLongRangeMax		= 2f;

	[SerializeField]
	protected	float	m_DamageLongRangeMin		= 0.5f;

	[SerializeField]
	protected	float	m_DamageCloseRange			= 5f;

	[SerializeField]
	protected	float	m_CloseCombatRange			= 1.2f;

	[SerializeField]
	protected	float	m_CloseCombatDelay			= 1f;

	[SerializeField]
	protected	float	m_MoveMaxSpeed				= 3f;


	protected	Entity	m_Instance					= null;
	protected	float	m_CloseCombatDelayInternal	= 0f;



	protected override void Awake()
	{
		base.Awake();

		m_Instance = this;

		// LOAD CONFIGURATION
		{
			GameManager.Configs.GetSection( m_SectionName = gameObject.name, ref m_SectionRef );
			if ( m_SectionRef == null )
			{
				print( name + " cannot find his section !!" );
				Destroy( gameObject );
				return;
			}

			Health					= m_SectionRef.AsFloat( "Health",				30.0f );
			float shieldStatus		= m_SectionRef.AsFloat( "Shield",				60.0f );
			if ( m_Shield != null )
				( m_Shield as IShield ).Status = shieldStatus;

			m_MoveMaxSpeed			= m_SectionRef.AsFloat( "MoveMaxSpeed",			1.0f );

			m_DamageLongRangeMax	= m_SectionRef.AsFloat( "DamageLongRangeMax",	2.0f );
			m_DamageLongRangeMin	= m_SectionRef.AsFloat( "DamageLongRangeMin",	0.5f );
			m_DamageCloseRange		= m_SectionRef.AsFloat( "DamageCloseRange",		5.0f );

			m_CloseCombatRange		= m_SectionRef.AsFloat( "CloseCombatRange",		1.2f );
			m_CloseCombatDelay		= m_SectionRef.AsFloat( "CloseCombatDelay",		1.2f );

			m_EntityType			= ENTITY_TYPE.ROBOT;
		}

		// BULLETS POOL CREATION
		{
			GameObject go = GameObject.CreatePrimitive( PrimitiveType.Sphere );
			go.name = "DroneBlt";
			Rigidbody rb = go.AddComponent<Rigidbody>();
			Bullet bullet = go.AddComponent<Bullet>();
			bullet.WhoRef = this;
			bullet.DamageMax = m_DamageLongRangeMax;
			bullet.DamageMin = m_DamageLongRangeMin;
			rb.useGravity = false;
			rb.velocity = Vector3.zero;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rb.detectCollisions = false;
			go.transform.localScale = Vector3.one * 0.1f;

			m_Pool = new GameObjectsPool( ref go, 10, true );
		}
	}

	public float bodyRotationSpeed = 5f;
	public float gunRotationSpeed = 5f;
	protected override void Update()
	{
		base.Update();

		if ( m_CurrentTarget == null )
			return;

		Vector3 dirToTarget		= ( m_CurrentTarget.transform.position - transform.position );
		Vector3 dirGunToTarget	= ( m_CurrentTarget.transform.position - m_GunTransform.position );
		

		// set direction to player
		Vector3 vBodyForward	= Vector3.Scale( dirToTarget,		new Vector3( 1.0f, 0.0f, 1.0f ) );
		transform.forward		= Vector3.RotateTowards( transform.forward, vBodyForward, bodyRotationSpeed * Time.deltaTime, 0.0f );

		m_CloseCombatDelayInternal -= Time.deltaTime;

		m_AllignedToTarget		= Vector3.Angle( transform.forward, vBodyForward ) < 7f;
		if ( m_AllignedToTarget )
		{
			m_GunTransform.forward	=  Vector3.RotateTowards( m_GunTransform.forward, dirGunToTarget, gunRotationSpeed * Time.deltaTime, 0.0f );

			// if near enough for close combat attack, attack target
			if ( dirToTarget.sqrMagnitude < m_CloseCombatRange * m_CloseCombatRange )
			{
				if ( m_CloseCombatDelayInternal < 0f )
				{
					m_CloseCombatDelayInternal = m_CloseCombatDelay;
					m_CurrentTarget.OnHit( ref m_Instance, m_DamageCloseRange );

					// TODO: add a attack/hit effect
				}
				return;
			}

			// Move toward the current target
			transform.position += dirToTarget.normalized * m_MoveMaxSpeed * Time.deltaTime;
		}


		m_ShotTimer -= Time.deltaTime;
		m_AllignedGunToTarget	= Vector3.Angle( m_GunTransform.forward, dirGunToTarget ) < 7f;
		if ( m_AllignedGunToTarget == false )
			return;

		// SHOOTING
		if ( m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;

		Bullet bullet = m_Pool.Get<Bullet>();
		bullet.enabled = true;
		bullet.transform.position = m_FirePoint.position;
		bullet.MaxLifeTime = 5f;
		bullet.SetVelocity( m_GunTransform.forward * m_BulletSpeed );
		bullet.SetActive( true );
		
		m_FireAudioSource.Play();
	}

}
