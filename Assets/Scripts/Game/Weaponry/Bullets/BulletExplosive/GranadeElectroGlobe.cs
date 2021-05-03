
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class GranadeElectroGlobe : BulletExplosive, ITimedExplosive
{	
	[SerializeField, ReadOnly]
	private		float					m_ExplosionDelay									= 3.0f;

	// INTERFACE START
				float					ITimedExplosive.GetExplosionDelay					() => m_ExplosionDelay;
				float					ITimedExplosive.GetRemainingTime					() => Mathf.Clamp(m_InternalCounter, 0f, 10f);
				float					ITimedExplosive.GetRemainingTimeNormalized			() => 1f - (m_InternalCounter / m_ExplosionDelay);
	// INTERFACE END

	private		float					m_InternalCounter									= 0f;

	[SerializeField]
	private		float					m_Duration											= 3f;

	private		Transform				m_ExplosionGlobe									= null;
	private		bool					m_InExplosion										= false;

	private		List<Entity>			m_Entites											= new List<Entity>();


	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		CustomAssertions.IsTrue(m_BulletMotionType == EBulletMotionType.PARABOLIC, $"{GetType().Name} can only have motion type {EBulletMotionType.PARABOLIC.ToString()}");

		if (CustomAssertions.IsNotNull(m_ExplosionGlobe = transform.GetChild(0)))
		{
			m_ExplosionGlobe.gameObject.SetActive(false);
		}
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

		m_RigidBody.constraints = RigidbodyConstraints.None;
		m_InternalCounter = 0f;
		m_InExplosion = false;
		m_Entites.Clear();
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDisable()
	{
		m_ExplosionGlobe.localScale = Vector3.zero;
		m_ExplosionGlobe.gameObject.SetActive(false);

		m_RigidBody.constraints = RigidbodyConstraints.None;
		m_InternalCounter = 0f;
		m_InExplosion = false;
		m_Entites.Clear();

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
		if (m_InExplosion)
		{
			MakeDamage();
		}
		else
		{
			m_InternalCounter -= deltaTime;
			if (m_InternalCounter < 0f)
			{
				OnExplosion();
			}
			else
			{
				m_Emission += deltaTime * 2f;
				m_Renderer.material.SetColor("_EmissionColor", Color.red * m_Emission);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void MakeDamage()
	{
		m_Entites.ForEach(e => e.OnHittedDetails(m_StartPosition, m_WhoRef, m_DamageType, m_Damage, canPenetrate: false));
	}


	///////////////////////////////////////////////////////////////////////////
	protected override void OnExplosion()
	{
		if (!m_InExplosion)
		{
			m_InExplosion = true;
			CoroutinesManager.Start(ExplosionCO(), "GranadeElectroGlobe::OnExplosion: Explosion CO");
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private IEnumerator ExplosionCO()
	{
		m_ExplosionGlobe.localScale = Vector3.zero;
		Vector3 finalScale = Vector3.one * m_Range * (transform.localScale.x * 40f);
		float interpolant = 0f;

		m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;
		m_Renderer.enabled = false;
		m_ExplosionGlobe.gameObject.SetActive(true);

		while (interpolant < 1f)
		{
			interpolant += Time.deltaTime * 0.5f;
			m_ExplosionGlobe.localScale = Vector3.LerpUnclamped(Vector3.zero, finalScale, interpolant);
			yield return null;
		}

		yield return new WaitForSeconds(m_Duration);

		m_RigidBody.constraints = RigidbodyConstraints.None;

		m_ExplosionGlobe.localScale = Vector3.zero;
		m_ExplosionGlobe.gameObject.SetActive(false);
		m_InExplosion = false;

		m_InternalCounter = 0f;
		gameObject.SetActive(false);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnCollisionEnter(Collision collision)
	{
		if (!m_InExplosion)
		{
			m_RigidBody.constraints = RigidbodyConstraints.FreezeAll;

			bool hitEntity = collision.transform.TryGetComponent(out Entity e);
			bool hitShield = collision.transform.TryGetComponent(out Shield s);
			bool hitBullet = collision.transform.TryGetComponent(out Bullet b);
			if (hitEntity || hitShield || hitBullet)
			{
				OnExplosion();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnTriggerEnter(Collider other)
	{
		if (other.transform.TrySearchComponent(ESearchContext.LOCAL, out Entity entity))
		{
			m_Entites.AddUnique(entity);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit(Collider other)
	{
		if (other.transform.TrySearchComponent(ESearchContext.LOCAL, out Entity entity))
		{
			if (m_Entites.Contains(entity))
			{
				m_Entites.Remove(entity);
			}
		}
	}
}
