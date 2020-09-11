
using UnityEngine;


public sealed class BulletElectro : BulletBallistic {


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter ( Override )
	protected override void OnTriggerEnter( Collider other )
	{
//		bool bIsBullet = other.transform.HasComponent<Bullet>();
//		if ( bIsBullet == true )
//			return;

		IEntity entity = null;
		IShield shield = null;
		bool bIsAnEntity = Utils.Base.SearchComponent( other.gameObject, ref entity, ESearchContext.LOCAL );
		bool bIsShield = Utils.Base.SearchComponent( other.gameObject, ref shield, ESearchContext.CHILDREN );

		int nParticle = 3;

		EffectsManager.EEffecs effectToPlay;
		if ( bIsShield )
		{
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
			nParticle = 15;
		}
		else
		// If is an entity and who and hitted entites are of different category
		if ( bIsAnEntity == true && ( (this.m_WhoRef is NonLiveEntity && entity is NonLiveEntity ) == false ) )
		{
			nParticle = 15;
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
		}
		else
		{
			nParticle = 25;
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
		}

		Vector3 position = other.ClosestPointOnBounds(this.transform.position );
		Vector3 direction = other.transform.position - position;
		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, nParticle );

		this.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{
		bool bIsBullet = collision.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		IEntity entity = null;
		IShield shield = null;
		bool bIsAnEntity = Utils.Base.SearchComponent( collision.gameObject, ref entity, ESearchContext.LOCAL    );
		bool bIsShield   = Utils.Base.SearchComponent( collision.gameObject, ref shield, ESearchContext.CHILDREN );

		Vector3 position  = collision.contacts[0].point;
		Vector3 direction = collision.contacts[0].normal;

		int nParticle = 3;

		EffectsManager.EEffecs effectToPlay;
		if ( bIsShield )
		{
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
			nParticle = 15;
		}
		else
		// If is an entity and who and hitted entites are of different category
		if ( bIsAnEntity == true && ( (this.m_WhoRef is NonLiveEntity && entity is NonLiveEntity ) == false ) )
		{
			nParticle = 15;
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
			entity.RigidBody.angularVelocity = entity.RigidBody.velocity = Vector3.zero;
		}
		else
		{
			nParticle = 25;
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
		}

		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, nParticle );

		this.SetActive( false );
	}

}
