using UnityEngine;
using System.Collections.Generic;

public abstract partial class WPN_FireModeBase : IModifiable
{
	protected			FireModeData				m_TmpFireModeData			= new FireModeData();

	[SerializeField, ReadOnly]
	protected			List<Database.Section>		m_Modifiers					= new List<Database.Section>();

	void IModifiable.StartModify								()								=> StartModify_Internal();
	void IModifiable.AddModifier								(Database.Section modifier)		=> ApplyModifier_Internal(modifier);
	void IModifiable.ResetBaseConfiguration						()								=> ResetBaseConfiguration_Internal();
	void IModifiable.RemoveModifier								(Database.Section modifier)		=> RemoveModifier_Internal(modifier);
	void IModifiable.EndModify									()								=> EndModify_Internal();

	protected virtual void StartModify_Internal					()
	{
		m_TmpFireModeData = new FireModeData();
		m_TmpFireModeData.AssignFrom(m_FireModeData);
	}
	protected virtual void ApplyModifier_Internal				(Database.Section modifier)		{ }
	protected virtual void ResetBaseConfiguration_Internal		()								{ }
	protected virtual void RemoveModifier_Internal				(Database.Section modifier)		{ }
	protected virtual void EndModify_Internal					()
	{
		m_FireModeData.AssignFrom(m_TmpFireModeData);
	}
}

