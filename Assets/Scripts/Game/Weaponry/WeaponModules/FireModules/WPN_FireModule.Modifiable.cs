
using System.Collections.Generic;
using UnityEngine;

public abstract partial class WPN_FireModule
{
	private				FireModuleData						m_TmpFireModuleData				= new FireModuleData();

	[System.Serializable]
	private class WeaponFireModuleModifier
	{
		public	float	MultMagazineCapacity		= 1.0f;
		public	float	MultBulletsPerShot			= 1.0f;
		public	float	MultBulletDamage			= 1.0f;
		public	float	MultBulletVelocity			= 1.0f;
		public	float	MultImpactForceMultiplier	= 1.0f;
		public	float	MultCamDeviation			= 1.0f;
		public	float	MultFireDispersion			= 1.0f;
		public	float	MultRecoil					= 1.0f;
		public	float	MultShotDelay				= 1.0f;

		public	string	FireMode					= string.Empty;
		public	string	BulletSection				= string.Empty;
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void StartModify_Internal()
	{
		base.StartModify_Internal();

		m_TmpFireModuleData = new FireModuleData();
		m_TmpFireModuleData.AssignFrom(m_FireModuleData);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void AddModifier_Internal(Database.Section modifierSection)
	{
		var modifier = new WeaponFireModuleModifier();
		if (CustomAssertions.IsTrue(GlobalManager.Configs.TrySectionToOuter(modifierSection, modifier)))
		{
			m_TmpFireModuleData.MagazineCapacity = (uint)((float)m_TmpFireModuleData.MagazineCapacity * modifier.MultMagazineCapacity);

			m_TmpFireModuleData.BulletsPerShot = (uint)((float)m_TmpFireModuleData.BulletsPerShot * modifier.MultBulletsPerShot);
			m_TmpFireModuleData.BulletDamage *= modifier.MultBulletDamage;
			m_TmpFireModuleData.BulletVelocity *= modifier.MultBulletVelocity;
			m_TmpFireModuleData.ImpactForceMultiplier *= modifier.MultImpactForceMultiplier;
			m_TmpFireModuleData.CamDeviation *= modifier.MultCamDeviation;
			m_TmpFireModuleData.FireDispersion *= modifier.MultFireDispersion;
			m_TmpFireModuleData.Recoil *= modifier.MultRecoil;
			m_TmpFireModuleData.ShotDelay *= modifier.MultShotDelay;

			m_TmpFireModuleData.FireMode = modifier.FireMode.IsNone() ? m_TmpFireModuleData.FireMode : modifier.FireMode;
			m_TmpFireModuleData.BulletSection = modifier.BulletSection.IsNone() ? m_TmpFireModuleData.BulletSection : modifier.BulletSection;
			m_Modifiers.Add(modifierSection);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void ResetBaseConfiguration_Internal()
	{
		base.ResetBaseConfiguration_Internal();

		// Reset everything of this module
		m_Modifiers.Clear();
		OnDetach();
		OnAttach(m_WeaponRef, m_ModuleSlot);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void RemoveModifier_Internal(Database.Section modifier)
	{
		base.RemoveModifier_Internal(modifier);

		int indexOfModifier = m_Modifiers.IndexOf(modifier);
		if (indexOfModifier >= 0)
		{
			m_Modifiers.RemoveAt(indexOfModifier);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void EndModify_Internal()
	{
		base.EndModify_Internal();

		m_FireModuleData.AssignFrom(m_TmpFireModuleData);

		// MAGAZINE
		m_PoolBullets.Resize(m_Magazine = m_FireModuleData.MagazineCapacity);

		// DAMAGE
		m_PoolBullets.ExecuteActionOnObjects(ActionOnBullet);

		// FIRE MODE
		if (TryChangeFireMode(m_FireModuleData.FireMode))
		{
			m_WpnFireMode.Setup(this, m_FireModuleData, Shoot);
		}

		// BULLET
		string bulletSectionName = m_FireModuleData.BulletSection;
		if (bulletSectionName != m_PoolBullets.PeekComponent().GetType().Name)
		{
			Bullet.GetBulletModel(bulletSectionName, out GameObject model);
			m_PoolBullets.Convert(model, ActionOnBullet);
		}
		m_UI_Crosshair.SetMin(m_FireModuleData.FireDispersion);
	}
}
