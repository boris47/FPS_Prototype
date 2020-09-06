using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Burst
public class WPN_FireMode_Burst : WPN_FireMode_Base {
	
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


	// Setup
	public	override void Setup( WPN_FireModule fireModule, float shotDelay, FireFunctionDel fireFunction )
	{
		if ( fireFunction != null )
		{
			this.m_FireDelay = shotDelay;
			this.m_FireFunction = fireFunction;
			this.m_FireModule = fireModule;
		}

		string moduleSectionName = this.GetType().Name;
		Database.Section section = null;
		if ( GlobalManager.Configs.GetSection( moduleSectionName, ref section ) )
		{
			this.m_BurstSize			= section.AsUInt( "BurstSize", this.m_BurstSize );
			this.m_ApplyDeviation	= section.AsBool( "ApplyDeviationOnLastShot", this.m_ApplyDeviation );
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
		this.m_BurstCount = 0;
		return true;
	}


	public	override	void	OnWeaponChange	()
	{
		this.m_CurrentDelay = 0.0f;
		this.m_BurstCount = 0;
	}


	//	INTERNAL UPDATE
	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{
		this.m_CurrentDelay -= DeltaTime;
	}

	//	START
	public override		void	OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		if (this.m_CurrentDelay <= 0.0f && this.m_BurstCount < this.m_BurstSize )
		{
			this.m_FireFunction( baseFireDispersion, baseCamDeviation );
			this.m_BurstCount ++;
			this.m_CurrentDelay = this.m_FireDelay;
		}
	}

	//	INTERNAL UPDATE
	public	override	void	OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{
		if (this.m_CurrentDelay <= 0.0f && this.m_BurstCount < this.m_BurstSize )
		{
			this.m_FireFunction( baseFireDispersion, baseCamDeviation );
			this.m_BurstCount ++;
			this.m_CurrentDelay = this.m_FireDelay;
		}
	}

	//	END
	public override		void	OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		this.m_BurstCount = 0;
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
		this.m_BurstCount = 0;
		this.m_BurstActive = false;
	}


	//	INTERNAL UPDATE
	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{
		this.m_CurrentDelay -= DeltaTime;

		if (this.m_CurrentDelay <= 0.0f && this.m_BurstActive == true )
		{
			this.m_FireFunction(this.m_BaseFireDispersion, this.m_BaseCamDeviation );

			this.m_BurstCount ++;

			this.m_CurrentDelay = this.m_FireDelay;

			if (this.m_BurstCount >= this.m_BurstSize || magazineSize == 0 )
			{
				this.StopAutoBurstSequence();
			}
		}

		if (this.m_FireModule.Magazine <= 0 )
		{
			this.StopAutoBurstSequence();
		}
	}
	

	//	START
	public override void OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		if (this.m_CurrentDelay <= 0.0f && this.m_BurstCount < this.m_BurstSize )
		{
			float fireDispersion = ( !this.m_ApplyDeviation ) ? baseFireDispersion : 0.0f;
			this.m_FireFunction( fireDispersion, baseCamDeviation );

			this.m_BaseFireDispersion	= fireDispersion;
			this.m_BaseCamDeviation		= baseCamDeviation;

			this.m_BurstCount ++;
			this.m_BurstActive = true;
			this.m_CurrentDelay = this.m_FireDelay;
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