using UnityEngine;
using System.Collections.Generic;

public abstract partial class WPN_BaseModule : IModifiable
{
	private				WeaponModuleData			m_TmpWeaponModuleData		= new WeaponModuleData();

	[SerializeField, ReadOnly]
	protected			List<Database.Section>		m_Modifiers					= new List<Database.Section>();
	
	void IModifiable.StartModify								()								=> StartModify_Internal();
	void IModifiable.AddModifier								(Database.Section modifier)		=> AddModifier_Internal(modifier);
	void IModifiable.ResetBaseConfiguration						()								=> ResetBaseConfiguration_Internal();
	void IModifiable.RemoveModifier								(Database.Section modifier)		=> RemoveModifier_Internal(modifier);
	void IModifiable.EndModify									()								=> EndModify_Internal();

	protected virtual void StartModify_Internal					()
	{
		m_TmpWeaponModuleData = new WeaponModuleData();
		m_TmpWeaponModuleData.AssignFrom(m_WeaponModuleData);
	}
	protected virtual void AddModifier_Internal					(Database.Section modifier)		{ }
	protected virtual void ResetBaseConfiguration_Internal		()								{ }
	protected virtual void RemoveModifier_Internal				(Database.Section modifier)		{ }
	protected virtual void EndModify_Internal					()
	{
		m_WeaponModuleData.AssignFrom(m_TmpWeaponModuleData);
	}
}
