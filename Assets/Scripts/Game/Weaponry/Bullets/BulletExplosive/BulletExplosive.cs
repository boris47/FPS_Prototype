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

		m_BlowOnHit = m_BulletSection.AsBool("bBlowOnHit", m_BlowOnHit);
		m_AttachOnHit = m_BulletSection.AsBool("bAttachOnHit", m_AttachOnHit);
		m_BlastRadius = m_BulletSection.AsFloat("fBlastRadius", m_BlastRadius);
		m_BlastDamage = m_BulletSection.AsFloat("fBlastDamage", m_BlastDamage);
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
	public virtual void ForceExplosion()
	{
		OnExplosion();
	}


	//////////////////////////////////////////////////////////////////////////
	protected	abstract	void	OnExplosion();
	
}
