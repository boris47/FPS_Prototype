
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WPN_FireModule_Barrel : WPN_FireModule
{
	public override EFireMode FireMode => EFireMode.NONE;

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
		streamUnit.SetInternal(name, m_Magazine );
		return true;
	}

	//
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		m_Magazine = (uint)streamUnit.GetAsInt(name );
		return true;
	}


	//
	public	override	bool	NeedReload()
	{
		return m_Magazine == 0 || m_Magazine < m_MagazineCapacity;
	}

	//
	public		override	void	OnAfterReload()
	{
		m_Magazine = m_MagazineCapacity;
	}

	// ON LOAD
	public		virtual		void	OnLoad( uint magazine )
	{
		m_Magazine = magazine;
	}

	// CAN SHOOT
	public	override		bool	CanBeUsed()
	{
		return m_Magazine > 0;
	}
	
	// SHOOT
	protected	override		void	Shoot( float moduleFireDispersion, float moduleCamDeviation )
	{
		--m_Magazine;

		// TODO muzzle flash
		EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
		EffectsManager.Instance.PlayEffect( EffectsManager.EEffecs.SMOKE,  m_FirePoint.position, m_FirePoint.forward, 1 );

		// BULLET
		IBullet bullet = m_PoolBullets.GetNextComponent();

		moduleFireDispersion	*= Player.Instance.Motion.MotionStrategy.States.IsCrouched		? 0.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.Motion.MotionStrategy.States.IsMoving		? 1.50f : 1.00f;
		moduleFireDispersion	*= Player.Instance.Motion.MotionStrategy.States.IsRunning		? 2.00f : 1.00f;
		moduleFireDispersion	*= WeaponManager.Instance.IsZoomed		? 0.80f : 1.00f;
		moduleFireDispersion	*= bullet.RecoilMult;

		// SHOOT
		bullet.Shoot( position: m_FirePoint.position, direction: m_FirePoint.forward, velocity: null );

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

	public override bool CanChangeWeapon()
	{
		return true;
	}

	public override void OnWeaponChange()
	{
		
	}

	protected override void InternalUpdate( float DeltaTime )
	{
		m_WpnFireMode.InternalUpdate( DeltaTime, m_Magazine );
	}

	//    START
	public override        void    OnStart()
	{
		if (CanBeUsed() )
		{
			m_WpnFireMode.OnStart(GetFireDispersion(), GetCamDeviation() );
		}
	}

	//    INTERNAL UPDATE
	public    override    void    OnUpdate()
	{
		if (CanBeUsed() )
		{
			m_WpnFireMode.OnUpdate(GetFireDispersion(), GetCamDeviation() );
		}
	}

	//    END
	public override        void    OnEnd()
	{
		if (CanBeUsed() )
		{
			m_WpnFireMode.OnEnd(GetFireDispersion(), GetCamDeviation() );
		}
	}
}


[System.Serializable]
public class WPN_FireModule_SniperBarrel : WPN_FireModule_Barrel
{

}