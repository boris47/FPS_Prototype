
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

	private			bool				m_HasLight				= false;
	private			bool				m_HasFlare				= false;



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();

		this.m_HasLight = this.transform.SearchComponent( ref this.m_PointLight, ESearchContext.LOCAL, null );
		this.m_HasFlare = this.transform.SearchComponent( ref this.m_LensFlare,  ESearchContext.LOCAL, null );
	}



	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Override )
	protected override IEnumerator SetupBulletCO()
	{
		yield return base.SetupBulletCO();

		if (this.m_HasLight )
		{
			this.m_PointLight.color = this.m_Renderer.material.GetColor( "_EmissionColor" );
			this.m_BulletEffect = this.m_PointLight;

			if (this.m_HasFlare )
			{
				this.m_LensFlare.color = this.m_PointLight.color;
			}
		}

		yield return null;

		this.SetActive( false );
	}

	//////////////////////////////////////////////////////////////////////////
	// Setup ( Override )
	public	override		void	Setup( Entity whoRef, Weapon weaponRef )
	{
		this.m_WhoRef		= whoRef;
		this.m_Weapon		= weaponRef;

		if ( whoRef )
		{
			whoRef.SetCollisionStateWith(this.m_Collider, state: false );
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
		switch (this.m_BulletMotionType )
		{
			case EBulletMotionType.INSTANT:
				this.ShootInstant( position, direction, velocity );
				break;
			case EBulletMotionType.DIRECT:
				this.ShootDirect( position, direction, velocity );
				break;
			case EBulletMotionType.PARABOLIC:
				this.ShootParabolic( position, direction, velocity );
				break;
			default:
				break;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ShootInstant ( Virtual )
	protected	virtual		void	ShootInstant( Vector3 position, Vector3 direction, float maxDistance = Mathf.Infinity )
	{
		bool bHasHit = Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity, Utils.LayersHelper.Layers_AllButOne("Bullets"));
		if ( bHasHit )
		{
			bool bIsBullet = hit.transform.HasComponent<Bullet>();
			if ( bIsBullet == true )
				return;

			EEffectType effectToPlay = EEffectType.ENTITY_ON_HIT;

			IEntity entity = null;
			IShield shield = null;
			if ( Utils.Base.SearchComponent( hit.transform.gameObject, ref entity, ESearchContext.LOCAL ) )
			{
				entity.Events.OnHittedBullet( this );
			}
			else if ( Utils.Base.SearchComponent( hit.transform.gameObject, ref shield, ESearchContext.CHILDREN ) )
			{
				shield.CollisionHit(this.gameObject );
			}
			else
			{
				effectToPlay = EEffectType.AMBIENT_ON_HIT;
			}

			EffectsManager.Instance.PlayEffect( effectToPlay, hit.point, hit.normal, 3 );
		}
		EffectsManager.Instance.PlayEffect( EEffectType.MUZZLE, position, direction, 0, 0.1f );
	}


	//////////////////////////////////////////////////////////////////////////
	// ShootDirect ( Virtual )
	protected	virtual		void	ShootDirect( Vector3 position, Vector3 direction, float velocity )
	{
		this.transform.up			= direction;
		this.transform.position		= position;
		this.m_StartPosition			= position;
		this.m_RigidBody.velocity	= this.m_RigidBodyVelocity = direction * ( ( velocity > 0f ) ? velocity : this.m_Velocity );
		this.m_RigidBody.useGravity	= false;
		this.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// ShootParabolic ( Virtual )
	protected	virtual		void	ShootParabolic( Vector3 position, Vector3 direction, float velocity )
	{
		this.transform.up			= direction;
		this.transform.position		= position;
		this.m_RigidBody.velocity	= this.m_RigidBodyVelocity = direction * ( ( velocity > 0f ) ? velocity : this.m_Velocity );
		this.m_StartPosition			= position;
		this.m_RigidBody.useGravity	= true;
		this.SetActive( true );
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
		bool bIsBullet = other.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		EEffectType effectToPlay = EEffectType.ENTITY_ON_HIT;

		IEntity entity = null;
		IShield shield = null;
		if ( Utils.Base.SearchComponent( other.gameObject, ref entity, ESearchContext.LOCAL ) )
		{
			entity.Events.OnHittedDetails(this.m_StartPosition, this.m_WhoRef, this.m_DamageType, 0, this.m_CanPenetrate );
		}
		else if ( Utils.Base.SearchComponent( other.gameObject, ref shield, ESearchContext.CHILDREN ) )
		{
			shield.CollisionHit(this.gameObject );
		}
		else
		{
			effectToPlay = EEffectType.AMBIENT_ON_HIT;
		}

		;

		Vector3 position = other.ClosestPointOnBounds(this.transform.position );
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

		EEffectType effectToPlay = EEffectType.ENTITY_ON_HIT;

		IEntity entity = null;
		IShield shield = null;
		if ( Utils.Base.SearchComponent( collision.gameObject, ref entity, ESearchContext.LOCAL ) )
		{
			entity.Events.OnHittedDetails(this.m_StartPosition, this.m_WhoRef, this.m_DamageType, 0, this.m_CanPenetrate );
		}
		else if ( Utils.Base.SearchComponent( collision.gameObject, ref shield, ESearchContext.CHILDREN ) )
		{
			shield.CollisionHit(this.gameObject );
		}
		else
		{
			effectToPlay = EEffectType.AMBIENT_ON_HIT;
		}

		Vector3 position  = collision.contacts[0].point;
		Vector3 direction = collision.contacts[0].normal;
		EffectsManager.Instance.PlayEffect( effectToPlay, position, direction, 3 );

		this.SetActive( false );
	}
	
}
