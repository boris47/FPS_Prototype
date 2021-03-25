
using UnityEngine;

public sealed class BulletRocket : BulletExplosive
{

	//////////////////////////////////////////////////////////////////////////
	protected override void OnEndTravel()
	{
		OnExplosion();
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void OnCollisionDetailed( in Vector3 point, in Vector3 normal, in Collider otherCollider )
	{
		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;
		if (Utils.Base.TrySearchComponent(otherCollider.gameObject, ESearchContext.LOCAL, out Entity entity))
		{
			entity.OnHittedDetails(m_StartPosition, m_WhoRef, EDamageType.EXPLOSIVE, 0, false);
		}
		else if (Utils.Base.TrySearchComponent(otherCollider.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Shield shield))
		{
			shield.OnHittedDetails(gameObject);
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
		}

		EffectsManager.Instance.PlayEffect(effectToPlay, point, normal, 3);

		enabled = false;
	}


	protected override void OnExplosion()
	{
		gameObject.SetActive(false);
	}
}