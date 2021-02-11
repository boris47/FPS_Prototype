﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Burst
public class WPN_FireMode_Burst : WPN_BaseFireMode {
	
	// Burst
	protected		uint					m_BurstCount				= 0;
	protected		uint					m_BurstSize					= 0;

	// Deviation
	protected		bool					m_ApplyDeviation			= false;


	public override EFireMode FireMode
	{
		get {
			return EFireMode.BURST;
		}
	}


//	public	WPN_FireMode_Burst( Database.Section section ) { }


	protected	override	void	InternalSetup	(in Database.Section fireModeSection, in WPN_FireModule fireModule, in float shotDelay, in FireFunctionDel fireFunction)
	{
		m_BurstSize			= fireModeSection.AsUInt( "BurstSize", m_BurstSize );
		m_ApplyDeviation	= fireModeSection.AsBool( "ApplyDeviationOnLastShot", m_ApplyDeviation );
	}


	public	override	void	ApplyModifier	( Database.Section modifier )
	{
		m_Modifiers.Add( modifier );
	}

	public	override	void	ResetBaseConfiguration()
	{

	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		m_Modifiers.Remove( modifier );
	}


	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		return true;
	}


	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		m_CurrentDelay = 0.0f;
		m_BurstCount = 0;
		return true;
	}


	public	override	void	OnWeaponChange	()
	{
		m_CurrentDelay = 0.0f;
		m_BurstCount = 0;
	}


	//	INTERNAL UPDATE
	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{
		m_CurrentDelay = Mathf.Max( m_CurrentDelay - DeltaTime, 0.0f );
	}

	//	START
	public override		void	OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		if (m_CurrentDelay <= 0.0f && m_BurstCount < m_BurstSize )
		{
			m_FireFunction( baseFireDispersion * m_DispersionMultiplier * 0.5f, baseCamDeviation * m_DeviationMultiplier * 0.5f );
			m_BurstCount ++;
			m_CurrentDelay = m_FireDelay;
		}
	}

	//	INTERNAL UPDATE
	public	override	void	OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{
		if (m_CurrentDelay <= 0.0f && m_BurstCount < m_BurstSize )
		{
			m_FireFunction( baseFireDispersion * m_DispersionMultiplier, baseCamDeviation * m_DeviationMultiplier );
			m_BurstCount ++;
			m_CurrentDelay = m_FireDelay;
		}
	}

	//	END
	public override		void	OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		m_BurstCount = 0;
	}
	
}




//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_BurstAuto
public class WPN_FireMode_BurstAuto : WPN_FireMode_Burst {
	
	[SerializeField]
	private	bool	m_BurstActive = false;

	private		float	m_BaseFireDispersion	= 0.0f;
	private		float	m_BaseCamDeviation		= 0.0f;
	

//	public	WPN_FireMode_BurstAuto( Database.Section section ) : base( section ) { }

	public override		void	ApplyModifier( Database.Section modifier )
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


	private	void	StopAutoBurstSequence()
	{
		m_BurstCount = 0;
		m_BurstActive = false;
	}


	//	INTERNAL UPDATE
	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{
		m_CurrentDelay = Mathf.Max( m_CurrentDelay - DeltaTime, 0.0f );

		if (m_CurrentDelay <= 0.0f && m_BurstActive == true )
		{
			m_FireFunction(m_BaseFireDispersion, m_BaseCamDeviation );

			m_BurstCount ++;

			m_CurrentDelay = m_FireDelay;

			if (m_BurstCount >= m_BurstSize || magazineSize == 0 )
			{
				StopAutoBurstSequence();
			}
		}

		if (m_FireModule.Magazine <= 0 )
		{
			StopAutoBurstSequence();
		}
	}
	

	//	START
	public override void OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		if (m_CurrentDelay <= 0.0f && m_BurstCount < m_BurstSize )
		{
			float fireDispersion = ( !m_ApplyDeviation ) ? baseFireDispersion : 0.0f;
			m_FireFunction( fireDispersion, baseCamDeviation );

			m_BaseFireDispersion	= fireDispersion;
			m_BaseCamDeviation		= baseCamDeviation;

			m_BurstCount ++;
			m_BurstActive = true;
			m_CurrentDelay = m_FireDelay;
		}
	}

	//	INTERNAL UPDATE
	public override void OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{

	}

	//	END
	public override void OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		
	}
	
}