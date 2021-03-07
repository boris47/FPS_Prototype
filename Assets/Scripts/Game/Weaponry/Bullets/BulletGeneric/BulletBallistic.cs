
using UnityEngine;


public class BulletBallistic : BulletGeneric
{
	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected	override	void	Update()
	{
		// Only every 25 frames
		if ( Time.frameCount % 25 == 0 )
			return;

		if ( m_RigidBody.SweepTest( transform.up, out RaycastHit hit, m_RigidBody.velocity.magnitude ))
		{
			OnCollisionDetailed( hit.point, hit.normal, hit.collider );
		}
		else
		{
			float traveledDistance = (m_StartPosition - transform.position ).sqrMagnitude;
			if ( traveledDistance > m_Range * m_Range )
			{
				SetActive( false );
			}
		}

	}


	//////////////////////////////////////////////////////////////////////////
	// ShootInstant ( Virtual )
	public	override		void	ShootInstant( Vector3 position, Vector3 direction, float? maxDistance )
	{
		if ( Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity, Utils.LayersHelper.Layers_AllButOne("Bullets")) )
		{
			bool bIsBullet = hit.transform.HasComponent<Bullet>();
			if ( bIsBullet == true )
				return;

			EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;
			if ( Utils.Base.TrySearchComponent( hit.transform.gameObject, ESearchContext.LOCAL, out Entity entity ) )
			{
				entity.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, m_Damage, m_CanPenetrate );
			}
			else if ( Utils.Base.TrySearchComponent( hit.transform.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Shield shield ) )
			{
				shield.CollisionHit(gameObject );
			}
			else
			{
				effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
				hit.rigidbody?.AddForceAtPosition( direction * m_Velocity * m_RigidBody.mass, hit.point, ForceMode.Impulse );
			}

			EffectsManager.Instance.PlayEffect( effectToPlay, hit.point, hit.normal, 3 );
		}
		EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.MUZZLE, position, direction, 0, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected	override	void	OnCollisionDetailed( in Vector3 point, in Vector3 normal, in Collider otherCollider )
	{
		bool bIsBullet = otherCollider.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;
		if ( Utils.Base.TrySearchComponent( otherCollider.gameObject, ESearchContext.LOCAL_AND_PARENTS, out Entity entity ) )
		{
			entity.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, m_Damage, m_CanPenetrate );
		}
		else if ( Utils.Base.TrySearchComponent( otherCollider.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Shield shield) )
		{
			shield.CollisionHit(gameObject );
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
			otherCollider.attachedRigidbody?.AddForceAtPosition( m_RigidBodyVelocity, point, ForceMode.Impulse );
//			otherCollider.attachedRigidbody?.AddForceAtPosition( this.m_RigidBodyVelocity * this.m_RigidBody.mass, point, ForceMode.Impulse );
		}

		EffectsManager.Instance.PlayEffect( effectToPlay, point, normal, 3 );
		SetActive( false );
	}
	
}
