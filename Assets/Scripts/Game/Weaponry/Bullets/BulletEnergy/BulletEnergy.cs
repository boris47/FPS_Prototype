
using UnityEngine;


public abstract class BulletEnergy : Bullet
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
	public	override		void	Setup( Entity whoRef, Weapon weaponRef )
	{
		m_WhoRef		= whoRef;
		m_Weapon		= weaponRef;

		if ( whoRef )
		{
			whoRef.SetCollisionStateWith(m_Collider, state: false );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
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
	public	override		void	ShootInstant( Vector3 position, Vector3 direction, float? maxDistance )
	{
		bool bHasHit = Physics.Raycast(position, direction, out RaycastHit hit, Mathf.Infinity, Utils.LayersHelper.Layers_AllButOne("Bullets"));
		if ( bHasHit )
		{
			bool bIsBullet = hit.transform.HasComponent<Bullet>();
			if ( bIsBullet == true )
				return;

			EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;

			if ( Utils.Base.TrySearchComponent( hit.transform.gameObject, ESearchContext.LOCAL, out Entity entity ) )
			{
				entity.OnHittedBullet( this );
			}
			else if ( Utils.Base.TrySearchComponent( hit.transform.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Shield shield ) )
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
	protected override void OnTriggerEnter( Collider other )
	{
		bool bIsBullet = other.transform.HasComponent<Bullet>();
		if (bIsBullet)
			return;

		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;

		if (Utils.Base.TrySearchComponent(other.gameObject, ESearchContext.LOCAL, out Entity entity))
		{
			entity.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, 0, m_CanPenetrate);
		}
		else if (Utils.Base.TrySearchComponent(other.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Shield shield))
		{
			shield.CollisionHit(gameObject);
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
		}

		Vector3 position = other.ClosestPointOnBounds(transform.position);
		Vector3 direction = other.transform.position - position;
		EffectsManager.Instance.PlayEffect(effectToPlay, position, direction, 3);

		SetActive(false);
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	override	void	OnCollisionEnter( Collision collision )
	{
		bool bIsBullet = collision.transform.HasComponent<Bullet>();
		if (bIsBullet)
			return;

		EffectsManager.EEffecs effectToPlay = EffectsManager.EEffecs.ENTITY_ON_HIT;

		if (Utils.Base.TrySearchComponent(collision.gameObject, ESearchContext.LOCAL, out Entity entity))
		{
			entity.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, 0, m_CanPenetrate);
		}
		else if (Utils.Base.TrySearchComponent(collision.gameObject, ESearchContext.LOCAL_AND_CHILDREN, out Shield shield))
		{
			shield.CollisionHit(gameObject);
		}
		else
		{
			effectToPlay = EffectsManager.EEffecs.AMBIENT_ON_HIT;
		}

		Vector3 position = collision.contacts[0].point;
		Vector3 direction = collision.contacts[0].normal;
		EffectsManager.Instance.PlayEffect(effectToPlay, position, direction, 3);

		SetActive(false);
	}
}
