
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Auto
[System.Serializable]
public class WPN_FireMode_Auto : WPN_BaseFireMode
{
	public override EFireMode FireMode => EFireMode.AUTO;

	protected	override	void	InternalSetup	(in Database.Section fireModeSection, in WPN_FireModule fireModule, in float shotDelay, in FireFunctionDel fireFunction)
	{

	}


	public	override	void	ApplyModifier	( Database.Section modifier )
	{ }

	public	override	void	ResetBaseConfiguration()
	{ }

	public	override	void	RemoveModifier( Database.Section modifier )
	{ }


	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		return true;
	}


	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		m_CurrentDelay = 0.0f;
		return true;
	}

	public	override	void	OnWeaponChange	()
	{
		m_CurrentDelay = 0.0f;
	}
	

	//	INTERNAL UPDATE
	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{
		m_CurrentDelay = Mathf.Max( m_CurrentDelay - DeltaTime, 0.0f );
	}

	//	START
	public override		void	OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		if ( m_CurrentDelay <= 0.0f )
		{
			m_FireFunction( baseFireDispersion * m_DispersionMultiplier * 0.5f, baseCamDeviation * m_DeviationMultiplier * 0.5f );
			m_CurrentDelay = m_FireDelay;
		}
	}

	//	INTERNAL UPDATE
	public	override	void	OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{
		if ( m_CurrentDelay <= 0.0f )
		{
			m_FireFunction( baseFireDispersion * m_DispersionMultiplier, baseCamDeviation * m_DeviationMultiplier );
			m_CurrentDelay = m_FireDelay;
		}
	}

	//	END
	public override		void	OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		if ( m_CurrentDelay <= 0.0f )
		{
			m_FireFunction( baseFireDispersion * m_DispersionMultiplier, baseCamDeviation * m_DeviationMultiplier );
			m_CurrentDelay = m_FireDelay;
		}
	}
}



//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Auto_Incremental
public class WPN_FireMode_Auto_Incremental : WPN_FireMode_Auto
{
	private		float	m_IncremetalSpeed			= 1.0f;
	private		float	m_MaxIncrement				= 2.0f;

	private		float	m_CurrentMultiplier			= 1.0f;


//	public WPN_FireMode_Auto_Incremental( Database.Section section ) : base( section ) { }

	protected	override	void	InternalSetup	(in Database.Section fireModeSection, in WPN_FireModule fireModule, in float shotDelay, in FireFunctionDel fireFunction)
	{
		m_IncremetalSpeed	= fireModeSection.AsFloat( "IncremetalSpeed", m_IncremetalSpeed );
		m_MaxIncrement		= fireModeSection.AsFloat( "MaxIncrement", m_MaxIncrement );
	}


	public override void ApplyModifier( Database.Section modifier )
	{
		base.ApplyModifier( modifier );

		// Do actions here
	}


	public	override	void	ResetBaseConfiguration()
	{
		base.ResetBaseConfiguration();

		// Do actions here
	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		base.RemoveModifier( modifier );

		// Do Actions here
	}


//	public override void Setup( IWeapon w, float shotDelay, Action<float, float> fireFunction )
//	{
//		base.Setup( w, shotDelay, fireFunction );
//	}

	public override bool OnLoad( StreamUnit streamUnit )
	{
		m_CurrentMultiplier = 1.0f;

		return base.OnLoad( streamUnit );
	}

	//	INTERNAL UPDATE
	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{
		m_CurrentDelay = Mathf.Max( m_CurrentDelay - ( DeltaTime * m_CurrentMultiplier ), 0.0f );
	}

	public override void OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		base.OnStart( baseFireDispersion, baseCamDeviation );
		m_CurrentMultiplier += m_IncremetalSpeed;
		m_CurrentMultiplier = Mathf.Clamp(m_CurrentMultiplier, 1.0f, m_MaxIncrement );
	}

	public override void OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{
		base.OnUpdate( baseFireDispersion, baseCamDeviation );
		m_CurrentMultiplier += m_IncremetalSpeed;
		m_CurrentMultiplier = Mathf.Clamp(m_CurrentMultiplier, 1.0f, m_MaxIncrement );
	}

	public override void OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		base.OnEnd( baseFireDispersion, baseCamDeviation );
		m_CurrentMultiplier = 1.0f;
	}
}
