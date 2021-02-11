
using UnityEngine;


public sealed class BulletElectro : BulletBallistic {


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();
	}



	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionDetailed( in Vector3 point, in Vector3 normal, in Collider otherCollider )
	{
		bool bIsBullet = otherCollider.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		bool bIsAnEntity = Utils.Base.TrySearchComponent( otherCollider.gameObject, ESearchContext.LOCAL, out IEntity entity );
		bool bIsShield   = Utils.Base.TrySearchComponent( otherCollider.gameObject, ESearchContext.CHILDREN, out IShield shield );

		int nParticle = 3;

		EffectsManager.EEffecs effectToPlay;
		if ( bIsShield )
		{
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
			nParticle = 15;
		}
		else
		// If is an entity and who and hitted entites are of different category
		if ( bIsAnEntity == true && ( (m_WhoRef is NonLiveEntity && entity is NonLiveEntity ) == false ) )
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

		EffectsManager.Instance.PlayEffect( effectToPlay, point, normal, nParticle );

		SetActive( false );
	}

}
