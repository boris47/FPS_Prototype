
using UnityEngine;
using System.Collections;

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


	/// <summary>
	/// Update forward direction and gun rotation
	/// </summary>
	protected override void Update()
	{
		base.Update();

		if ( m_CurrentTarget == null )
			return;

		Vector3 directionToTarget = ( m_CurrentTarget.transform.position - transform.position );
			
		// set direction to player
		Vector3 vForward = Vector3.Scale( directionToTarget, new Vector3( 1.0f, 0.0f, 1.0f ) );
		transform.forward =  vForward;

		// Gun looks at valid target
		m_GunTransform.LookAt( m_CurrentTarget.transform.position );
	}

}
