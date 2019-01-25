using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WPN_FireModule_Auto
[System.Serializable]
public class WPN_FireModule_Auto : WPN_FireModule {

	//
	public override FireModes FireMode
	{
		get { return FireModes.AUTO; }
	}
	
	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	void	OnWeaponChange	() { }

	protected	override	float	GetFireDispersion()
	{
		return m_BaseFireDispersion;
	}
	protected	override	float	GetCamDeviation()
	{
		return m_BaseCamDeviation;
	}


	public override bool CanBeUsed()
	{
		return base.CanBeUsed() && m_FireDelay <= 0f;
	}

	//	INTERNAL UPDATE
	public	override	void	InternalUpdate()
	{
		m_FireDelay -= Time.deltaTime;
	}

	//	START
	public override		void	OnStart()
	{
		if ( CanBeUsed() )
		{
			Shoot( GetFireDispersion(), GetCamDeviation() );
		}
	}

	//	INTERNAL UPDATE
	public	override	void	OnUpdate()
	{
		if ( CanBeUsed() )
		{
			Shoot( GetFireDispersion(), GetCamDeviation() );
		}
	}

	//	END
	public override		void	OnEnd()
	{
		if ( CanBeUsed() )
		{
			Shoot( GetFireDispersion(), GetCamDeviation() );
		}
	}

}
