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
			EffectManager.Instance.PlayEffect( EffectType.ENTITY_ON_HIT, collision.contacts[0].point, collision.contacts[0].normal, 3 );

			Transform effectPivot = ( entity.EffectsPivot != null ) ?  entity.EffectsPivot : collision.transform;
			EffectManager.Instance.PlayEffect( EffectType.ELETTRO, effectPivot.position, collision.contacts[0].normal, 3 );
		}
		else
		{
			EffectManager.Instance.PlayEffect( EffectType.AMBIENT_ON_HIT, collision.contacts[0].point, collision.contacts[0].normal, 3 );
		}

/*		if ( shield != null )
		{
			float damage = UnityEngine.Random.Range( m_DamageMin, m_DamageMax );
			shield.OnHit( m_StartPosition, m_WhoRef, m_Weapon, damage, m_CanPenetrate );
		}
		else
*/		if ( entity != null )
		{
			Rigidbody erg = entity.RigidBody;
			erg.angularVelocity = erg.velocity = Vector3.zero;
//			entity.OnHit( m_Instance );

			float damage = UnityEngine.Random.Range( m_DamageMin, m_DamageMax );
			entity.OnHit( m_StartPosition, m_WhoRef, damage, m_CanPenetrate );
		}

		this.SetActive( false );
	}

}
