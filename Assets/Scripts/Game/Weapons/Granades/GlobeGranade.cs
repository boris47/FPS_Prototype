using UnityEngine;
using System.Collections;
using System;

public class GlobeGranade : GranadeBase {
	
	[SerializeField]
	private		float			m_Duration			= 3f;

	private		Transform		m_ExplosionGlobe	= null;
	private		bool			m_InExplosion		= false;

//	private		ParticleSystem	m_ParticleSystem	= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			CFG_Reader.Section section = null;
			GameManager.Configs.GetSection( "GlobeGranade", ref section );

			m_DamageMax					= section.AsFloat( "Damage",			m_DamageMax );
			m_Range						= section.AsFloat( "Radius",			m_Range );
			m_Velocity					= section.AsFloat( "ThrowForce",		m_Velocity );
			m_ExplosionDelay			= section.AsFloat( "ExplosionDelay",	m_ExplosionDelay );
		}

		m_ExplosionGlobe	= transform.GetChild(0);
//		m_ParticleSystem	= GetComponentInChildren<ParticleSystem>();
	}

	//////////////////////////////////////////////////////////////////////////
	// OnEnable ( Override )
	protected override void OnEnable()
	{
		m_ExplosionGlobe.localScale = Vector3.zero;
//		m_ParticleSystem.Play( withChildren : true );
	}


	//////////////////////////////////////////////////////////////////////////
	// Setup ( Override )
	public override void Setup( Entity whoRef, Weapon weapon )
	{
		m_WhoRef	= whoRef;
		m_Weapon	= weapon;
	}


	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Override )
	public override void Shoot( Vector3 position, Vector3 direction, float velocity )
	{
		transform.position		= position;
		m_RigidBody.velocity	= direction * ( ( velocity > 0f ) ? velocity : m_Velocity );
		SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// SetActive ( Override )
	public override void	SetActive( bool state )
	{
		StopAllCoroutines();
		m_RigidBody.constraints = RigidbodyConstraints.None;
		m_InExplosion = false;
		base.SetActive( state );
	}


	//////////////////////////////////////////////////////////////////////////
	// GetRemainingTime ( Override )
	public override float	GetRemainingTime()
	{
		return base.GetRemainingTime();
	}


	//////////////////////////////////////////////////////////////////////////
	// GetRemainingTimeNormalized ( Override )
	public override float	GetRemainingTimeNormalized()
	{
		return base.GetRemainingTimeNormalized();
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected override void Update()
	{
		if ( m_InExplosion == true )
			return;

		m_InternalCounter -= Time.deltaTime;
		if ( m_InternalCounter < 0 )
		{
			OnExplosion();
			return;
		}

		m_Emission += Time.deltaTime * 2f;
		m_Renderer.material.SetColor( "_EmissionColor", Color.red * m_Emission );
	}


	//////////////////////////////////////////////////////////////////////////
	// ForceExplosion ( Override )
	public override void ForceExplosion()
	{
		base.ForceExplosion();
	}


	///////////////////////////////////////////////////////////////////////////
	// OnExplosion ( Override )
	protected override	void	OnExplosion()
	{
		if ( m_InExplosion == true )
			return;

		m_InExplosion = true;

		StartCoroutine( ExplosionCO() );
	}


	//////////////////////////////////////////////////////////////////////////
	// ExplosionCO ( Coroutine )
	private	IEnumerator	ExplosionCO()
	{
		m_ExplosionGlobe.localScale = Vector3.zero;
		Vector3 finalScale			= Vector3.one * ( m_Range ) * ( transform.localScale.x * 40f );
		float	interpolant			= 0f;

		m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;
		m_Renderer.enabled = false;

		while( interpolant < 1f )
		{
			interpolant += Time.deltaTime * 0.5f;
			m_ExplosionGlobe.localScale = Vector3.LerpUnclamped( Vector3.zero, finalScale, interpolant );
			yield return null;
		}

		yield return new WaitForSeconds( m_Duration );

		m_RigidBody.constraints = RigidbodyConstraints.None;

		m_ExplosionGlobe.localScale = Vector3.zero;
		m_InExplosion = false;

		SetActive( false );
		m_InternalCounter = 0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{
		Entity entity = collision.gameObject.GetComponent<Entity>();
		Shield shield = collision.gameObject.GetComponent<Shield>();
		if ( entity != null || shield != null )
		{
			OnExplosion();
			return;
		}

		IBullet bullet = collision.gameObject.GetComponent<IBullet>();
		if ( bullet == null )
			m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;
	}


	//////////////////////////////////////////////////////////////////////////
	// MakeDamage
	public	void	MakeDamage( ref Entity entity )
	{
		float tmpDmg = m_DamageMax;
		m_DamageMax *= Time.deltaTime * m_DamageMult;
		entity.OnHit( ref m_Instance );
		m_DamageMax = tmpDmg;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerStay
	private void OnTriggerStay( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity == null )
			return;
		
		MakeDamage( ref entity );
	}

}
