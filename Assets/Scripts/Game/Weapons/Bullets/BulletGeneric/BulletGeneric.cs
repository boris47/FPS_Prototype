
using System;
using System.Collections;
using UnityEngine;


public interface IBulletGeneric {
}


/// <summary>
/// Base class for projectiles
/// </summary>
public abstract class BulletGeneric : Bullet, IBulletGeneric {

	protected		Light				m_PointLight			= null;
	protected		LensFlare			m_LensFlare				= null;

	private			bool				m_bHasLight				= false;
	private			bool				m_bHasFlare				= false;



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();

		m_bHasLight = transform.SearchComponent( ref m_PointLight, SearchContext.LOCAL, null );
		m_bHasFlare = transform.SearchComponent( ref m_LensFlare,  SearchContext.LOCAL, null );
	}



	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Override )
	protected override IEnumerator SetupBulletCO()
	{
		yield return base.SetupBulletCO();

		if ( m_bHasLight )
		{
			m_PointLight.color = m_Renderer.material.GetColor( "_EmissionColor" );
			m_BulletEffect = m_PointLight;

			if ( m_bHasFlare )
			{
				m_LensFlare.color = m_PointLight.color;
			}
		}

		yield return null;

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
			whoRef.SetCollisionStateWith( m_Collider, state: false );
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
	public		override	void	Shoot( Vector3 position, Vector3 direction, float velocity )
	{
		switch ( m_BulletMotionType )
		{
			case BulletMotionType.INSTANT:		ShootInstant( position, direction, velocity );
				break;
			case BulletMotionType.DIRECT:		ShootDirect( position, direction, velocity );
				break;
			case BulletMotionType.PARABOLIC:	ShootParabolic( position, direction, velocity );
				break;
			default:
				break;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ShootInstant ( Virtual )
	protected	virtual		void	ShootInstant( Vector3 position, Vector3 direction, float maxDistance = Mathf.Infinity )
	{
		RaycastHit hit = default( RaycastHit );
		bool bHasHit = Physics.Raycast( position, direction, out hit, Mathf.Infinity, Utils.LayersHelper.Layers_AllButOne( "Bullets" ) );
		if ( bHasHit )
		{
			bool bIsBullet = hit.transform.HasComponent<Bullet>();
			if ( bIsBullet == true )
				return;

			EffectType effectToPlay = EffectType.ENTITY_ON_HIT;

			IEntity entity = null;
			IShield shield = null;
			if ( Utils.Base.SearchComponent( hit.transform.gameObject, ref entity, SearchContext.LOCAL ) )
			{
				entity.Events.OnHittedBullet( this );
			}
			else if ( Utils.Base.SearchComponent( hit.transform.gameObject, ref shield, SearchContext.CHILDREN ) )
			{
				shield.CollisionHit( gameObject );
			}
			else
			{
				effectToPlay = EffectType.AMBIENT_ON_HIT;
			}

			EffectsManager.Instance.PlayEffect( effectToPlay, hit.point, hit.normal, 3 );
		}
		EffectsManager.Instance.PlayEffect( EffectType.MUZZLE, position, direction, 0, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	// ShootDirect ( Virtual )
	protected	virtual		void	ShootDirect( Vector3 position, Vector3 direction, float velocity )
	{
		transform.up			= direction;
		transform.position		= position;
		m_StartPosition			= position;
		m_RigidBody.velocity	= m_RigidBodyVelocity = direction * ( ( velocity > 0f ) ? velocity : m_Velocity );
		m_RigidBody.useGravity	= false;
		SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// ShootParabolic ( Virtual )
	protected	virtual		void	ShootParabolic( Vector3 position, Vector3 direction, float velocity )
	{
		transform.up			= direction;
		transform.position		= position;
		m_RigidBody.velocity	= m_RigidBodyVelocity = direction * ( ( velocity > 0f ) ? velocity : m_Velocity );
		m_StartPosition			= position;
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

		EffectType effectToPlay = EffectType.ENTITY_ON_HIT;

		IEntity entity = null;
		IShield shield = null;
		if ( Utils.Base.SearchComponent( other.gameObject, ref entity, SearchContext.LOCAL ) )
		{
			entity.Events.OnHittedDetails( m_StartPosition, m_WhoRef, m_DamageType, 0, m_CanPenetrate );
		}
		else if ( Utils.Base.SearchComponent( other.gameObject, ref shield, SearchContext.CHILDREN ) )
		{
			shield.CollisionHit( gameObject );
		}
		else
		{
			effectToPlay = EffectType.AMBIENT_ON_HIT;
		}

		;

		Vector3 position = other.ClosestPointOnBounds( transform.position );
		Vector3 direction = other.transform.position - position;
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

		EffectType effectToPlay = EffectType.ENTITY_ON_HIT;

		IEntity entity = null;
		IShield shield = null;
		if ( Utils.Base.SearchComponent( collision.gameObject, ref entity, SearchContext.LOCAL ) )
		{
			entity.Events.OnHittedDetails( m_StartPosition, m_WhoRef, m_DamageType, 0, m_CanPenetrate );
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
	
}
