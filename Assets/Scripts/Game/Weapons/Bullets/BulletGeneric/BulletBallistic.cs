
using UnityEngine;


public class BulletBallistic : BulletGeneric {


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

		float traveledDistance = ( m_StartPosition - transform.position ).sqrMagnitude;
		if ( traveledDistance > m_Range * m_Range )
		{
			SetActive( false );
		}

		switch ( m_BulletMotionType )
		{
			case BulletMotionType.INSTANT:
			{
				break;
			}
			case BulletMotionType.DIRECT:
			{
			//	m_RigidBody.velocity	= m_RigidBodyVelocity;
				transform.up			= m_RigidBodyVelocity;
				break;
			}
			case BulletMotionType.PARABOLIC:
			{
				transform.up = m_RigidBody.velocity;
				break;
			}
			default:
			{
				break;
			}
		}
	}


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
	protected	override		void	ShootInstant( Vector3 position, Vector3 direction, float maxDistance = Mathf.Infinity )
	{
		RaycastHit hit = default( RaycastHit );
		bool bHasHit = Physics.Raycast( position, direction, out hit, Mathf.Infinity, Utils.Base.LayersAllButOne( "Bullets" ) );
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
	protected	override		void	ShootDirect( Vector3 position, Vector3 direction, float velocity )
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
	protected	override		void	ShootParabolic( Vector3 position, Vector3 direction, float velocity )
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
	{ }

	
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
