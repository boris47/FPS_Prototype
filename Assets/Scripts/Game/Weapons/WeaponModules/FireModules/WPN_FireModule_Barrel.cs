
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WPN_FireModule_Barrel : WPN_FireModule
{
	public override EFireMode FireMode
	{
		get => EFireMode.NONE;
	}

	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{ return true; }

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


	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		streamUnit.SetInternal(this.name, this.m_Magazine );
		return true;
	}

	//
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		this.m_Magazine = (uint)streamUnit.GetAsInt(this.name );
		return true;
	}


	//
	public	override	bool	NeedReload()
	{
		return this.m_Magazine == 0 || this.m_Magazine < this.m_MagazineCapacity;
	}

	//
	public		override	void	OnAfterReload()
	{
		this.m_Magazine = this.m_MagazineCapacity;
	}

	// ON LOAD
	public		virtual		void	OnLoad( uint magazine )
	{
		this.m_Magazine = magazine;
	}

	// CAN SHOOT
	public	override		bool	CanBeUsed()
	{
		return this.m_Magazine > 0;
	}

	/*
	//
	protected	abstract	float	GetFireDispersion();
	//
	protected	abstract	float	GetCamDeviation();
	*/
	// SHOOT
	protected	override		void	Shoot( float moduleFireDispersion, float moduleCamDeviation )
	{
		//		m_FireDelay = m_BaseShotDelay;

		this.m_Magazine --;

		// TODO muzzle flash
		EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.MUZZLE, this.m_FirePoint.position, this.m_FirePoint.forward, 1 );
		EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.SMOKE, this.m_FirePoint.position, this.m_FirePoint.forward, 1 );

		// BULLET
		IBullet bullet = this.m_PoolBullets.GetNextComponent();

		moduleFireDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		moduleFireDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;
		moduleFireDispersion	*= bullet.RecoilMult;

		// SHOOT
		bullet.Shoot( position: this.m_FirePoint.position, direction: this.m_FirePoint.forward );

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

	public override bool CanChangeWeapon()
	{
		return true;
	}

	public override void OnWeaponChange()
	{
		
	}

	public override void InternalUpdate( float DeltaTime )
	{
		this.m_WpnFireMode.InternalUpdate( DeltaTime, this.m_Magazine );
	}

	//    START
	public override        void    OnStart()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnStart(this.GetFireDispersion(), this.GetCamDeviation() );
		}
	}

	//    INTERNAL UPDATE
	public    override    void    OnUpdate()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnUpdate(this.GetFireDispersion(), this.GetCamDeviation() );
		}
	}

	//    END
	public override        void    OnEnd()
	{
		if (this.CanBeUsed() )
		{
			this.m_WpnFireMode.OnEnd(this.GetFireDispersion(), this.GetCamDeviation() );
		}
	}
}


[System.Serializable]
public class WPN_FireModule_SniperBarrel : WPN_FireModule_Barrel {
}