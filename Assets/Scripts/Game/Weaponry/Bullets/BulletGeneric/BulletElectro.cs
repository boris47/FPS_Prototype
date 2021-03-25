
using UnityEngine;


public sealed class BulletElectro : BulletBallistic
{
	//////////////////////////////////////////////////////////////////////////
	protected override void OnCollisionDetailed(in Vector3 point, in Vector3 normal, in Collider otherCollider)
	{
		base.OnCollisionDetailed(point, normal, otherCollider);

		bool bIsAnEntity = Utils.Base.TrySearchComponent(otherCollider.gameObject, ESearchContext.LOCAL_AND_PARENTS, out Entity entity);
		bool bIsShield = Utils.Base.TrySearchComponent(otherCollider.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Shield shield);

		int nParticle = 3;

		EffectsManager.EEffecs effectToPlay;
		if (bIsShield)
		{
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
			nParticle = 15;
		}
		else
		// If is an entity and who and hitted entites are of different category
		if (bIsAnEntity && !(m_WhoRef is NonLiveEntity && entity is NonLiveEntity))
		{
			nParticle = 15;
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
			entity.EntityRigidBody.angularVelocity = entity.EntityRigidBody.velocity = Vector3.zero;
		}
		else
		{
			nParticle = 25;
			effectToPlay = EffectsManager.EEffecs.ELETTRO;
		}

		EffectsManager.Instance.PlayEffect(effectToPlay, point, normal, nParticle);
	}

}
