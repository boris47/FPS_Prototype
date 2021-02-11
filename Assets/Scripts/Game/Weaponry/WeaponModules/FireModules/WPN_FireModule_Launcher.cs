﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WPN_FireModule_Launcher : WPN_FireModule {

	[SerializeField]
	protected	float	m_BaseLaunchForce = 20f;

	public override EFireMode FireMode
	{
		get {
			return EFireMode.NONE;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{
		m_BaseLaunchForce			= moduleSection.AsFloat( "BaseLaunchForce", m_BaseLaunchForce );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	ApplyModifier( Database.Section modifier )
	{
		// Do actions here

		float MultLaunchForce		= modifier.AsFloat( "MultLaunchForce",	1.0f );
		m_BaseLaunchForce			= m_BaseLaunchForce * MultLaunchForce;

		base.ApplyModifier( modifier );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	ResetBaseConfiguration()
	{
		// Do actions here

		base.ResetBaseConfiguration();
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	RemoveModifier( Database.Section modifier )
	{
		// Do Actions here

		base.RemoveModifier( modifier );
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	bool	OnSave			( StreamUnit streamUnit )
	{
		streamUnit.SetInternal(name, m_Magazine );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	bool	OnLoad			( StreamUnit streamUnit )
	{
		m_Magazine = (uint)streamUnit.GetAsInt(name );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	bool	NeedReload()
	{
		return m_Magazine == 0 || m_Magazine < m_MagazineCapacity;
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		override	void	OnAfterReload()
	{
		m_Magazine = m_MagazineCapacity;
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		virtual		void	OnLoad( uint magazine )
	{
		m_Magazine = magazine;
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		override	bool	CanBeUsed()
	{
		return m_Magazine > 0;
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Shoot( float moduleFireDispersion, float moduleCamDeviation )
	{
		//		m_FireDelay = m_BaseShotDelay;

		m_Magazine --;

		// TODO muzzle flash
//		EffectManager.Instance.PlayEffect( EffectType.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
//		EffectManager.Instance.PlayEffect( EffectType.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1 );

		// BULLET
		IBullet bullet = m_PoolBullets.GetNextComponent();

		moduleFireDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		moduleFireDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;
		moduleFireDispersion	*= bullet.RecoilMult;

		// SHOOT
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward, velocity: m_BaseLaunchForce );

		m_AudioSourceFire.Play();

		// CAM DEVIATION
		m_WeaponRef.ApplyDeviation( moduleCamDeviation );

		// CAM DISPERSION
		m_WeaponRef.ApplyDispersion( moduleFireDispersion );

		// CAM RECOIL
		m_WeaponRef.AddRecoil( m_Recoil );

		// UI ELEMENTS
		UIManager.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	bool	CanChangeWeapon()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void OnWeaponChange()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	void InternalUpdate( float DeltaTime )
	{
		m_WpnFireMode.InternalUpdate( DeltaTime, m_Magazine );
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		 override	void	OnStart()
	{
		if (CanBeUsed() )
		{
			m_WpnFireMode.OnStart(GetFireDispersion(), GetCamDeviation() );
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		override	void	OnUpdate()
	{
		if (CanBeUsed() )
		{
			m_WpnFireMode.OnUpdate(GetFireDispersion(), GetCamDeviation() );
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		override	void	OnEnd()
	{
		if (CanBeUsed() )
		{
			m_WpnFireMode.OnEnd(GetFireDispersion(), GetCamDeviation() );
		}
	}
	
}