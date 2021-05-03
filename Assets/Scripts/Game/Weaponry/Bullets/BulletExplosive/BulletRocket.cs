
using UnityEngine;

public sealed class BulletRocket : BulletExplosive
{
	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		CustomAssertions.IsTrue(m_BulletMotionType == EBulletMotionType.DIRECT, $"{GetType().Name} can only have motion type {EBulletMotionType.DIRECT.ToString()}");
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void OnEndTravel()
	{
		gameObject.SetActive(false);
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void OnCollisionDetailed(in Vector3 point, in Vector3 normal, in Collider otherCollider)
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
			if (otherCollider.attachedRigidbody)
			{
				effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
			}
			else
			{
				return; // hitting a trigger volume
			}
		}

		EffectsManager.Instance.PlayEffect(effectToPlay, point, normal, 3);
		gameObject.SetActive(false);
		OnExplosion();
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Shoot(in Vector3 origin, in Vector3 direction, in float velocity, in float impactForceMultiplier)
	{
		base.Shoot(origin, direction, velocity, impactForceMultiplier);

		ShootDirect(origin, direction);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnExplosion()
	{
		// TODO Spawn explosion
	}
}