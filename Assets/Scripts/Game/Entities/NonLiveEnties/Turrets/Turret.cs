
using UnityEngine;


public abstract class Turret : NonLiveEntity {

	[SerializeField]
	protected	float	m_DamageMax	= 2f;

	[SerializeField]
	protected	float	m_DamageMin	= 0.5f;


	protected override void Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			GameManager.Configs.GetSection( m_SectionName = gameObject.name, ref m_SectionRef );
			if ( m_SectionRef == null )
			{
				Destroy( gameObject );
				return;
			}

			Health					= m_SectionRef.AsFloat( "Health", 60.0f );

			float shieldStatus		= m_SectionRef.AsFloat( "Shield", 0.0f );
			if ( m_Shield != null )
				( m_Shield as IShield ).Status = shieldStatus;

			m_DamageMax				= m_SectionRef.AsFloat( "DamageMax", 2.0f );
			m_DamageMin				= m_SectionRef.AsFloat( "DamageMin", 0.5f );

			m_EntityType			= ENTITY_TYPE.ROBOT;
		}

		// BULLETS POOL CREATION
		{
			GameObject go = GameObject.CreatePrimitive( PrimitiveType.Sphere );
			go.name = "TurretBlt";
			Rigidbody rb = go.AddComponent<Rigidbody>();
			Bullet bullet = go.AddComponent<Bullet>();
			bullet.WhoRef = this;
			bullet.DamageMax = m_DamageMax;
			bullet.DamageMin = m_DamageMin;
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
	/// <summary>
	/// Update forward direction and gun rotation
	/// </summary>
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

		m_AllignedToTarget		= Vector3.Angle( transform.forward, vBodyForward ) < 7f;
		if ( m_AllignedToTarget )
		{
			m_GunTransform.forward	=  Vector3.RotateTowards( m_GunTransform.forward, dirGunToTarget, gunRotationSpeed * Time.deltaTime, 0.0f );
		}


		m_AllignedGunToTarget	= Vector3.Angle( m_GunTransform.forward, dirGunToTarget ) < 7f;
		if ( m_AllignedGunToTarget == false )
			return;


		// SHOOTING
		m_ShotTimer -= Time.deltaTime;
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
