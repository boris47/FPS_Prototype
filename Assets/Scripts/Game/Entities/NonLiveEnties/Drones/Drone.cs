
using UnityEngine;
using System.Collections;

public abstract class Drone : NonLiveEntity {
	
	[SerializeField]
	protected	float	m_DamageLongRange	= 2f;

	[SerializeField]
	protected	float	m_DamageCloseRange	= 5f;

	[SerializeField]
	private		float		m_MaxSpeed = 3f;


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

			Health		= m_SectionRef.AsFloat( "Health", 30.0f );

			float shieldStatus		= m_SectionRef.AsFloat( "Shield", 0.0f );
			if ( m_Shield != null )
				( m_Shield as IShield ).Status = shieldStatus;
		}

		// BULLETS POOL CREATION
		{
			GameObject go = GameObject.CreatePrimitive( PrimitiveType.Sphere );
			go.name = "DroneBlt";
			Rigidbody rb = go.AddComponent<Rigidbody>();
			rb.useGravity = false;
			rb.velocity = Vector3.zero;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rb.detectCollisions = false;
			go.transform.localScale = Vector3.one * 0.1f;

			m_Pool = new GameObjectsPool( ref go, 10, true );
		}
	}


	protected override void Update()
	{
		base.Update();


	}

}
