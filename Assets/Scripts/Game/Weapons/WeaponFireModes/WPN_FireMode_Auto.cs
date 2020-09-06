
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Auto
[System.Serializable]
public class WPN_FireMode_Auto : WPN_FireMode_Base {

	public override EFireMode FireMode
	{
		get {
			return EFireMode.AUTO;
		}
	}

//	public	WPN_FireMode_Auto( Database.Section section ) { }

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
		if (this.m_CurrentDelay <= 0.0f )
		{
			this.m_FireFunction( baseFireDispersion, baseCamDeviation );
			this.m_CurrentDelay = this.m_FireDelay;
		}
	}

	//	END
	public override		void	OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		if (this.m_CurrentDelay <= 0.0f )
		{
			this.m_FireFunction( baseFireDispersion, baseCamDeviation );
			this.m_CurrentDelay = this.m_FireDelay;
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
		if ( GlobalManager.Configs.GetSection( moduleSectionName, ref section ) )
		{
			this.m_IncremetalSpeed	= section.AsFloat( "IncremetalSpeed", this.m_IncremetalSpeed );
			this.m_MaxIncrement		= section.AsFloat( "MaxIncrement", this.m_MaxIncrement );
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
		this.m_CurrentMultiplier = 1.0f;

		return base.OnLoad( streamUnit );
	}

	//	INTERNAL UPDATE
	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{
		this.m_CurrentDelay -= DeltaTime * this.m_CurrentMultiplier;
	}

	public override void OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		base.OnStart( baseFireDispersion, baseCamDeviation );
		this.m_CurrentMultiplier += this.m_IncremetalSpeed;
		this.m_CurrentMultiplier = Mathf.Clamp(this.m_CurrentMultiplier, 1.0f, this.m_MaxIncrement );
	}

	public override void OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{
		base.OnUpdate( baseFireDispersion, baseCamDeviation );
		this.m_CurrentMultiplier += this.m_IncremetalSpeed;
		this.m_CurrentMultiplier = Mathf.Clamp(this.m_CurrentMultiplier, 1.0f, this.m_MaxIncrement );
	}

	public override void OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		base.OnEnd( baseFireDispersion, baseCamDeviation );
		this.m_CurrentMultiplier = 1.0f;
	}
}
