using UnityEngine;


public interface IExplosive
{
	bool		BlowOnHit							{ get; }
	bool		AttachOnHit							{ get; }
	float		BlastRadius							{ get; }
	float		BlastDamage							{ get; }

	void		ForceExplosion						();
}


public interface ITimedExplosive
{
	float		GetExplosionDelay					();
	float		GetRemainingTime					();
	float		GetRemainingTimeNormalized			();
}

/// <summary> Base class for rockets and granades </summary>
public abstract class BulletExplosive : BulletGeneric, IExplosive
{
	[SerializeField, ReadOnly]
	protected	bool			m_BlowOnHit					= true;

	[SerializeField, ReadOnly]
	protected	bool			m_AttachOnHit				= false;

	[SerializeField, ReadOnly]
	protected	float			m_BlastRadius				= 0.0f;

	[SerializeField, ReadOnly]
	protected	float			m_BlastDamage				= 0.0f;

	// INTERFACE START
				bool			IExplosive.BlowOnHit		=> m_BlowOnHit;
				bool			IExplosive.AttachOnHit		=> m_AttachOnHit;
				float			IExplosive.BlastRadius		=> m_BlastRadius;
				float			IExplosive.BlastDamage		=> m_BlastDamage;
	// INTERFACE END



	protected	float			m_Emission					= 0f;



	//////////////////////////////////////////////////////////////////////////
	protected override void SetupBullet()
	{
		base.SetupBullet();

		CustomAssertions.IsTrue(m_BulletSection.TryAsBool("BlowOnHit", out m_BlowOnHit));
		CustomAssertions.IsTrue(m_BulletSection.TryAsBool("AttachOnHit", out m_AttachOnHit));

		CustomAssertions.IsTrue(m_BulletSection.TryAsFloat("BlastRadius", out m_BlastRadius));
		CustomAssertions.IsTrue(m_BulletSection.TryAsFloat("BlastDamage", out m_BlastDamage));
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnEnable()
	{
		base.OnEnable();

		m_Emission						= 0f;

		m_Renderer.material.SetColor("_EmissionColor", Color.red);

		m_RigidBody.mass				= float.Epsilon;
		m_RigidBody.useGravity			= true;
		m_RigidBody.detectCollisions	= true;
		m_Collider.enabled				= true;
		m_Renderer.enabled				= true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDisable()
	{
		transform.position				= Vector3.zero;
		m_RigidBody.velocity			= Vector3.zero;

		m_RigidBody.mass				= float.Epsilon;
		m_RigidBody.useGravity			= false;
		m_RigidBody.detectCollisions	= false;
		m_Collider.enabled				= false;
		m_Renderer.enabled				= false;

		base.OnDisable();
	}


	//////////////////////////////////////////////////////////////////////////
	public override void Shoot(in Vector3 origin, in Vector3 direction, in float velocity, in float impactForceMultiplier)
	{
		base.Shoot(origin, direction, velocity, impactForceMultiplier);

		switch (m_BulletMotionType)
		{
			case EBulletMotionType.INSTANT:		ShootInstant(origin, direction);	break;
			case EBulletMotionType.DIRECT:		ShootDirect(origin, direction);		break;
			case EBulletMotionType.PARABOLIC:	ShootParabolic(origin, direction);	break;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public virtual void ForceExplosion()
	{
		OnExplosion();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	OnExplosion();
	
}
