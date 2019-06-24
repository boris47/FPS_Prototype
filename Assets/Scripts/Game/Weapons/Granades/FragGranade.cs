﻿using UnityEngine;
using System.Collections;


public class FragGranade : GranadeBase {

	private		Collider[]		m_SphereResults		= new Collider[ 100 ];

	private		bool			m_BlowOnHit			= false;

	[SerializeField]
	private class FragGranadeSectionData {
		public float DamageMax		=  0f;
		public float Range			=  0f;
		public float Velocity		=  0f;
		public float ExplosionDelay	=  0f;
		public bool	 BlowOnHit		=  false;
	}
	[SerializeField, ReadOnly]
	private FragGranadeSectionData m_FragGranadeSectionData = new FragGranadeSectionData();

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			if ( GlobalManager.Configs.bGetSection( "FragGranade", m_FragGranadeSectionData ) )
			{
				m_DamageMax					= m_FragGranadeSectionData.DamageMax;
				m_Range						= m_FragGranadeSectionData.Range;
				m_Velocity					= m_FragGranadeSectionData.Velocity;
				m_ExplosionDelay			= m_FragGranadeSectionData.ExplosionDelay;
				m_BlowOnHit					= m_FragGranadeSectionData.BlowOnHit;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Override )
	public override void Shoot( Vector3 position, Vector3 direction, float velocity )
	{
		transform.position		= position;
		m_RigidBody.velocity	= direction * ( ( velocity > 0f ) ? velocity : m_Velocity );
		m_StartPosition = position;
		SetActive( true );
	}

	//////////////////////////////////////////////////////////////////////////
	// SetActive
	public override void	SetActive( bool state )
	{
		base.SetActive( state );
	}


	//////////////////////////////////////////////////////////////////////////
	// GetRemainingTime
	public override float	GetRemainingTime()
	{
		return base.GetRemainingTime();
	}


	//////////////////////////////////////////////////////////////////////////
	// GetRemainingTimeNormalized
	public override float	GetRemainingTimeNormalized()
	{
		return base.GetRemainingTimeNormalized();
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	protected override void Update()
	{
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
	// ForceExplosion
	public override void ForceExplosion()
	{
		base.ForceExplosion();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnExplosion
	protected override	void	OnExplosion()
	{
		int nresults = Physics.OverlapSphereNonAlloc( transform.position, m_Range, m_SphereResults );
		for ( int i = 0; i < nresults; i++ )
		{
			Collider hittedCollider = m_SphereResults[ i ];


			// Entity
			IEntity entity = null;
			IShield shield = null;
			bool bIsEntity = Utils.Base.SearchComponent( hittedCollider.gameObject, ref entity, SearchContext.LOCAL );
			bool bHasShield = Utils.Base.SearchComponent( hittedCollider.gameObject, ref shield, SearchContext.LOCAL );

			if ( bIsEntity && ( ( bHasShield && shield.Status > 0f ) || true ) )
			{
				float dmgMult = Vector3.Distance( transform.position, entity.Transform.position ) / m_Range + 0.001f;
				float damage = m_DamageMax * dmgMult;
//				if ( entity.Shield != null && entity.Shield.Status > 0.0f )
//				{
//					entity.Shield.OnHit( m_StartPosition, m_WhoRef, m_Weapon, damage, m_CanPenetrate );
//				}
//				else
				{
					entity.Events.OnHittedDetails( m_StartPosition, m_WhoRef, damage, m_CanPenetrate );
				}
			}

			// Dynamic props
			Rigidbody rb = null;
			if ( bIsEntity == false && hittedCollider.transform.SearchComponent( ref rb, SearchContext.LOCAL ) )
			{
				rb.AddExplosionForce( 1000, transform.position, m_Range, 3.0f );
			}			
		}
		EffectsManager.Instance.PlayEffect( EffectType.EXPLOSION, transform.position, Vector3.up, 0 );
		SetActive( false );
		m_InternalCounter	= 0f;
	}

	protected override void OnTriggerEnter( Collider other )
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{
		if ( m_BlowOnHit == true )
		{
			ForceExplosion();
		}
	}

}
