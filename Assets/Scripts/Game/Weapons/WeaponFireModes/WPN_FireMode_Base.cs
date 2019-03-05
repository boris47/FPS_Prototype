﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public	abstract	class	WPN_FireMode_Base : IModifiable {
	
	public delegate	void FireFunctionDel( float baseFireDispersion, float baseCamDeviation );

	public	abstract	FireModes	FireMode
	{
		get;
	}

	protected			WPN_FireModule				m_FireModule				= null;
	protected			List<Database.Section>		m_Modifiers					= new List<Database.Section>();
	protected			float						m_FireDelay					= 1.0f;
	protected			float						m_CurrentDelay				= 0.0f;


	protected			FireFunctionDel				m_FireFunction				= delegate { };

	public	abstract	void	Setup				( WPN_FireModule fireModule, float shotDelay, FireFunctionDel fireFunction );

	public	abstract	void	ApplyModifier		( Database.Section modifier );
	public	abstract	void	ResetBaseConfiguration	();
	public	abstract	void	RemoveModifier		( Database.Section modifier );

	public	abstract	bool	OnSave				( StreamUnit streamUnit );
	public	abstract	bool	OnLoad				( StreamUnit streamUnit );

	public	abstract	void	OnWeaponChange		();

	public	abstract	void	InternalUpdate		( float DeltaTime, uint magazineSize );

	//
	public	abstract	void	OnStart				( float baseFireDispersion, float baseCamDeviation );
	public	abstract	void	OnUpdate			( float baseFireDispersion, float baseCamDeviation );
	public	abstract	void	OnEnd				( float baseFireDispersion, float baseCamDeviation );

}

public	class WPN_FireMode_Empty : WPN_FireMode_Base {

	public override FireModes FireMode
	{
		get {
			return FireModes.NONE;
		}
	}

	public	WPN_FireMode_Empty( Database.Section section )
	{ }

	public	override	void	Setup			( WPN_FireModule fireModule, float shotDelay, FireFunctionDel fireFunction )
	{ }

	public	override	void	ApplyModifier	( Database.Section modifier )
	{ }

	public	override	void	ResetBaseConfiguration()
	{ }

	public	override	void	RemoveModifier( Database.Section modifier )
	{ }

	public	override	bool	OnSave			( StreamUnit streamUnit )
	{ return true; }

	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{ return true; }

	public	override	void	OnWeaponChange	()
	{ }

	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{ }

	public override		void	OnStart( float baseFireDispersion, float baseCamDeviation )
	{ }

	public	override	void	OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{ }

	public override		void	OnEnd( float baseFireDispersion, float baseCamDeviation )
	{ }
}