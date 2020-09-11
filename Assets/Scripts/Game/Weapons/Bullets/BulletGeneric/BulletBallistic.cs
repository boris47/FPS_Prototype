
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

		float traveledDistance = (this.m_StartPosition - this.transform.position ).sqrMagnitude;
		if ( traveledDistance > this.m_Range * this.m_Range )
		{
			this.SetActive( false );
		}

		switch (this.m_BulletMotionType )
		{
			case EBulletMotionType.INSTANT:
			{
				break;
			}
			case EBulletMotionType.DIRECT:
			{
				//	m_RigidBody.velocity	= m_RigidBodyVelocity;
				this.transform.up			= this.m_RigidBodyVelocity;
				break;
			}
			case EBulletMotionType.PARABOLIC:
			{
				this.transform.up = this.m_RigidBody.velocity;
				break;
			}
			default:
			{
				break;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ShootInstant ( Virtual )
	public	override		void	ShootInstant( Vector3 position, Vector3 direction, float maxDistance = Mathf.Infinity )
	{
		if ( Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity, Utils.LayersHelper.Layers_AllButOne("Bullets")) )
		{
			bool bIsBullet = hit.transform.HasComponent<Bullet>();
			if ( bIsBullet == true )
				return;

			EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;
			if ( Utils.Base.SearchComponent( hit.transform.gameObject, out IEntity entity, ESearchContext.LOCAL ) )
			{
//				entity.Events.OnHittedBullet( this );

				entity.Events.OnHittedDetails(this.m_StartPosition, this.m_WhoRef, this.m_DamageType, this.m_Damage, this.m_CanPenetrate );
			}
			else if ( Utils.Base.SearchComponent( hit.transform.gameObject, out IShield shield, ESearchContext.CHILDREN ) )
			{
				shield.CollisionHit(this.gameObject );
			}
			else
			{
				effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
				hit.rigidbody?.AddForceAtPosition( direction * this.m_Velocity * this.m_RigidBody.mass, hit.point, ForceMode.Impulse );
			}

			EffectsManager.Instance.PlayEffect( effectToPlay, hit.point, hit.normal, 3 );
		}
		EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.MUZZLE, position, direction, 0, 0.1f );
	}

	//////////////////////////////////////////////////////////////////////////
	// SetActive ( Override )
	public		override	void	SetActive( bool state )
	{
		// Reset
		if ( state == false )
		{
			this.transform.position		= Vector3.zero;
			this.m_RigidBody.velocity	= Vector3.zero;
		}

		this.m_RigidBody.angularVelocity = Vector3.zero;
		this.m_RigidBody.detectCollisions = state;
		//	m_Collider.enabled = state;
		//	m_Renderer.enabled = state;
		this.gameObject.SetActive( state );
	//	this.enabled = state;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter ( Override )
	protected override void OnTriggerEnter( Collider other )
	{
		if ( other.isTrigger ) return;

		bool bIsBullet = other.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;
		if ( Utils.Base.SearchComponent( other.gameObject, out IEntity entity, ESearchContext.LOCAL ) )
		{
			entity.Events.OnHittedDetails(this.m_StartPosition, this.m_WhoRef, this.m_DamageType, this.m_Damage, this.m_CanPenetrate );
		}
		else if ( Utils.Base.SearchComponent( other.gameObject, out IShield shield, ESearchContext.CHILDREN ) )
		{
			shield.CollisionHit(this.gameObject );
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
			other.attachedRigidbody?.AddForce( this.m_RigidBody.velocity * this.m_RigidBody.mass, ForceMode.Impulse );
		}

		Vector3 position = this.transform.position;
		Vector3 direction = this.m_RigidBody.velocity;
		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, 3 );
		
		this.SetActive( false );
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected	override	void	OnCollisionEnter( Collision collision )
	{
		bool bIsBullet = collision.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;
		if ( Utils.Base.SearchComponent( collision.gameObject, out IEntity entity, ESearchContext.LOCAL ) )
		{
			entity.Events.OnHittedDetails(this.m_StartPosition, this.m_WhoRef, this.m_DamageType, this.m_Damage, this.m_CanPenetrate );
		}
		else if ( Utils.Base.SearchComponent( collision.gameObject, out IShield shield, ESearchContext.CHILDREN ) )
		{
			shield.CollisionHit(this.gameObject );
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
			collision.rigidbody?.AddForceAtPosition(this.m_RigidBodyVelocity, collision.contacts[0].point );
		}

		Vector3 position  = collision.contacts[0].point;
		Vector3 direction = collision.contacts[0].normal;
		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, 3 );

		this.SetActive( false );
	}
	
}
