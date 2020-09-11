using UnityEngine;
using System.Collections;
using System;

public sealed class BulletRocket : BulletExplosive, IFlyingExplosive {

	private	float		m_MaxRange = 80.0f;


	// INTERFACE START
		float		IFlyingExplosive.GetMaxRange							()
		{
			return this.m_MaxRange;
		}
	// INTERFACE END


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();
	}



	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Override )
	protected override void SetupBulletCO()
	{
		base.SetupBulletCO();

		this.m_MaxRange = this.m_BulletSection.AsFloat( "fMaxRange", this.m_MaxRange );
	}



	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected	override	void	Update()
	{
		// Only every 25 frames
		if ( Time.frameCount % 25 == 0 )
			return;

		this.m_RigidBody.velocity	= this.m_RigidBodyVelocity;
		this.transform.up			= this.m_RigidBodyVelocity;

		float traveledDistance = (this.m_StartPosition - this.transform.position ).sqrMagnitude;
		if ( traveledDistance > this.m_Range * this.m_Range )
		{
			this.OnExplosion();
		}
	}


	/*
	protected override void OnCollisionEnter( Collision collision )
	{
		bool bIsBullet = collision.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		EffectType effectToPlay = EffectType.ENTITY_ON_HIT;

		IEntity entity = null;
		IShield shield = null;
		if ( Utils.Base.SearchComponent( collision.gameObject, ref entity, SearchContext.LOCAL ) )
		{
			entity.Events.OnHittedDetails( m_StartPosition, m_WhoRef, 0, m_CanPenetrate );
		}
		else if ( Utils.Base.SearchComponent( collision.gameObject, ref shield, SearchContext.CHILDREN ) )
		{
			shield.CollisionHit( gameObject );
		}
		else
		{
			effectToPlay = EffectType.AMBIENT_ON_HIT;
		}

		Vector3 position  = collision.contacts[0].point;
		Vector3 direction = collision.contacts[0].normal;
		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, 3 );

		this.SetActive( false );
	}
	*/

	protected override void OnTriggerEnter( Collider other )
	{
//		bool bIsBullet = other.transform.HasComponent<Bullet>();
//		if ( bIsBullet == true )
//			return;

		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;
		if ( Utils.Base.SearchComponent( other.gameObject, out IEntity entity, ESearchContext.LOCAL ) )
		{
			entity.Events.OnHittedDetails(this.m_StartPosition, this.m_WhoRef, EDamageType.EXPLOSIVE, 0, false );
		}
		else if ( Utils.Base.SearchComponent( other.gameObject, out IShield shield, ESearchContext.CHILDREN ) )
		{
			shield.CollisionHit(this.gameObject );
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
		}

		Vector3 position = other.ClosestPointOnBounds(this.transform.position );
		Vector3 direction = other.transform.position - position;
		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, 3 );

		this.SetActive( false );
	}


	protected override void OnExplosion()
	{
		
	}
}