
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class GranadeElectroGlobe : BulletExplosive, ITimedExplosive {
	
	[SerializeField, ReadOnly]
	private		float			m_ExplosionDelay	= 3.0f;

	// INTERFACE START
		float		ITimedExplosive.GetExplosionDelay					()
		{
			return m_ExplosionDelay;
		}
		float		ITimedExplosive.GetRemainingTime					()
		{
			return Mathf.Clamp( m_InternalCounter, 0f, 10f );
		}
		float		ITimedExplosive.GetRemainingTimeNormalized			()
		{
			return 1f - (  m_InternalCounter / m_ExplosionDelay );
		}
	// INTERFACE END

	private		float			m_InternalCounter	= 0f;

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

		m_WaitInstruction	= new WaitForSeconds( m_Duration );
		m_ExplosionGlobe	= transform.GetChild(0);
		m_ExplosionGlobe.gameObject.SetActive( false );
	}



	//////////////////////////////////////////////////////////////////////////
	// ConfigureInternal ( Override )
	protected override IEnumerator SetupBulletCO()
	{
		yield return base.SetupBulletCO();

		m_ExplosionDelay = m_BulletSection.AsFloat( "fExplosionDelay", m_ExplosionDelay );
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

		CoroutinesManager.Start( ExplosionCO(), "GranadeElectroGlobe::OnExplosion: Explosion CO" );
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

		bool hitEntity = collision.transform.HasComponent<Entity>();
		bool hitShield = collision.transform.HasComponent<Shield>();
		bool hitBullet = collision.transform.HasComponent<Bullet>();

		if ( hitEntity || hitShield || hitBullet )
		{
			OnExplosion();
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	protected	override void OnTriggerEnter( Collider other )
	{
		Entity entity = null;
		if ( other.transform.SearchComponent( ref entity, SearchContext.LOCAL ) )
		{
			if ( m_Entites.Contains( entity ) )
				return;

			m_Entites.Add( entity );
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerExit( Collider other )
	{
		Entity entity = null;
		if ( other.transform.SearchComponent( ref entity, SearchContext.LOCAL ) )
		{
			if ( m_Entites.Contains( entity ) )
				return;

			m_Entites.Remove( entity );
		}
	}
}
