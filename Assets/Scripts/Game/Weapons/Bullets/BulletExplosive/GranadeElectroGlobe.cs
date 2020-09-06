
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class GranadeElectroGlobe : BulletExplosive, ITimedExplosive {
	
	[SerializeField, ReadOnly]
	private		float			m_ExplosionDelay	= 3.0f;

	// INTERFACE START
		float		ITimedExplosive.GetExplosionDelay					()
		{
			return this.m_ExplosionDelay;
		}
		float		ITimedExplosive.GetRemainingTime					()
		{
			return Mathf.Clamp(this.m_InternalCounter, 0f, 10f );
		}
		float		ITimedExplosive.GetRemainingTimeNormalized			()
		{
			return 1f - (this.m_InternalCounter / this.m_ExplosionDelay );
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

		this.m_WaitInstruction	= new WaitForSeconds(this.m_Duration );
		this.m_ExplosionGlobe	= this.transform.GetChild(0);
		this.m_ExplosionGlobe.gameObject.SetActive( false );
	}



	//////////////////////////////////////////////////////////////////////////
	// ConfigureInternal ( Override )
	protected override IEnumerator SetupBulletCO()
	{
		yield return base.SetupBulletCO();

		this.m_ExplosionDelay = this.m_BulletSection.AsFloat( "fExplosionDelay", this.m_ExplosionDelay );
	}



	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		this.m_ExplosionGlobe.localScale = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Override )
	public override void Shoot( Vector3 position, Vector3 direction, float velocity )
	{
		this.SetActive( false );
		{
			this.transform.position		= position;
			this.m_RigidBody.velocity	= direction * ( ( velocity > 0f ) ? velocity : this.m_Velocity );
			this.m_StartPosition = position;
		}
		this.SetActive( true );
	}



	//////////////////////////////////////////////////////////////////////////
	// SetActive ( Override )
	public override void	SetActive( bool state )
	{
		this.StopAllCoroutines();
		this.m_RigidBody.constraints		= RigidbodyConstraints.None;
		this.m_InternalCounter			= 0f;
		this.m_InExplosion				= false;
		this.m_Entites.Clear();

		if (this.m_ExplosionGlobe == null )
		{
			this.m_ExplosionGlobe	= this.transform.GetChild(0);
		}
		this.m_ExplosionGlobe.localScale = Vector3.zero;
		this.m_ExplosionGlobe.gameObject.SetActive( false );

		base.SetActive( state );
	}



	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected override void Update()
	{
		if (this.m_InExplosion == true )
		{
			this.MakeDamage();
			return;
		}

		this.m_InternalCounter -= Time.deltaTime;
		if (this.m_InternalCounter < 0f )
		{
			this.OnExplosion();
			return;
		}

		this.m_Emission += Time.deltaTime * 2f;
		this.m_Renderer.material.SetColor( "_EmissionColor", Color.red * this.m_Emission );
	}



	//////////////////////////////////////////////////////////////////////////
	// MakeDamage
	public	void	MakeDamage()
	{
		this.m_Entites.ForEach( ( Entity entity ) =>
		{
			entity.OnHittedDetails(this.m_StartPosition, this.m_WhoRef, this.m_DamageType, this.m_Damage, canPenetrate: false );
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
		if (this.m_InExplosion == true )
			return;

		this.m_InExplosion = true;

		CoroutinesManager.Start(this.ExplosionCO(), "GranadeElectroGlobe::OnExplosion: Explosion CO" );
	}



	//////////////////////////////////////////////////////////////////////////
	// ExplosionCO ( Coroutine )
	private					IEnumerator	ExplosionCO()
	{
		this.m_ExplosionGlobe.localScale = Vector3.zero;
		Vector3 finalScale			= Vector3.one * (this.m_Range ) * (this.transform.localScale.x * 40f );
		float	interpolant			= 0f;

		this.m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;
		this.m_Renderer.enabled = false;
		this.m_ExplosionGlobe.gameObject.SetActive( true );

		while( interpolant < 1f )
		{
			interpolant += Time.deltaTime * 0.5f;
			this.m_ExplosionGlobe.localScale = Vector3.LerpUnclamped( Vector3.zero, finalScale, interpolant );
			yield return null;
		}

		yield return this.m_WaitInstruction; // wait for m_Duration

		this.m_RigidBody.constraints = RigidbodyConstraints.None;

		this.m_ExplosionGlobe.localScale = Vector3.zero;
		this.m_ExplosionGlobe.gameObject.SetActive( false );
		this.m_InExplosion = false;

		this.m_InternalCounter = 0f;
		this.SetActive( false );
	}



	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{
		if (this.m_InExplosion == true )
			return;

		print("OnCollision");

		if (this.m_RigidBody.constraints == RigidbodyConstraints.FreezeAll )
			return;

		this.m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;

		bool hitEntity = collision.transform.HasComponent<Entity>();
		bool hitShield = collision.transform.HasComponent<Shield>();
		bool hitBullet = collision.transform.HasComponent<Bullet>();

		if ( hitEntity || hitShield || hitBullet )
		{
			this.OnExplosion();
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	protected	override void OnTriggerEnter( Collider other )
	{
		Entity entity = null;
		if ( other.transform.SearchComponent( ref entity, ESearchContext.LOCAL ) )
		{
			if (this.m_Entites.Contains( entity ) )
				return;

			this.m_Entites.Add( entity );
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerExit( Collider other )
	{
		Entity entity = null;
		if ( other.transform.SearchComponent( ref entity, ESearchContext.LOCAL ) )
		{
			if (this.m_Entites.Contains( entity ) )
				return;

			this.m_Entites.Remove( entity );
		}
	}
}
