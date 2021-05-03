using UnityEngine;

[System.Serializable]
public class WPN_FireMode_Auto : WPN_FireModeBase
{
	public	override	EFireMode				FireMode					=> EFireMode.AUTO;

	protected override void InternalSetup(in Database.Section fireModeSection, in WPN_FireModule fireModule)
	{

	}

	public override bool OnSave(StreamUnit streamUnit)
	{
		return true;
	}

	public override bool OnLoad(StreamUnit streamUnit)
	{
		m_CurrentDelay = 0.0f;
		return true;
	}

	public override void OnWeaponChange()
	{
		m_CurrentDelay = 0.0f;
	}

	//	INTERNAL UPDATE
	public override void InternalUpdate(float DeltaTime, uint magazineSize)
	{
		m_CurrentDelay = Mathf.Max(m_CurrentDelay - DeltaTime, 0.0f);
	}

	//	START
	public override void OnStart(float baseFireDispersion, float baseCamDeviation)
	{
		if (m_CurrentDelay <= 0.0f)
		{
			m_FireFunction(baseFireDispersion * m_FireModeData.DispersionMultiplier * 0.5f, baseCamDeviation * m_FireModeData.DeviationMultiplier * 0.5f);
			m_CurrentDelay = m_FireModuleData.ShotDelay;
		}
	}

	//	INTERNAL UPDATE
	public override void OnUpdate(float baseFireDispersion, float baseCamDeviation)
	{
		if (m_CurrentDelay <= 0.0f)
		{
			m_FireFunction(baseFireDispersion * m_FireModeData.DispersionMultiplier, baseCamDeviation * m_FireModeData.DeviationMultiplier);
			m_CurrentDelay = m_FireModuleData.ShotDelay;
		}
	}

	//	END
	public override void OnEnd(float baseFireDispersion, float baseCamDeviation)
	{
		if (m_CurrentDelay <= 0.0f)
		{
			m_FireFunction(baseFireDispersion * m_FireModeData.DispersionMultiplier, baseCamDeviation * m_FireModeData.DeviationMultiplier);
			m_CurrentDelay = m_FireModuleData.ShotDelay;
		}
	}
}



public class WPN_FireMode_Auto_Incremental : WPN_FireMode_Auto
{
	[SerializeField, ReadOnly]
	private				float					m_IncremetalSpeed			= 1.0f;
	[SerializeField, ReadOnly]
	private				float					m_MaxIncrement				= 2.0f;
	[SerializeField, ReadOnly]
	private				float					m_CurrentMultiplier			= 1.0f;


	protected override void InternalSetup(in Database.Section fireModeSection, in WPN_FireModule fireModule)
	{
		CustomAssertions.IsTrue(fireModeSection.TryAsFloat("IncremetalSpeed", out m_IncremetalSpeed));
		CustomAssertions.IsTrue(fireModeSection.TryAsFloat("MaxIncrement", out m_MaxIncrement));
	}

	public override bool OnLoad(StreamUnit streamUnit)
	{
		m_CurrentMultiplier = 1.0f;

		return base.OnLoad(streamUnit);
	}

	//	INTERNAL UPDATE
	public override void InternalUpdate(float DeltaTime, uint magazineSize)
	{
		m_CurrentDelay = Mathf.Max(m_CurrentDelay - (DeltaTime * m_CurrentMultiplier), 0.0f);
	}

	public override void OnStart(float baseFireDispersion, float baseCamDeviation)
	{
		base.OnStart(baseFireDispersion, baseCamDeviation);
		m_CurrentMultiplier += m_IncremetalSpeed;
		m_CurrentMultiplier = Mathf.Clamp(m_CurrentMultiplier, 1.0f, m_MaxIncrement);
	}

	public override void OnUpdate(float baseFireDispersion, float baseCamDeviation)
	{
		base.OnUpdate(baseFireDispersion, baseCamDeviation);
		m_CurrentMultiplier += m_IncremetalSpeed;
		m_CurrentMultiplier = Mathf.Clamp(m_CurrentMultiplier, 1.0f, m_MaxIncrement);
	}

	public override void OnEnd(float baseFireDispersion, float baseCamDeviation)
	{
		base.OnEnd(baseFireDispersion, baseCamDeviation);
		m_CurrentMultiplier = 1.0f;
	}
}
