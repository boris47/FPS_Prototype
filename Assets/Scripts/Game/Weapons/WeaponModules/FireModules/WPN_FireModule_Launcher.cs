using System.Collections;
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
		this.m_BaseLaunchForce			= moduleSection.AsFloat( "BaseLaunchForce", this.m_BaseLaunchForce );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	void	ApplyModifier( Database.Section modifier )
	{
		// Do actions here

		float MultLaunchForce		= modifier.AsFloat( "MultLaunchForce",	1.0f );
		this.m_BaseLaunchForce			= this.m_BaseLaunchForce * MultLaunchForce;

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
		streamUnit.SetInternal(this.name, this.m_Magazine );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	bool	OnLoad			( StreamUnit streamUnit )
	{
		this.m_Magazine = (uint)streamUnit.GetAsInt(this.name );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public		override	bool	NeedReload()
	{
		return this.m_Magazine == 0 || this.m_Magazine < this.m_MagazineCapacity;
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		override	void	OnAfterReload()
	{
		this.m_Magazine = this.m_MagazineCapacity;
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		virtual		void	OnLoad( uint magazine )
	{
		this.m_Magazine = magazine;
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		override	bool	CanBeUsed()
	{
		return this.m_Magazine > 0;
	}

	
	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Shoot( float moduleFireDispersion, float moduleCamDeviation )
	{
		//		m_FireDelay = m_BaseShotDelay;

		this.m_Magazine --;

		// TODO muzzle flash
//		EffectManager.Instance.PlayEffect( EffectType.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
//		EffectManager.Instance.PlayEffect( EffectType.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1 );

		// BULLET
		IBullet bullet = this.m_PoolBullets.GetNextComponent();

		moduleFireDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		moduleFireDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;
		moduleFireDispersion	*= bullet.RecoilMult;

		// SHOOT
		bullet.Shoot( position: this.m_FirePoint.position, direction: this.m_FirePoint.forward, velocity: this.m_BaseLaunchForce );

		this.m_AudioSourceFire.Play();

		// CAM DEVIATION
		CameraControl.Instance.ApplyDeviation( moduleCamDeviation );

		// CAM DISPERSION
		CameraControl.Instance.ApplyDispersion( moduleFireDispersion );

		// CAM RECOIL
		CameraControl.Instance.AddRecoil(this.m_Recoil );

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
	public		override	void InternalUpdate( float DeltaTime )
	{
		this.m_WpnFireMode.InternalUpdate( DeltaTime, this.m_Magazine );
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		 override	void	OnStart()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnStart(this.GetFireDispersion(), this.GetCamDeviation() );
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		override	void	OnUpdate()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnUpdate(this.GetFireDispersion(), this.GetCamDeviation() );
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	public		override	void	OnEnd()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnEnd(this.GetFireDispersion(), this.GetCamDeviation() );
		}
	}
	
}
