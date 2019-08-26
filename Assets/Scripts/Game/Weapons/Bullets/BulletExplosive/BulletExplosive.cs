using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletExplosive : BulletGeneric, IExplosive {


	[SerializeField, ReadOnly]
	protected	float			m_ExplosionDelay			= 3f;

	[SerializeField, ReadOnly]
	protected	bool			m_AttachOnHit				= false;

	// INTERFACE START

		bool IExplosive.BlowOnHit
		{
			get {
				return false;
			}
		}

		float IExplosive.BlastRadius
		{
			get {
				return 0f;
			}
		}

		float IExplosive.BlastDamage
		{
			get {
				return 0f;
			}
		}

				float			IExplosive.ExplosionDelay		{	get { return m_ExplosionDelay; }}
				bool			IExplosive.AttachOnHit			{	get { return m_AttachOnHit; }		}
	// INTERFACE END
	
	protected	float			m_InternalCounter			= 0f;
	protected	float			m_Emission					= 0f;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void	Awake()
	{
		base.Awake();

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
		m_InternalCounter				= m_ExplosionDelay;
		this.enabled					= state;
	}


	//////////////////////////////////////////////////////////////////////////
	// GetRemainingTime ( Virtual )
	public		virtual		float	GetRemainingTime()
	{
		return Mathf.Clamp( m_InternalCounter, 0f, 10f );
	}


	//////////////////////////////////////////////////////////////////////////
	// GetRemainingTimeNormalized ( Virtual )
	public		virtual		float	GetRemainingTimeNormalized()
	{
		return 1f - (  m_InternalCounter / m_ExplosionDelay );
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
