using UnityEngine;
using System.Collections.Generic;

public abstract partial class WeaponBase : IModifiable
{
	protected		List<Database.Section>		m_Modifiers						= new List<Database.Section>();

	protected		WeaponData					m_TmpWeaponData					= new WeaponData();

	[System.Serializable]
	protected	class WeaponModifier
	{
		public float MultZoomFactor	= 1f;
		public float MultZoomingTime = 1f;
		public float MultZoomSensitivity = 1f;
	}

	//////////////////////////////////////////////////////////////////////////
	void IModifiable.StartModify()
	{
		m_TmpWeaponData = new WeaponData();
		m_TmpWeaponData.AssignFrom(m_WeaponData);
	}


	//////////////////////////////////////////////////////////////////////////
	void IModifiable.AddModifier(Database.Section modifierSection)
	{
		var modifier = new WeaponModifier();
		if (CustomAssertions.IsTrue(GlobalManager.Configs.TrySectionToOuter(modifierSection, modifier)))
		{
			m_TmpWeaponData.BaseZoomFactor				*= modifier.MultZoomFactor;
			m_TmpWeaponData.BaseZoomingTime				*= modifier.MultZoomingTime;
			m_TmpWeaponData.BaseZoomSensitivity			*= modifier.MultZoomSensitivity;
			m_Modifiers.Add(modifierSection);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void IModifiable.ResetBaseConfiguration()
	{
		// Reload Base Configuration
		RestoreBaseConfiguration();
	}


	//////////////////////////////////////////////////////////////////////////
	void IModifiable.RemoveModifier(Database.Section modifier)
	{
		if (m_Modifiers.Contains(modifier))
		{
			m_Modifiers.Remove(modifier);

		//	using (var modifiable = new Modifiable(this))
		//	{
		//		modifiable.ResetBaseConfiguration();
		//
		//		foreach (Database.Section otherModifier in m_Modifiers)
		//		{
		//			modifiable.AddModifier(otherModifier);
		//		}
		//	}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void IModifiable.EndModify()
	{
		m_WeaponData.AssignFrom(m_TmpWeaponData);
	}
}
