
using UnityEngine;



public sealed class GranadeFrammentation : BulletExplosive, ITimedExplosive
{
	[SerializeField, ReadOnly]
	private		float					m_ExplosionDelay									= 3.0f;

	// INTERFACE START
				float					ITimedExplosive.GetExplosionDelay					() => m_ExplosionDelay;
				float					ITimedExplosive.GetRemainingTime					() => Mathf.Clamp(m_InternalCounter, 0f, 10f);
				float					ITimedExplosive.GetRemainingTimeNormalized			() => 1f - (m_InternalCounter / m_ExplosionDelay);
	// INTERFACE END

	private		float					m_InternalCounter									= 0f;

	private		Collider[]				m_SphereResults										= new Collider[100];


	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		CustomAssertions.IsTrue(m_BulletMotionType == EBulletMotionType.PARABOLIC, $"{GetType().Name} can only have motion type {EBulletMotionType.PARABOLIC.ToString()}");
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void SetupBullet()
	{
		base.SetupBullet();

		CustomAssertions.IsTrue(m_BulletSection.TryAsFloat("ExplosionDelay", out m_ExplosionDelay));
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnEnable()
	{
		base.OnEnable();
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDisable()
	{
		m_InternalCounter = 0f;

		base.OnDisable();
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Shoot(in Vector3 origin, in Vector3 direction, in float velocity, in float impactForceMultiplier)
	{
		base.Shoot(origin, direction, velocity, impactForceMultiplier);

		ShootParabolic(origin, direction);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame(float deltaTime)
	{
		m_InternalCounter -= deltaTime;
		if (m_InternalCounter < 0)
		{
			OnExplosion();
		}
		else
		{
			m_Emission += deltaTime * 2f;
			m_Renderer.material.SetColor("_EmissionColor", Color.red * m_Emission);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override	void	OnExplosion()
	{
		for (int i = 0; i < Physics.OverlapSphereNonAlloc(transform.position, m_Range, m_SphereResults); i++)
		{
			Collider hittedCollider = m_SphereResults[i];

			bool bIsEntity = Utils.Base.TrySearchComponent(hittedCollider.gameObject, ESearchContext.LOCAL, out Entity entity);
			bool bHasShield = Utils.Base.TrySearchComponent(hittedCollider.gameObject, ESearchContext.LOCAL, out Shield shield);
			if (bIsEntity)
			{
				if ((bHasShield && shield.Status > 0f) || true)
				{
					float dmgMult = (Vector3.Distance(transform.position, entity.transform.position) / m_Range) + 0.001f;
					float damage = m_Damage * dmgMult;
					entity.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, damage, m_CanPenetrate);
				}
			}
			else
			{
				// Dynamic props
				if (hittedCollider.transform.TrySearchComponent(ESearchContext.LOCAL, out Rigidbody rb))
				{
					rb.AddExplosionForce(1000, transform.position, m_Range, 3.0f);
				}
			}
		}
		EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.EXPLOSION, transform.position, Vector3.up, 0);
		gameObject.SetActive(false);
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
