using UnityEngine;
using System.Collections;


public class FragGranade : GranadeBase {

	private		Collider[]		m_SphereResults		= new Collider[ 100 ];

	private		bool			m_BlowOnHit			= false;

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void	Awake()
	{
		base.Awake();

		// LOAD CONFIGURATION
		{
			CFG_Reader.Section section = null;
			GameManager.Configs.GetSection( "FragGranade", ref section );

			m_DamageMax					= section.AsFloat( "Damage",			m_DamageMax );
			m_Range						= section.AsFloat( "Radius",			m_Range );
			m_Velocity					= section.AsFloat( "ThrowForce",		m_Velocity );
			m_ExplosionDelay			= section.AsFloat( "ExplosionDelay",	m_ExplosionDelay );
			m_BlowOnHit					= section.AsBool(  "BlowOnHit",			m_BlowOnHit );
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

			// Entites
			IEntity entity = hittedCollider.GetComponent<IEntity>();
			if ( entity != null )
			{
				float dmgMult = Vector3.Distance( transform.position, entity.Transform.position ) / m_Range + 0.001f;
				float damage = m_DamageMax * dmgMult;
				if ( entity.Shield != null && entity.Shield.Status > 0.0f )
				{
					entity.Shield.OnHit( m_StartPosition, m_WhoRef, m_Weapon, damage, m_CanPenetrate );
				}
				else
				{
					entity.OnHit( m_StartPosition, m_WhoRef, damage, m_CanPenetrate );
				}
			}

			// Dynamic props
			Rigidbody rb = hittedCollider.GetComponent<Rigidbody>();
			if ( entity == null && rb != null )
			{
				rb.AddExplosionForce( 1000, transform.position, m_Range, 3.0f );
			}			
		}
		EffectManager.Instance.PlayEffect( EffectType.EXPLOSION, transform.position, Vector3.up, 0 );
		SetActive( false );
		m_InternalCounter	= 0f;
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
