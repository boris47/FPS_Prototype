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
		bool	IExplosive.BlowOnHit			{	get { return this.m_BlowOnHit; } }
		bool	IExplosive.AttachOnHit			{	get { return this.m_AttachOnHit; } }
		float	IExplosive.BlastRadius			{	get { return this.m_BlastRadius; } }
		float	IExplosive.BlastDamage			{	get { return this.m_BlastDamage;	} }
	// INTERFACE END
	

	
	protected	float			m_Emission					= 0f;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void	Awake()
	{
		base.Awake();

		this.SetActive( false );
	}



	//////////////////////////////////////////////////////////////////////////
	// SetupBulletCO ( Override )
	protected override IEnumerator SetupBulletCO()
	{
		yield return base.SetupBulletCO();

		this.m_BlowOnHit		= this.m_BulletSection.AsBool( "bBlowOnHit", this.m_BlowOnHit );
		this.m_AttachOnHit	= this.m_BulletSection.AsBool( "bAttachOnHit", this.m_AttachOnHit );
		this.m_BlastRadius	= this.m_BulletSection.AsFloat( "fBlastRadius", this.m_BlastRadius );
		this.m_BlastDamage	= this.m_BulletSection.AsFloat( "fBlastDamage", this.m_BlastDamage );

		this.SetActive( false );
	}


	
	//////////////////////////////////////////////////////////////////////////
	// SetActive ( Override )
	public		override	void	SetActive( bool state )
	{
		// Reset
		if ( state == false )
		{
			this.transform.position			= Vector3.zero;
			this.m_RigidBody.velocity		= Vector3.zero;
		}
		else
		{
			this.m_Emission					= 0f;
		}
		this.m_RigidBody.mass				= float.Epsilon;
		this.m_RigidBody.useGravity			= state;
		this.m_RigidBody.detectCollisions	= state;
		this.m_Collider.enabled				= state;
		this.m_Renderer.enabled				= state;
		this.m_Renderer.material.SetColor( "_EmissionColor", Color.red );
		this.enabled					= state;
	}


	//////////////////////////////////////////////////////////////////////////
	// ForceExplosion ( Virtual )
	public		virtual		void	ForceExplosion()
	{
		this.OnExplosion();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnExplosion ( Abstract )
	protected	abstract	void	OnExplosion();
	
}
