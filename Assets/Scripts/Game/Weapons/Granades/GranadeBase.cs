
using System;
using UnityEngine;


public interface IGranade {
	float		Radius					{ get; }
	float		ExplosionDelay			{ get; }

	void		ForceExplosion();
}


public abstract class GranadeBase : Bullet, IGranade {
	
	[ReadOnly]
	protected	float			m_ExplosionDelay			= 3f;

	// INTERFACE START
				float			IGranade.Radius				{	get { return m_Range; }		}
				float			IGranade.ExplosionDelay		{	get { return m_ExplosionDelay; }}
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
	// Setup ( Override )
	public override void Setup( bool canPenetrate, Entity whoRef, Weapon weaponRef, float damageMin = -1f, float damageMax = -1f )
	{
		m_Weapon		= weaponRef;
		m_WhoRef		= whoRef;
		m_CanPenetrate	= canPenetrate;
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
