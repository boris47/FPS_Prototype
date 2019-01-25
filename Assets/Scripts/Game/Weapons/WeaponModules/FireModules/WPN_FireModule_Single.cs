using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
// WPN_FireModule_SingleShot
[System.Serializable]
public class WPN_FireModule_Single : WPN_FireModule {

	//
	public override FireModes FireMode
	{
		get { return FireModes.SINGLE; }
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
			Shoot( m_BaseFireDispersion, m_BaseCamDeviation );
		}
	}

	//	INTERNAL UPDATE
	public	override	void	OnUpdate()
	{

	}

	//	END
	public override		void	OnEnd()
	{

	}
	
}
