
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Auto
[System.Serializable]
public class WPN_FireMode_Auto : WPN_FireMode_Base {

	public override FireModes FireMode
	{
		get {
			return FireModes.AUTO;
		}
	}

//	public	WPN_FireMode_Auto( Database.Section section ) { }

	public	override	void	Setup			( WPN_FireModule fireModule, float shotDelay, FireFunctionDel fireFunction )
	{
		if ( fireFunction != null )
		{
			m_FireFunction = fireFunction;
			m_FireDelay = shotDelay;
			m_FireModule = fireModule;
		}
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
		m_CurrentDelay -= DeltaTime;
	}

	//	START
	public override		void	OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		if ( m_CurrentDelay <= 0.0f )
		{
			m_FireFunction( baseFireDispersion, baseCamDeviation );
			m_CurrentDelay = m_FireDelay;
		}
	}

	//	INTERNAL UPDATE
	public	override	void	OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{
		if ( m_CurrentDelay <= 0.0f )
		{
			m_FireFunction( baseFireDispersion, baseCamDeviation );
			m_CurrentDelay = m_FireDelay;
		}
	}

	//	END
	public override		void	OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		if ( m_CurrentDelay <= 0.0f )
		{
			m_FireFunction( baseFireDispersion, baseCamDeviation );
			m_CurrentDelay = m_FireDelay;
		}
	}
}



//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Auto_Incremental
public class WPN_FireMode_Auto_Incremental : WPN_FireMode_Auto {

	private		float	m_IncremetalSpeed			= 1.0f;
	private		float	m_MaxIncrement				= 2.0f;

	private		float	m_CurrentMultiplier			= 1.0f;


//	public WPN_FireMode_Auto_Incremental( Database.Section section ) : base( section ) { }

	public override void Setup( WPN_FireModule fireModule, float shotDelay, FireFunctionDel fireFunction )
	{
		base.Setup( fireModule, shotDelay, fireFunction );

		string moduleSectionName = this.GetType().Name;
		Database.Section section = null;
		if ( GameManager.Configs.bGetSection( moduleSectionName, ref section ) )
		{
			m_IncremetalSpeed	= section.AsFloat( "IncremetalSpeed", m_IncremetalSpeed );
			m_MaxIncrement		= section.AsFloat( "MaxIncrement", m_MaxIncrement );
		}
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
		m_CurrentDelay -= DeltaTime * m_CurrentMultiplier;
	}

	public override void OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		base.OnStart( baseFireDispersion, baseCamDeviation );
		m_CurrentMultiplier += m_IncremetalSpeed;
		m_CurrentMultiplier = Mathf.Clamp( m_CurrentMultiplier, 1.0f, m_MaxIncrement );
	}

	public override void OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{
		base.OnUpdate( baseFireDispersion, baseCamDeviation );
		m_CurrentMultiplier += m_IncremetalSpeed;
		m_CurrentMultiplier = Mathf.Clamp( m_CurrentMultiplier, 1.0f, m_MaxIncrement );
	}

	public override void OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		base.OnEnd( baseFireDispersion, baseCamDeviation );
		m_CurrentMultiplier = 1.0f;
	}
}
