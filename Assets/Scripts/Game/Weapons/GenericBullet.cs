
using UnityEngine;


public class GenericBullet : Bullet {


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();

		m_RigidBody.useGravity					= false;

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
	// OnEnable ( Override )
	protected	override	void	OnEnable()
	{
		m_RigidBody.angularVelocity = Vector3.zero;
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
	}


	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Override )
	public		override	void	Shoot( Vector3 position, Vector3 direction, float velocity )
	{
		transform.up			= direction;
		transform.position		= position;
		m_StartPosition			= position;
		m_RigidBody.velocity	= direction * ( ( velocity > 0f ) ? velocity : m_Velocity );
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
		
		m_RigidBody.detectCollisions = state;
		m_Collider.enabled = state;
		m_Renderer.enabled = state;
		this.enabled = state;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected	override	void	OnCollisionEnter( Collision collision )
	{
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

		/*
		Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
		if ( rb != null )
		{
//			rb.AddForce( m_RigidBody.velocity * 0.01f );
		}
		*/
		if ( shield != null )
			shield.OnHit( ref m_Instance );
		else
		if ( entity != null )
		{
			Rigidbody erg = ( entity as IEntity ).RigidBody;
			erg.angularVelocity = erg.velocity = Vector3.zero;
			entity.OnHit( ref m_Instance );
		}


		this.SetActive( false );
	}

}
