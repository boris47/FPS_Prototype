using UnityEngine;

public class WPN_FireModule_Launcher : WPN_FireModule
{
	public	override	EFireMode		FireMode			=> EFireMode.NONE;


	//////////////////////////////////////////////////////////////////////////
	protected override bool InternalSetup(Database.Section moduleSection)
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool OnSave(StreamUnit streamUnit)
	{
		streamUnit.SetInternal(name, m_Magazine);
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool OnLoad(StreamUnit streamUnit)
	{
		m_Magazine = (uint)streamUnit.GetAsInt(name);
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool NeedReload()
	{
		return m_Magazine == 0 || m_Magazine < m_FireModuleData.MagazineCapacity;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnAfterReload()
	{
		m_Magazine = m_FireModuleData.MagazineCapacity;
	}


	//////////////////////////////////////////////////////////////////////////
	public virtual void OnLoad(uint magazine)
	{
		m_Magazine = magazine;
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool CanBeUsed()
	{
		return m_Magazine > 0;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void Shoot(float moduleFireDispersion, float moduleCamDeviation)
	{
		m_Magazine--;

		// TODO muzzle flash
		//EffectManager.Instance.PlayEffect( EffectType.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 1 );
		//EffectManager.Instance.PlayEffect( EffectType.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1 );

		// BULLET
		IBullet bullet = m_PoolBullets.GetNextComponent();

		moduleFireDispersion *= m_WeaponRef.Owner.Motion.MotionStrategy.States.IsCrouched ? 0.50f : 1.00f;
		moduleFireDispersion *= m_WeaponRef.Owner.Motion.MotionStrategy.States.IsMoving ? 1.50f : 1.00f;
		moduleFireDispersion *= m_WeaponRef.Owner.Motion.MotionStrategy.States.IsRunning ? 2.00f : 1.00f;
		moduleFireDispersion *= WeaponManager.Instance.IsZoomed ? 0.80f : 1.00f;
		moduleFireDispersion *= bullet.RecoilMult;

		// SHOOT
		bullet.Shoot(origin: m_FirePoint.position, direction: m_FirePoint.forward, velocity: m_FireModuleData.BulletVelocity, impactForceMultiplier: m_FireModuleData.ImpactForceMultiplier);

		m_AudioSourceFire.Play();

		// WEAPON DEVIATION
		WeaponPivot.Instance.ApplyDeviation(moduleCamDeviation);

		// CAM DISPERSION
		WeaponPivot.Instance.ApplyDispersion(moduleFireDispersion);

		// CAM RECOIL
		WeaponPivot.Instance.AddRecoil(m_FireModuleData.Recoil);

		// UI ELEMENTS
		UIManager.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	public override bool CanChangeWeapon()
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnWeaponChange()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame(float DeltaTime)
	{
		m_WpnFireMode.InternalUpdate(DeltaTime, m_Magazine);
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnStart()
	{
		if (CanBeUsed())
		{
			m_WpnFireMode.OnStart(GetFireDispersion(), GetCamDeviation());
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnUpdate()
	{
		if (CanBeUsed())
		{
			m_WpnFireMode.OnUpdate(GetFireDispersion(), GetCamDeviation());
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnEnd()
	{
		if (CanBeUsed())
		{
			m_WpnFireMode.OnEnd(GetFireDispersion(), GetCamDeviation());
		}
	}
}
