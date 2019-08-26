
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GranadeElectroGlobe : BulletExplosive {
	
	[SerializeField]
	private		float					m_Duration					= 3f;

	private		Transform				m_ExplosionGlobe			= null;
	private		bool					m_InExplosion				= false;

	private		List<Entity>			m_Entites					= new List<Entity>();

	private		WaitForSeconds			m_WaitInstruction			= null;


	[SerializeField]
	private class GranadeElectroGlobeSectionData {
		public float Damage			=  0f;
		public float Range			=  0f;
		public float Velocity		=  0f;
		public float ExplosionDelay	=  0f;
	}
	[SerializeField, ReadOnly]
	private GranadeElectroGlobeSectionData m_GranadeElectroGlobeSectionData = new GranadeElectroGlobeSectionData();

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			if ( GlobalManager.Configs.bGetSection( "GranadeElectroGlobe", m_GranadeElectroGlobeSectionData ) )
			{
				m_DamageMax					= m_GranadeElectroGlobeSectionData.Damage;
				m_Range						= m_GranadeElectroGlobeSectionData.Range;
				m_Velocity					= m_GranadeElectroGlobeSectionData.Velocity;
				m_ExplosionDelay			= m_GranadeElectroGlobeSectionData.ExplosionDelay;
			}
		}

		m_WaitInstruction	= new WaitForSeconds( m_Duration );
		m_ExplosionGlobe	= transform.GetChild(0);
		m_ExplosionGlobe.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// ReadInternals ( Override )
	protected override void ReadInternals( Database.Section section )
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		m_ExplosionGlobe.localScale = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Override )
	public override void Shoot( Vector3 position, Vector3 direction, float velocity )
	{
		SetActive( false );
		{
			transform.position		= position;
			m_RigidBody.velocity	= direction * ( ( velocity > 0f ) ? velocity : m_Velocity );
			m_StartPosition = position;
		}
		SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// SetActive ( Override )
	public override void	SetActive( bool state )
	{
		StopAllCoroutines();
		m_RigidBody.constraints		= RigidbodyConstraints.None;
		m_InternalCounter			= 0f;
		m_InExplosion				= false;
		m_Entites.Clear();

		if ( m_ExplosionGlobe == null )
		{
			m_ExplosionGlobe	= transform.GetChild(0);
		}
		m_ExplosionGlobe.localScale = Vector3.zero;
		m_ExplosionGlobe.gameObject.SetActive( false );

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
		if ( m_InternalCounter < 0f )
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
		m_Entites.ForEach( ( Entity entity ) =>
		{
			entity.OnHittedDetails( m_StartPosition, m_WhoRef, m_DamageMax, canPenetrate: false );
		} );
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

		print("OnCollision");

		if ( m_RigidBody.constraints == RigidbodyConstraints.FreezeAll )
			return;

		m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;

		bool hitEntity = collision.gameObject.GetComponent<Entity>() != null;
		bool hitShield = collision.gameObject.GetComponent<Shield>() != null;
		bool hitBullet = collision.gameObject.GetComponent<Bullet>() != null;

		if ( ( hitEntity || hitShield || hitBullet ) && m_InExplosion == false )
		{
			OnExplosion();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	protected	override void OnTriggerEnter( Collider other )
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
