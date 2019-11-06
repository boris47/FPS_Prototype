
using System.Collections;
using UnityEngine;



public sealed class GranadeFrammentation : BulletExplosive, ITimedExplosive {

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

	private		Collider[]		m_SphereResults		= new Collider[ 100 ];



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void	Awake()
	{
		base.Awake();
	}



	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Override )
	protected override IEnumerator SetupBulletCO()
	{
		yield return base.SetupBulletCO();

		m_ExplosionDelay = m_BulletSection.AsFloat( "fExplosionDelay", m_ExplosionDelay );
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
			IEntity entityInterface = null;
			IShield shield = null;
			bool bIsEntity = Utils.Base.SearchComponent( hittedCollider.gameObject, ref entityInterface, SearchContext.LOCAL );
			bool bHasShield = Utils.Base.SearchComponent( hittedCollider.gameObject, ref shield, SearchContext.LOCAL );

			if ( bIsEntity && ( ( bHasShield && shield.Status > 0f ) || true ) )
			{
				float dmgMult = Vector3.Distance( transform.position, entityInterface.AsEntity.transform.position ) / m_Range + 0.001f;
				float damage = m_Damage * dmgMult;
//				if ( entity.Shield != null && entity.Shield.Status > 0.0f )
//				{
//					entity.Shield.OnHit( m_StartPosition, m_WhoRef, m_Weapon, damage, m_CanPenetrate );
//				}
//				else
				{
					entityInterface.Events.OnHittedDetails( m_StartPosition, m_WhoRef, m_DamageType, damage, m_CanPenetrate );
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
