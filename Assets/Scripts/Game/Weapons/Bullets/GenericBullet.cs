
using UnityEngine;


public class GenericBullet : Bullet {

	private	Light		m_PointLight		= null;
	private	LensFlare	m_LensFlare			= null;



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();

		m_RigidBody.useGravity					= false;

		m_BulletEffect = m_PointLight = GetComponent<Light>();

		m_LensFlare = GetComponent<LensFlare>();

		if ( m_PointLight != null )
			m_PointLight.color = m_Renderer.material.GetColor( "_EmissionColor" );

		if ( m_LensFlare != null )
			m_LensFlare.color = m_PointLight.color;

		SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////											// For Generic Bullets
	// Setup ( Override )
	public		override	void	Setup( float damage,  bool canPenetrate, Entity whoRef, Weapon weapon )
	{
		float multDamage = damage * m_DamageMult;
		m_DamageMin		= Mathf.Min( damage, multDamage );
		m_DamageMax		= Mathf.Max( damage, multDamage );
		m_WhoRef		= whoRef;
		m_Weapon		= weapon;
		m_CanPenetrate	= canPenetrate;
	}


	//////////////////////////////////////////////////////////////////////////
	// Setup ( Override )
	public override void Setup( float damageMin, float damageMax, bool canPenetrate, Entity whoRef, Weapon weapon )		// For Entities
	{
		m_DamageMin		= damageMin;
		m_DamageMax		= damageMax;
		m_WhoRef		= whoRef;
		m_Weapon		= weapon;
		m_CanPenetrate	= canPenetrate;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected	override	void	Update()
	{
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
		transform.up			= direction;
		transform.position		= position;
		m_StartPosition			= position;
		m_RigidBody.velocity	= m_RigidBodyVelocity = direction * ( ( velocity > 0f ) ? velocity : m_Velocity );
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
	// OnCollisionEnter ( Override )
	protected	override	void	OnCollisionEnter( Collision collision )
	{
		Bullet bullet = collision.gameObject.GetComponent<Bullet>();
		if ( bullet != null )
			return;

		Entity entity = collision.gameObject.GetComponent<Entity>();
		Shield shield = collision.gameObject.GetComponent<Shield>();

		if ( ( entity != null || shield != null ) && ( m_WhoRef is NonLiveEntity && entity is NonLiveEntity ) == false )
		{
			EffectManager.Instance.PlayEntityOnHit( collision.contacts[0].point, collision.contacts[0].normal );
		}
		else
		{
			EffectManager.Instance.PlayAmbientOnHit( collision.contacts[0].point, collision.contacts[0].normal );
		}

		if ( shield != null )
			shield.OnHit( m_Instance );
		else
		if ( entity != null )
		{
			Rigidbody rb = entity.Interface.RigidBody;
			rb.angularVelocity = rb.velocity = Vector3.zero;
			entity.OnHit( m_Instance );
		}

		this.SetActive( false );
	}

}
