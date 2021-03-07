
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
			return Mathf.Clamp(m_InternalCounter, 0f, 10f );
		}
		float		ITimedExplosive.GetRemainingTimeNormalized			()
		{
			return 1f - (m_InternalCounter / m_ExplosionDelay );
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
	protected override void SetupBullet()
	{
		base.SetupBullet();

		m_ExplosionDelay = m_BulletSection.AsFloat("fExplosionDelay", m_ExplosionDelay);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Shoot( Vector3 position, Vector3 direction, float? velocity )
	{
		transform.position		= position;
		m_RigidBody.velocity	= direction * (velocity ?? m_Velocity);
		m_StartPosition = position;
		SetActive(true);
	}



	//////////////////////////////////////////////////////////////////////////
	public override void SetActive(bool state)
	{
		base.SetActive(state);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void Update()
	{
		m_InternalCounter -= Time.deltaTime;
		if (m_InternalCounter < 0)
		{
			OnExplosion();
			return;
		}

		m_Emission += Time.deltaTime * 2f;
		m_Renderer.material.SetColor("_EmissionColor", Color.red * m_Emission);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void ForceExplosion()
	{
		base.ForceExplosion();
	}


	//////////////////////////////////////////////////////////////////////////
	protected override	void	OnExplosion()
	{
		int nresults = Physics.OverlapSphereNonAlloc(transform.position, m_Range, m_SphereResults);
		for (int i = 0; i < nresults; i++)
		{
			Collider hittedCollider = m_SphereResults[i];

			bool bIsEntity = Utils.Base.TrySearchComponent(hittedCollider.gameObject, ESearchContext.LOCAL, out Entity entity);
			bool bHasShield = Utils.Base.TrySearchComponent(hittedCollider.gameObject, ESearchContext.LOCAL, out Shield shield);

			if (bIsEntity && ((bHasShield && shield.Status > 0f) || true))
			{
				float dmgMult = (Vector3.Distance(transform.position, entity.transform.position) / m_Range) + 0.001f;
				float damage = m_Damage * dmgMult;
//				if ( entity.Shield != null && entity.Shield.Status > 0.0f )
//				{
//					entity.Shield.OnHit( m_StartPosition, m_WhoRef, m_Weapon, damage, m_CanPenetrate );
//				}
//				else
				{
					entity.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, damage, m_CanPenetrate);
				}
			}

			// Dynamic props
			if (!bIsEntity && hittedCollider.transform.TrySearchComponent(ESearchContext.LOCAL, out Rigidbody rb))
			{
				rb.AddExplosionForce(1000, transform.position, m_Range, 3.0f);
			}
		}
		EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.EXPLOSION, transform.position, Vector3.up, 0);
		SetActive(false);
		m_InternalCounter	= 0f;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnCollisionEnter(Collision collision)
	{
		if (m_BlowOnHit)
		{
			ForceExplosion();
		}
	}
}
