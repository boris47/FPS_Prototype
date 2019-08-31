using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IExplosive {
	bool		BlowOnHit							{ get; }
	bool		AttachOnHit							{ get; }
	float		BlastRadius							{ get; }
	float		BlastDamage							{ get; }

	void		ForceExplosion						();
}

public interface IFlyingExplosive {
	float		GetMaxRange							();
}

public interface ITimedExplosive {
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
	// ConfigureInternal ( Override )
	protected	override	void	ConfigureInternal( Database.Section bulletSection )
	{
		m_BlowOnHit		= bulletSection.AsBool( "bBlowOnHit", m_BlowOnHit );
		m_AttachOnHit	= bulletSection.AsBool( "bAttachOnHit", m_AttachOnHit );
		m_BlastRadius	= bulletSection.AsFloat( "fBlastRadius", m_BlastRadius );
		m_BlastDamage	= bulletSection.AsFloat( "fBlastDamage", m_BlastDamage );
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
		this.enabled					= state;
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
