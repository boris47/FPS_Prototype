using UnityEngine;
using System.Collections;


public class BulletElettro : GenericBullet {


	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{
		Bullet bullet = collision.gameObject.GetComponent<Bullet>();
		if ( bullet != null )
			return;

		IEntity entity = collision.gameObject.GetComponent<IEntity>();
		IShield shield = collision.gameObject.GetComponent<Shield>();
		
		if ( ( entity != null || shield != null ) && ( m_WhoRef is NonLiveEntity && entity is NonLiveEntity ) == false )
		{
			EffectManager.Instance.PlayEntityOnHit( collision.contacts[0].point, collision.contacts[0].normal );

			Transform effectPivot = ( entity.EffectsPivot != null ) ?  entity.EffectsPivot : collision.transform;
			EffectManager.Instance.PlayElettroEffect( effectPivot.position, collision.contacts[0].normal );
		}
		else
		{
			EffectManager.Instance.PlayAmbientOnHit( collision.contacts[0].point, collision.contacts[0].normal );
		}

		if ( shield != null )
		{
			float damage = UnityEngine.Random.Range( m_DamageMin, m_DamageMax );
			shield.OnHit( m_StartPosition, m_WhoRef, m_Weapon, damage, m_CanPenetrate );
		}
		else
		if ( entity != null )
		{
			Rigidbody erg = entity.RigidBody;
			erg.angularVelocity = erg.velocity = Vector3.zero;
			entity.OnHit( m_Instance );
		}

		this.SetActive( false );
	}

}
