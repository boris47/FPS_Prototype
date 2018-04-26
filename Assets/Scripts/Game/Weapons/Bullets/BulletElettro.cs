using UnityEngine;
using System.Collections;


public class BulletElettro : GenericBullet {

	private		ParticleSystem			m_PS			= null;



	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{
		Entity entity = collision.gameObject.GetComponent<Entity>();
		Shield shield = collision.gameObject.GetComponent<Shield>();

		
		if ( ( entity != null || shield != null ) && ( m_WhoRef is NonLiveEntity && entity is NonLiveEntity ) == false )
		{
			EffectManager.Instance.PlayEntityOnHit( collision.contacts[0].point, collision.contacts[0].normal );
			m_PS = EffectManager.Instance.PlayElettroHit( collision.transform );
		}
		else
		{
			EffectManager.Instance.PlayAmbientOnHit( collision.contacts[0].point, collision.contacts[0].normal );
		}

		if ( shield != null )
			shield.OnHit( ref m_Instance );
		else
		if ( entity != null )
		{
			Rigidbody erg = ( entity as IEntity ).RigidBody;
			erg.angularVelocity = erg.velocity = Vector3.zero;
			entity.OnHit( ref m_Instance );
		}

		this.SetActive( false );
	}

}
