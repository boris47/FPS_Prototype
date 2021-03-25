using UnityEngine;


public class WPN_FireModule_Shotgun : WPN_FireModule
{
	[SerializeField]
	protected			float			m_BasePerShotFireDispersion = 0.2f;

	public	override	EFireMode		FireMode => EFireMode.NONE;

	//////////////////////////////////////////////////////////////////////////
	protected override bool InternalSetup(Database.Section moduleSection)
	{
		m_BasePerShotFireDispersion = moduleSection.AsFloat("BasePerShotFireDispersion", m_BasePerShotFireDispersion);
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	public override void ApplyModifier(Database.Section modifier)
	{
		base.ApplyModifier(modifier);

		float MultPerShotFireDispersion = modifier.AsFloat("MultPerShotFireDispersion", 1.0f);
		float MultBucketSize = modifier.AsFloat("MultBucketSize", 1.0f);

		m_BasePerShotFireDispersion = m_BasePerShotFireDispersion * MultPerShotFireDispersion;
		m_BaseBulletsPerShot = (uint)((float)m_BaseBulletsPerShot * MultBucketSize);
	}

	//////////////////////////////////////////////////////////////////////////
	public override void ResetBaseConfiguration()
	{
		base.ResetBaseConfiguration();

		// Do actions here
	}

	//////////////////////////////////////////////////////////////////////////
	public override void RemoveModifier(Database.Section modifier)
	{
		base.RemoveModifier(modifier);

		// Do Actions here
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
		return m_Magazine == 0 || m_Magazine < m_MagazineCapacity;
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnAfterReload()
	{
		m_Magazine = m_MagazineCapacity;
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
		for (uint i = 0; i < m_BaseBulletsPerShot; i++)
		{
			InternalShoot(moduleFireDispersion, moduleCamDeviation);
		}

		EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.MUZZLE, m_FirePoint.position, m_FirePoint.forward, 4);
		EffectsManager.Instance.PlayEffect(EffectsManager.EEffecs.SMOKE, m_FirePoint.position, m_FirePoint.forward, 1);

		m_AudioSourceFire.Play();

		// CAM DEVIATION
		m_WeaponRef.ApplyDeviation(moduleCamDeviation);

		// CAM DISPERSION
		m_WeaponRef.ApplyDispersion(moduleFireDispersion);

		// CAM RECOIL
		m_WeaponRef.AddRecoil(m_Recoil);

		// UI ELEMENTS
		UIManager.InGame.UpdateUI();
	}

	//////////////////////////////////////////////////////////////////////////
	protected void InternalShoot(float moduleFireDispersion, float moduleCamDeviation)
	{
		// BULLET
		IBullet bullet = m_PoolBullets.GetNextComponent();

		// POSITION
		Vector3 position = m_FirePoint.position;

		// DIRECTION
		Vector3 dispersionVector = new Vector3
		(
			Random.Range(-m_BasePerShotFireDispersion, m_BasePerShotFireDispersion),
			Random.Range(-m_BasePerShotFireDispersion, m_BasePerShotFireDispersion),
			Random.Range(-m_BasePerShotFireDispersion, m_BasePerShotFireDispersion)
		);

		Vector3 direction = (m_FirePoint.forward + dispersionVector).normalized;

		// SHOOT
		bullet.Shoot(position: position, direction: direction, velocity: null, impactForceMultiplier: null);
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
	protected override void InternalUpdate(float DeltaTime)
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
