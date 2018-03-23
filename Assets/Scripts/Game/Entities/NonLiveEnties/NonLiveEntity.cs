
using UnityEngine;
using System.Collections;

public abstract class NonLiveEntity : Entity {
	
	[SerializeField]
	protected		float				m_MaxDamage			= 7f;

	[SerializeField]
	protected		float				m_MinDamage			= 3f;

	[SerializeField]
	protected		float				m_ShotDelay			= 0.7f;

	[SerializeField]
	protected		float				m_BulletSpeed		= 10f;

//	[SerializeField]
	protected		AudioSource			m_FireAudioSource	= null;

	protected		Transform			m_GunTransform		= null;
	protected		Transform			m_FirePoint			= null;
	protected		Collider			m_Collider			= null;
	protected		GameObjectsPool		m_Pool				= null;
	protected		float				m_ShotTimer			= 0f;


	protected virtual void Awake()
	{
		m_Collider = GetComponent<Collider>();
		m_FireAudioSource = GetComponent<AudioSource>();

		m_GunTransform = transform.Find( "Gun" );

		m_FirePoint = m_GunTransform.GetChild( 0 );
	}


	protected virtual void OnCollisionEnter( Collision collision )
	{
		Bullet bullet = collision.gameObject.GetComponent<Bullet>();
		if ( bullet == null )
			return;

		if ( bullet.WhoRef is Player )
		{
			if ( bullet.IsCloseRange )
				OnHurt( null );
			else
				OnHit( null );
		}
	}

}
