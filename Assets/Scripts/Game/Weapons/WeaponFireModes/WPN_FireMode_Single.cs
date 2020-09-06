using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Single
public class WPN_FireMode_Single : WPN_FireMode_Base {

	public override EFireMode FireMode
	{
		get {
			return EFireMode.SINGLE;
		}
	}

//	public	WPN_FireMode_Single( Database.Section section ) { }


	public	override	void	Setup			( WPN_FireModule fireModule, float shotDelay, FireFunctionDel fireFunction )
	{
		if ( fireFunction != null )
		{
			this.m_FireFunction = fireFunction;
			this.m_FireDelay = shotDelay;
			this.m_FireModule = fireModule;
		}
	}
	

	public	override	void	ApplyModifier	( Database.Section modifier )
	{
		this.m_Modifiers.Add( modifier );
	}

	public	override	void	ResetBaseConfiguration()
	{

	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		this.m_Modifiers.Remove( modifier );
	}


	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		return true;
	}


	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		this.m_CurrentDelay = 0.0f;
		return true;
	}

	public	override	void	OnWeaponChange	()
	{
		this.m_CurrentDelay = 0.0f;
	}
	

	//	INTERNAL UPDATE
	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{
		this.m_CurrentDelay -= DeltaTime;
	}

	//	START
	public override		void	OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		if (this.m_CurrentDelay <= 0.0f )
		{
			this.m_FireFunction( baseFireDispersion, baseCamDeviation );
			this.m_CurrentDelay = this.m_FireDelay;
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
