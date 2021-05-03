
using System.Collections.Generic;
using UnityEngine;

public abstract partial class WPN_FireModeBase : MonoBehaviour
{
	public delegate void FireFunctionDel(float baseFireDispersion, float baseCamDeviation);

	[System.Serializable]
	protected class FireModeData
	{
		[SerializeField, ReadOnly]
		public			float						DispersionMultiplier	= 1.0f;
		[SerializeField, ReadOnly]
		public			float						DeviationMultiplier		= 1.0f;

		public void AssignFrom(FireModeData other)
		{
			DispersionMultiplier = other.DispersionMultiplier;
			DeviationMultiplier = other.DeviationMultiplier;
		}
	}
	[SerializeField]
	protected			FireModeData						m_FireModeData				= new FireModeData();
	[SerializeField, ReadOnly]
	protected			WPN_FireModule						m_FireModule				= null;
	[SerializeField, ReadOnly]
	protected			WPN_FireModule.FireModuleData		m_FireModuleData			= null;
	[SerializeField, ReadOnly]
	protected			float								m_CurrentDelay				= 0.0f;
	[SerializeField, ReadOnly]
	protected			FireFunctionDel						m_FireFunction				= delegate { };


	public	abstract	EFireMode							FireMode					{ get; }

	//////////////////////////////////////////////////////////////////////////
	public void Setup(in WPN_FireModule fireModule, in WPN_FireModule.FireModuleData fireModuleData, in FireFunctionDel fireFunction)
	{
		if (CustomAssertions.IsNotNull(fireFunction))
		{
			m_FireModuleData = fireModuleData;
			m_FireFunction = fireFunction;
			m_FireModule = fireModule;
		}

		string moduleSectionName = GetType().Name;
		if (CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(moduleSectionName, out Database.Section fireModeSection)))
		{
			CustomAssertions.IsTrue(GlobalManager.Configs.TrySectionToOuter(fireModeSection, m_FireModeData));

			InternalSetup(fireModeSection, fireModule);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	protected abstract	void	InternalSetup		(in Database.Section fireModeSection, in WPN_FireModule fireModule);

	//////////////////////////////////////////////////////////////////////////
	public	abstract	bool	OnSave				(StreamUnit streamUnit);
	public	abstract	bool	OnLoad				(StreamUnit streamUnit);

	//////////////////////////////////////////////////////////////////////////
	public	abstract	void	OnWeaponChange		();

	//////////////////////////////////////////////////////////////////////////
	public	abstract	void	InternalUpdate		(float DeltaTime, uint magazineSize);

	//////////////////////////////////////////////////////////////////////////
	public	abstract	void	OnStart				(float baseFireDispersion, float baseCamDeviation);
	public	abstract	void	OnUpdate			(float baseFireDispersion, float baseCamDeviation);
	public	abstract	void	OnEnd				(float baseFireDispersion, float baseCamDeviation);

}
