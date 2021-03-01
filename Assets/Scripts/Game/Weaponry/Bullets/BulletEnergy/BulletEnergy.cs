
using System;
using System.Collections;
using UnityEngine;


public interface IBulletEnergy
{

}


/// <summary>
/// Base class for projectiles
/// </summary>
public abstract class BulletEnergy : Bullet, IBulletEnergy
{
	protected		Light				m_PointLight			= null;
	protected		LensFlare			m_LensFlare				= null;

	private			bool				m_HasLight				= false;
	private			bool				m_HasFlare				= false;



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();

		m_HasLight = transform.TrySearchComponent(ESearchContext.LOCAL, out m_PointLight );
		m_HasFlare = transform.TrySearchComponent(ESearchContext.LOCAL, out m_LensFlare);
	}



	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Override )
	protected override void SetupBullet()
	{
		base.SetupBullet();

		if (m_HasLight )
		{
			m_PointLight.color = m_Renderer.material.GetColor( "_EmissionColor" );
			m_BulletEffect = m_PointLight;

			if (m_HasFlare )
			{
				m_LensFlare.color = m_PointLight.color;
			}
		}

		SetActive( false );
	}

	//////////////////////////////////////////////////////////////////////////
	// Setup ( Override )
	public	override		void	Setup( Entity whoRef, Weapon weaponRef )
	{
		m_WhoRef		= whoRef;
		m_Weapon		= weaponRef;

		if ( whoRef )
		{
			whoRef.SetCollisionStateWith(m_Collider, state: false );
		}
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected	override	void	Update()
	{
		// Only every 25 frames
		if ( Time.frameCount % 25 == 0 )
			return;

		m_RigidBody.velocity	= m_RigidBodyVelocity;
		transform.up			= m_RigidBodyVelocity;

		float traveledDistance = ( m_StartPosition - transform.position ).sqrMagnitude;
		if ( traveledDistance > m_Range * m_Range )
		{
			SetActive( false );
		}
	}
	*/

	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Override )
	public		override	void	Shoot( Vector3 position, Vector3 direction, float? velocity )
	{
		switch (m_BulletMotionType )
		{
			case EBulletMotionType.INSTANT:
				ShootInstant( position, direction, velocity );
				break;
			case EBulletMotionType.DIRECT:
				ShootDirect( position, direction, velocity );
				break;
			case EBulletMotionType.PARABOLIC:
				ShootParabolic( position, direction, velocity );
				break;
		}
	}

	
		//////////////////////////////////////////////////////////////////////////
	// ShootInstant ( Virtual )
	public	override		void	ShootInstant( Vector3 position, Vector3 direction, float? maxDistance )
	{
		bool bHasHit = Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity, Utils.LayersHelper.Layers_AllButOne("Bullets"));
		if ( bHasHit )
		{
			bool bIsBullet = hit.transform.HasComponent<Bullet>();
			if ( bIsBullet == true )
				return;

			EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;

			if ( Utils.Base.TrySearchComponent( hit.transform.gameObject, ESearchContext.LOCAL, out IEntity entity ) )
			{
				entity.Events.OnHittedBullet( this );
			}
			else if ( Utils.Base.TrySearchComponent( hit.transform.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out IShield shield ) )
			{
				shield.CollisionHit(gameObject );
			}
			else
			{
				effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
			}

			EffectsManager.Instance.PlayEffect( effectToPlay, hit.point, hit.normal, 3 );
		}
		EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.MUZZLE, position, direction, 0, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	// ShootDirect ( Virtual )
	public	override		void	ShootDirect( Vector3 position, Vector3 direction, float? velocity )
	{
		transform.up			= direction;
		transform.position		= position;
		m_StartPosition		= position;
		m_RigidBody.velocity	= m_RigidBodyVelocity = direction * (velocity ?? m_Velocity);
		m_RigidBody.useGravity	= false;
		SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// ShootParabolic ( Virtual )
	public	override		void	ShootParabolic( Vector3 position, Vector3 direction, float? velocity )
	{
		transform.up			= direction;
		transform.position		= position;
		m_RigidBody.velocity	= m_RigidBodyVelocity = direction * ( velocity ?? m_Velocity );
		m_StartPosition		= position;
		m_RigidBody.useGravity	= true;
		SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// SetActive ( Override )
	public		override	void	SetActive( bool state )
	{
		// Reset
		if ( state == false )
		{
			transform.position		= Vector3.zero;
			m_RigidBody.velocity	= Vector3.zero;
		}

		m_RigidBody.angularVelocity = Vector3.zero;
		m_RigidBody.detectCollisions = state;
		//	m_Collider.enabled = state;
		//	m_Renderer.enabled = state;
		gameObject.SetActive( state );
	//	this.enabled = state;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter ( Override )
	protected override void OnTriggerEnter( Collider other )
	{
		bool bIsBullet = other.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;

		if (Utils.Base.TrySearchComponent( other.gameObject, ESearchContext.LOCAL, out IEntity entity ) )
		{
			entity.Events.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, 0, m_CanPenetrate );
		}
		else if ( Utils.Base.TrySearchComponent( other.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out IShield shield ) )
		{
			shield.CollisionHit(gameObject );
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
		}

		;

		Vector3 position = other.ClosestPointOnBounds(transform.position );
		Vector3 direction = other.transform.position - position;
		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, 3 );

		SetActive( false );
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected	override	void	OnCollisionEnter( Collision collision )
	{
		bool bIsBullet = collision.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;

		if ( Utils.Base.TrySearchComponent( collision.gameObject, ESearchContext.LOCAL, out IEntity entity ) )
		{
			entity.Events.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, 0, m_CanPenetrate );
		}
		else if ( Utils.Base.TrySearchComponent( collision.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out IShield shield ) )
		{
			shield.CollisionHit(gameObject );
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
		}

		Vector3 position  = collision.contacts[0].point;
		Vector3 direction = collision.contacts[0].normal;
		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, 3 );

		SetActive( false );
	}
	
}
