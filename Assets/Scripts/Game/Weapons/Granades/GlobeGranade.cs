﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlobeGranade : GranadeBase {
	
	[SerializeField]
	private		float					m_Duration					= 3f;

	private		Transform				m_ExplosionGlobe			= null;
	private		bool					m_InExplosion				= false;

	private		List<Entity>			m_Entites					= new List<Entity>();

	private		WaitForSeconds			m_WaitInstruction			= null;

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

		m_WaitInstruction	= new WaitForSeconds( m_Duration );
		m_ExplosionGlobe	= transform.GetChild(0);
		m_ExplosionGlobe.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable ( Override )
	protected override void OnEnable()
	{
		m_ExplosionGlobe.localScale = Vector3.zero;
		m_ExplosionGlobe.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		m_ExplosionGlobe.localScale = Vector3.zero;
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
		SetActive( false );
		transform.position		= position;
		m_RigidBody.velocity	= direction * ( ( velocity > 0f ) ? velocity : m_Velocity );
		SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// SetActive ( Override )
	public override void	SetActive( bool state )
	{
		m_RigidBody.constraints = RigidbodyConstraints.None;
		StopAllCoroutines();
		m_InternalCounter = 0f;
		m_InExplosion = false;
		m_Entites.Clear();
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
		{
			MakeDamage();
			return;
		}

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
	// MakeDamage
	public	void	MakeDamage()
	{
		foreach( IEntity entity in m_Entites )
		{
			float tmpDmg = m_DamageMax;
			m_DamageMax *= m_DamageMult;
			entity.OnHit( m_Instance );
			m_DamageMax = tmpDmg;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ForceExplosion ( Override )
	public		override	void		ForceExplosion()
	{
		base.ForceExplosion();
	}


	///////////////////////////////////////////////////////////////////////////
	// OnExplosion ( Override )
	protected	override	void		OnExplosion()
	{
		if ( m_InExplosion == true )
			return;

		m_InExplosion = true;

		StartCoroutine( ExplosionCO() );
	}


	//////////////////////////////////////////////////////////////////////////
	// ExplosionCO ( Coroutine )
	private					IEnumerator	ExplosionCO()
	{
		m_ExplosionGlobe.localScale = Vector3.zero;
		Vector3 finalScale			= Vector3.one * ( m_Range ) * ( transform.localScale.x * 40f );
		float	interpolant			= 0f;

		m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;
		m_Renderer.enabled = false;
		m_ExplosionGlobe.gameObject.SetActive( true );

		while( interpolant < 1f )
		{
			interpolant += Time.deltaTime * 0.5f;
			m_ExplosionGlobe.localScale = Vector3.LerpUnclamped( Vector3.zero, finalScale, interpolant );
			yield return null;
		}

		yield return m_WaitInstruction; // wait for m_Duration

		m_RigidBody.constraints = RigidbodyConstraints.None;

		m_ExplosionGlobe.localScale = Vector3.zero;
		m_ExplosionGlobe.gameObject.SetActive( false );
		m_InExplosion = false;

		m_InternalCounter = 0f;
		SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{
		if ( m_InExplosion == true )
			return;

		m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;

		bool hitEntity = collision.gameObject.GetComponent<Entity>() != null;
		bool hitShield = collision.gameObject.GetComponent<Shield>() != null;
		bool hitBullet = collision.gameObject.GetComponent<Bullet>() != null;

		if ( ( hitEntity || hitShield || hitShield ) && m_InExplosion == false )
		{
			OnExplosion();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity == null )
			return;
		
		if ( m_Entites.Contains( entity ) )
			return;

		m_Entites.Add( entity );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerExit( Collider other )
	{
		Entity entity = other.GetComponent<Entity>();
		if ( entity == null )
			return;

		if ( m_Entites.Contains( entity ) )
			return;

		m_Entites.Remove( entity );
	}

}
