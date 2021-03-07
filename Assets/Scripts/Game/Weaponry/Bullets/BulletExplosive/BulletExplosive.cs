using UnityEngine;


public interface IExplosive
{
	bool		BlowOnHit							{ get; }
	bool		AttachOnHit							{ get; }
	float		BlastRadius							{ get; }
	float		BlastDamage							{ get; }

	void		ForceExplosion						();
}

public interface IFlyingExplosive
{
	float		GetMaxRange							();
}

public interface ITimedExplosive
{
	float		GetExplosionDelay					();
	float		GetRemainingTime					();
	float		GetRemainingTimeNormalized			();
}

/// <summary>
/// Base class for rockets and granades
/// </summary>
public abstract class BulletExplosive : BulletGeneric, IExplosive {

	[SerializeField, ReadOnly]
	protected	bool			m_BlowOnHit					= true;

	[SerializeField, ReadOnly]
	protected	bool			m_AttachOnHit				= false;

	[SerializeField, ReadOnly]
	protected	float			m_BlastRadius				= 0.0f;

	[SerializeField, ReadOnly]
	protected	float			m_BlastDamage				= 0.0f;

	// INTERFACE START
		bool	IExplosive.BlowOnHit			{	get { return m_BlowOnHit; } }
		bool	IExplosive.AttachOnHit			{	get { return m_AttachOnHit; } }
		float	IExplosive.BlastRadius			{	get { return m_BlastRadius; } }
		float	IExplosive.BlastDamage			{	get { return m_BlastDamage;	} }
	// INTERFACE END
	

	
	protected	float			m_Emission					= 0f;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void	Awake()
	{
		base.Awake();

		SetActive( false );
	}



	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Override )
	protected override void SetupBullet()
	{
		base.SetupBullet();

		m_BlowOnHit	= m_BulletSection.AsBool( "bBlowOnHit", m_BlowOnHit );
		m_AttachOnHit	= m_BulletSection.AsBool( "bAttachOnHit", m_AttachOnHit );
		m_BlastRadius	= m_BulletSection.AsFloat( "fBlastRadius", m_BlastRadius );
		m_BlastDamage	= m_BulletSection.AsFloat( "fBlastDamage", m_BlastDamage );

		SetActive( false );
	}


	
	//////////////////////////////////////////////////////////////////////////
	// SetActive ( Override )
	public		override	void	SetActive( bool state )
	{
		// Reset
		if ( state == false )
		{
			transform.position			= Vector3.zero;
			m_RigidBody.velocity		= Vector3.zero;
		}
		else
		{
			m_Emission					= 0f;
		}
		m_RigidBody.mass				= float.Epsilon;
		m_RigidBody.useGravity			= state;
		m_RigidBody.detectCollisions	= state;
		m_Collider.enabled				= state;
		m_Renderer.enabled				= state;
		m_Renderer.material.SetColor( "_EmissionColor", Color.red );
		enabled					= state;
	}


	//////////////////////////////////////////////////////////////////////////
	// ForceExplosion ( Virtual )
	public		virtual		void	ForceExplosion()
	{
		OnExplosion();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnExplosion ( Abstract )
	protected	abstract	void	OnExplosion();
	
}
