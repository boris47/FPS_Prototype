
using System.Collections;
using UnityEngine;



public sealed class GranadeFrammentation : BulletExplosive, ITimedExplosive {

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

	private		Collider[]		m_SphereResults		= new Collider[ 100 ];



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void	Awake()
	{
		base.Awake();
	}



	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Override )
	protected override void SetupBulletCO()
	{
		base.SetupBulletCO();

		this.m_ExplosionDelay = this.m_BulletSection.AsFloat( "fExplosionDelay", this.m_ExplosionDelay );
	}



	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Override )
	public override void Shoot( Vector3 position, Vector3 direction, float velocity )
	{
		this.transform.position		= position;
		this.m_RigidBody.velocity	= direction * ( ( velocity > 0f ) ? velocity : this.m_Velocity );
		this.m_StartPosition = position;
		this.SetActive( true );
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
		this.m_InternalCounter -= Time.deltaTime;
		if (this.m_InternalCounter < 0 )
		{
			this.OnExplosion();
			return;
		}

		this.m_Emission += Time.deltaTime * 2f;
		this.m_Renderer.material.SetColor( "_EmissionColor", Color.red * this.m_Emission );
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
		int nresults = Physics.OverlapSphereNonAlloc(this.transform.position, this.m_Range, this.m_SphereResults );
		for ( int i = 0; i < nresults; i++ )
		{
			Collider hittedCollider = this.m_SphereResults[ i ];


			// Entity
			bool bIsEntity = Utils.Base.SearchComponent( hittedCollider.gameObject, out IEntity entityInterface, ESearchContext.LOCAL );
			bool bHasShield = Utils.Base.SearchComponent( hittedCollider.gameObject, out IShield shield, ESearchContext.LOCAL );

			if ( bIsEntity && ( ( bHasShield && shield.Status > 0f ) || true ) )
			{
				float dmgMult = (Vector3.Distance(this.transform.position, entityInterface.AsEntity.transform.position ) / this.m_Range) + 0.001f;
				float damage = this.m_Damage * dmgMult;
//				if ( entity.Shield != null && entity.Shield.Status > 0.0f )
//				{
//					entity.Shield.OnHit( m_StartPosition, m_WhoRef, m_Weapon, damage, m_CanPenetrate );
//				}
//				else
				{
					entityInterface.Events.OnHittedDetails(this.m_StartPosition, this.m_WhoRef, this.m_DamageType, damage, this.m_CanPenetrate );
				}
			}

			// Dynamic props
			Rigidbody rb = null;
			if ( bIsEntity == false && hittedCollider.transform.SearchComponent( ref rb, ESearchContext.LOCAL ) )
			{
				rb.AddExplosionForce( 1000, this.transform.position, this.m_Range, 3.0f );
			}			
		}
		EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.EXPLOSION, this.transform.position, Vector3.up, 0 );
		this.SetActive( false );
		this.m_InternalCounter	= 0f;
	}



	protected override void OnTriggerEnter( Collider other )
	{
		
	}



	//////////////////////////////////////////////////////////////////////////
	// OnCollisionEnter ( Override )
	protected override void OnCollisionEnter( Collision collision )
	{
		if (this.m_BlowOnHit == true )
		{
			this.ForceExplosion();
		}
	}

}
