
using UnityEngine;
using System.Collections;

public abstract class Drone : NonLiveEntity {

	[SerializeField]
	private		GameObject		m_BulletGameObject	= null;

	[SerializeField]
	protected	float			m_DamageLongRangeMax		= 2f;

	[SerializeField]
	protected	float			m_DamageLongRangeMin		= 0.5f;

	[SerializeField]
	protected	float			m_DamageCloseRange			= 5f;

	[SerializeField]
	protected	float			m_CloseCombatRange			= 1.2f;

	[SerializeField]
	protected	float			m_CloseCombatDelay			= 1f;

	[SerializeField]
	protected	float			m_MoveMaxSpeed				= 3f;


	protected	Entity			m_Instance					= null;
	protected	float			m_CloseCombatDelayInternal	= 0f;



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
			GameObject	bulletGO		= m_BulletGameObject;
			m_Pool = new GameObjectsPool<Bullet>( ref bulletGO, 5, destroyModel : false, actionOnObject : ( Bullet o ) =>
			{
				o.SetActive( false );
				o.Setup( m_DamageLongRangeMin, m_DamageLongRangeMax, this, null, false );
				Physics.IgnoreCollision( o.Collider, m_PhysicCollider, ignore : true );
				if ( m_Shield != null )
					Physics.IgnoreCollision( o.Collider, m_Shield.Collider, ignore : true );
			} );
			m_Pool.ContainerName = name + "BulletPool";
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

		Bullet bullet = m_Pool.GetComponent();
		bullet.transform.position = m_FirePoint.position;
		bullet.SetVelocity( bullet.transform.up = m_GunTransform.forward );
		bullet.SetActive( true );
		
		m_FireAudioSource.Play();
	}

}
