using UnityEngine;


public class WPN_FireModule_Shotgun : WPN_FireModule
{
//	[SerializeField]
//	protected			float			m_BasePerShotFireDispersion = 0.2f;

	public	override	EFireMode		FireMode => EFireMode.NONE;

	//////////////////////////////////////////////////////////////////////////
	protected override bool InternalSetup(Database.Section moduleSection)
	{
//		m_BasePerShotFireDispersion = moduleSection.AsFloat("BasePerShotFireDispersion", m_BasePerShotFireDispersion);
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
		float perShotDispersion = moduleFireDispersion / m_FireModuleData.BulletsPerShot;
		for (uint i = 0; i < m_FireModuleData.BulletsPerShot; i++)
		{
			InternalShoot(perShotDispersion);
		}

		EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 4);
		EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1);

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
	protected void InternalShoot(float moduleFireDispersion)
	{
		// BULLET
		IBullet bullet = m_PoolBullets.GetNextComponent();

		// POSITION
		Vector3 position = m_FirePoint.position;

		// DIRECTION
		Vector3 dispersionVector = new Vector3
		(
			Random.Range(-moduleFireDispersion, moduleFireDispersion),
			Random.Range(-moduleFireDispersion, moduleFireDispersion),
			Random.Range(-moduleFireDispersion, moduleFireDispersion)
		);

		Vector3 direction = (m_FirePoint.forward + dispersionVector).normalized;

		// SHOOT
		bullet.Shoot(origin: position, direction: direction, velocity: m_FireModuleData.BulletVelocity, impactForceMultiplier: m_FireModuleData.ImpactForceMultiplier);
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
