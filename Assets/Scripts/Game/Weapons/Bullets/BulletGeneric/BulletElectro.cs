using UnityEngine;
using System.Collections;


public class BulletElectro : BulletGeneric {


	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{
		bool bIsBullet = collision.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		IEntity entity = null;
		IShield shield = null;
		bool bIsAnEntity = Utils.Base.SearchComponent( collision.gameObject, ref entity, SearchContext.LOCAL    );
		bool bIsShield   = Utils.Base.SearchComponent( collision.gameObject, ref shield, SearchContext.CHILDREN );

		Vector3 position  = collision.contacts[0].point;
		Vector3 direction = collision.contacts[0].normal;

		int nParticle = 3;

		EffectType effectToPlay;
		if ( bIsShield )
		{
			effectToPlay = EffectType.ELETTRO;
			nParticle = 15;
		}
		else
		// If is an entity and who and hitted entites are of different category
		if ( bIsAnEntity == true && ( ( m_WhoRef is NonLiveEntity && entity is NonLiveEntity ) == false ) )
		{
			nParticle = 15;
			effectToPlay = EffectType.ELETTRO;
			entity.RigidBody.angularVelocity = entity.RigidBody.velocity = Vector3.zero;
		}
		else
		{
			nParticle = 25;
			effectToPlay = EffectType.ELETTRO;
		}

		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, nParticle );

		this.SetActive( false );
	}

}
