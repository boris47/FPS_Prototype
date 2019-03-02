using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Single
public class WPN_FireMode_Single : WPN_FireMode_Base {

	public override FireModes FireMode
	{
		get {
			return FireModes.SINGLE;
		}
	}

	public	WPN_FireMode_Single( Database.Section section )
	{

	}


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
		
	}

	//	END
	public override		void	OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		
	}
}
