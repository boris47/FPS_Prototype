
using UnityEngine;


public class GenericBullet : Bullet {

	private	Light		m_PointLight		= null;
	private	LensFlare	m_LensFlare			= null;



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();
		
		bool bHasLight = transform.SearchComponent( ref m_PointLight, SearchContext.LOCAL, null );
		bool bHasFlare = transform.SearchComponent( ref m_LensFlare,  SearchContext.LOCAL, null );

		if ( bHasLight )
		{
			m_PointLight.color = m_Renderer.material.GetColor( "_EmissionColor" );
			m_BulletEffect = m_PointLight;

			if ( bHasFlare  )
			{
				m_LensFlare.color = m_PointLight.color;
			}
		}

		SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// Setup ( Override )
	public	override		void	Setup( bool canPenetrate, Entity whoRef, Weapon weaponRef, float damageMin = -1f, float damageMax = -1f )
	{
		m_CanPenetrate	= canPenetrate;
		m_WhoRef		= whoRef;
		m_Weapon		= weaponRef;

		if ( damageMin <= 0.0f )
		{
			float multDamage = damageMax * m_DamageMult;
			m_DamageMin		= Mathf.Min( damageMax, multDamage );
			m_DamageMax		= Mathf.Max( damageMax, multDamage );
		}
		else
		{
			m_DamageMin = damageMin;
			m_DamageMax = damageMax;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected	override	void	Update()
	{
		// Only every 10 frames
		if ( Time.frameCount % 25 == 0 )
			return;

		float traveledDistance = ( m_StartPosition - transform.position ).sqrMagnitude;
		if ( traveledDistance > m_Range * m_Range )
		{
			SetActive( false );
		}

		m_RigidBody.velocity	= m_RigidBodyVelocity;
		transform.up			= m_RigidBodyVelocity;
	}


	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Override )
	public		override	void	Shoot( Vector3 position, Vector3 direction, float velocity )
	{
		switch ( m_MotionType )
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
		bool bHasHit = Physics.Raycast( position, direction, out hit, Mathf.Infinity, Utils.Base.LayersAllButOne( "Bullets" ) );
		if ( bHasHit )
		{
			Bullet bullet = hit.transform.gameObject.GetComponent<Bullet>();
			if ( bullet != null )
				return;

			IEntity entity = null;
			bool bIsAnEntity = Utils.Base.SearchComponent( hit.transform.gameObject, ref entity, SearchContext.LOCAL );
			IShield shield = null;
			bool bIsShield = Utils.Base.SearchComponent( hit.transform.gameObject, ref shield, SearchContext.CHILDREN );

			if ( bIsAnEntity )
			{
				entity.Events.OnHittedBullet( this );
			}
			else
			if ( bIsShield )
			{
				shield.CollisionHit( gameObject );
			}

			EffectType effectToPlay;

			if ( bIsShield )
			{
				effectToPlay = EffectType.ENTITY_ON_HIT;
			}
			else
			// If is an entity and who and hitted entites are of different category
			if ( bIsAnEntity == true && ( ( m_WhoRef is NonLiveEntity && entity is NonLiveEntity ) == false ) )
			{
				effectToPlay = EffectType.ENTITY_ON_HIT;
				entity.RigidBody.angularVelocity = entity.RigidBody.velocity = Vector3.zero;
			}
			else
			{
				effectToPlay = EffectType.AMBIENT_ON_HIT;
			}

			EffectManager.Instance.PlayEffect( effectToPlay, hit.point, hit.normal, 3 );
		}
		EffectManager.Instance.PlayEffect( EffectType.MUZZLE, position, direction, 0, 0.1f );
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
//		transform.up			= direction;
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
		m_Collider.enabled = state;
		m_Renderer.enabled = state;
		gameObject.SetActive( state );
		this.enabled = state;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter ( Override )
	protected override void OnTriggerEnter( Collider other )
	{ }

	
	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected	override	void	OnCollisionEnter( Collision collision )
	{
		bool bIsBullet = collision.transform.HasComponent<Bullet>();
		if ( bIsBullet == true )
			return;

		IEntity entity = null;
		IShield shield = null;
		bool bIsAnEntity = Utils.Base.SearchComponent( collision.gameObject, ref entity, SearchContext.LOCAL );
		bool bIsShield   = Utils.Base.SearchComponent( collision.gameObject, ref shield, SearchContext.CHILDREN, s => s.Context == ShieldContext.ENTITY );

		Vector3 position  = collision.contacts[0].point;
		Vector3 direction = collision.contacts[0].normal;
		
		EffectType effectToPlay;
		if ( bIsShield )
		{
			effectToPlay = EffectType.ENTITY_ON_HIT;
		}
		else
		// If is an entity and who and hitted entites are of different category
		if ( bIsAnEntity == true && ( ( m_WhoRef is NonLiveEntity && entity is NonLiveEntity ) == false ) )
		{
			effectToPlay = EffectType.ENTITY_ON_HIT;
			entity.RigidBody.angularVelocity = entity.RigidBody.velocity = Vector3.zero;
			entity.Events.OnHittedBullet( this );
		}
		else
		{
			effectToPlay = EffectType.AMBIENT_ON_HIT;
		}

		EffectManager.Instance.PlayEffect( effectToPlay, position, direction, 3 );

		this.SetActive( false );
	}
	
}
