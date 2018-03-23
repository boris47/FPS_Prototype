
using UnityEngine;
using System.Collections;

public abstract class Turret : NonLiveEntity {

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

			Health		= m_SectionRef.AsFloat( "Health", 60.0f );
			m_Shield	= m_SectionRef.AsFloat( "Shield", 0.0f );
		}

		// BULLETS POOL CREATION
		{
			GameObject go = GameObject.CreatePrimitive( PrimitiveType.Sphere );
			go.name = "TurretBlt";
			Rigidbody rb = go.AddComponent<Rigidbody>();
			Bullet bullet = go.AddComponent<Bullet>();
			bullet.WhoRef = this;
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
	protected virtual void Update()
	{
		Vector3 directionToPlayer = ( Player.Instance.transform.position - transform.position );
			
		// set direction to player
		Vector3 vForward = Vector3.Scale( directionToPlayer, new Vector3( 1.0f, 0.0f, 1.0f ) );
		transform.forward =  vForward;

		m_GunTransform.LookAt( Player.Instance.transform.position );
	}

}
