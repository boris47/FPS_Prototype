using UnityEngine;
using System.Collections;
using System;

public sealed class BulletRocket : BulletExplosive, IFlyingExplosive
{
	private	float		m_MaxRange = 80.0f;


	// INTERFACE START
	float IFlyingExplosive.GetMaxRange() => m_MaxRange;
	// INTERFACE END


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();
	}



	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Override )
	protected override void SetupBullet()
	{
		base.SetupBullet();

		m_MaxRange = m_BulletSection.AsFloat( "fMaxRange", m_MaxRange );
	}



	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected	override	void	Update()
	{
		// Only every 25 frames
		if ( Time.frameCount % 25 == 0 )
			return;

		m_RigidBody.velocity	= m_RigidBodyVelocity;
		transform.up			= m_RigidBodyVelocity;

		float traveledDistance = (m_StartPosition - transform.position ).sqrMagnitude;
		if ( traveledDistance > m_Range * m_Range )
		{
			OnExplosion();
		}
	}

	protected override void OnCollisionDetailed( in Vector3 point, in Vector3 normal, in Collider otherCollider )
	{
		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;
		if (Utils.Base.TrySearchComponent(otherCollider.gameObject, ESearchContext.LOCAL, out Entity entity))
		{
			entity.OnHittedDetails(m_StartPosition, m_WhoRef, EDamageType.EXPLOSIVE, 0, false);
		}
		else if (Utils.Base.TrySearchComponent(otherCollider.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Shield shield))
		{
			shield.CollisionHit(gameObject);
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
		}

		EffectsManager.Instance.PlayEffect(effectToPlay, point, normal, 3);

		SetActive(false);
	}


	protected override void OnExplosion()
	{
		
	}
}