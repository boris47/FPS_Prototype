using UnityEngine;
using System.Collections;


public class FragGranade : GranadeBase {

	private		Collider[]		m_SphereResults		= new Collider[ 100 ];


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
		}

	}

	//////////////////////////////////////////////////////////////////////////
	// OnEnable ( Override )
	protected override void OnEnable()
	{
		
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

			Entity entity = hittedCollider.GetComponent<Entity>();
			if ( entity != null )
			{
				float dmgMult = Vector3.Distance( transform.position, entity.transform.position ) / m_Range;
				float tmpDmg = m_DamageMax;
				m_DamageMax *= dmgMult;
				entity.OnHit( ref m_Instance );
				m_DamageMax = tmpDmg;
			}

			Rigidbody rb = hittedCollider.GetComponent<Rigidbody>();
            if ( entity == null && rb != null )
			{
                rb.AddExplosionForce( 1000, transform.position, m_Range, 3.0F );
			}			
		}
		EffectManager.Instance.PlayEntityExplosion( transform.position, Vector3.up );
		SetActive( false );
		m_InternalCounter	= 0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{}

}
