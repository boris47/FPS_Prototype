using UnityEngine;

public class WPN_FireMode_Burst : WPN_FireModeBase
{	
	protected			uint					m_BurstCount				= 0;
	protected			uint					m_BurstSize					= 0;
	protected			bool					m_ApplyDeviation			= false;

	public	override	EFireMode				FireMode					=> EFireMode.BURST;

	protected override void InternalSetup(in Database.Section fireModeSection, in WPN_FireModule fireModule)
	{
		CustomAssertions.IsTrue(fireModeSection.TryAsUInt("BurstSize", out m_BurstSize));
		CustomAssertions.IsTrue(fireModeSection.TryAsBool("ApplyDeviationOnLastShot", out m_ApplyDeviation));
	}

	public override bool OnSave(StreamUnit streamUnit)
	{
		return true;
	}

	public override bool OnLoad(StreamUnit streamUnit)
	{
		m_CurrentDelay = 0.0f;
		m_BurstCount = 0;
		return true;
	}

	public override void OnWeaponChange()
	{
		m_CurrentDelay = 0.0f;
		m_BurstCount = 0;
	}

	//	INTERNAL UPDATE
	public override void InternalUpdate(float DeltaTime, uint magazineSize)
	{
		m_CurrentDelay = Mathf.Max(m_CurrentDelay - DeltaTime, 0.0f);
	}

	//	START
	public override void OnStart(float baseFireDispersion, float baseCamDeviation)
	{
		if (m_CurrentDelay <= 0.0f && m_BurstCount < m_BurstSize)
		{
			m_FireFunction(baseFireDispersion * m_FireModeData.DispersionMultiplier * 0.5f, baseCamDeviation * m_FireModeData.DeviationMultiplier * 0.5f);
			m_BurstCount++;
			m_CurrentDelay = m_FireModuleData.ShotDelay;
		}
	}

	//	INTERNAL UPDATE
	public override void OnUpdate(float baseFireDispersion, float baseCamDeviation)
	{
		if (m_CurrentDelay <= 0.0f && m_BurstCount < m_BurstSize)
		{
			m_FireFunction(baseFireDispersion * m_FireModeData.DispersionMultiplier, baseCamDeviation * m_FireModeData.DeviationMultiplier);
			m_BurstCount++;
			m_CurrentDelay = m_FireModuleData.ShotDelay;
		}
	}

	//	END
	public override void OnEnd(float baseFireDispersion, float baseCamDeviation)
	{
		m_BurstCount = 0;
	}
}



public class WPN_FireMode_BurstAuto : WPN_FireMode_Burst
{
	[SerializeField, ReadOnly]
	private				bool					m_BurstActive				= false;
	[SerializeField, ReadOnly]
	private				float					m_BaseFireDispersion		= 0.0f;
	[SerializeField, ReadOnly]
	private				float					m_BaseCamDeviation			= 0.0f;

	private void StopAutoBurstSequence()
	{
		m_BurstCount = 0;
		m_BurstActive = false;
	}

	//	INTERNAL UPDATE
	public override void InternalUpdate(float DeltaTime, uint magazineSize)
	{
		m_CurrentDelay = Mathf.Max(m_CurrentDelay - DeltaTime, 0.0f);

		if (m_CurrentDelay <= 0.0f && m_BurstActive)
		{
			m_FireFunction(m_BaseFireDispersion, m_BaseCamDeviation);

			m_BurstCount++;

			m_CurrentDelay = m_FireModuleData.ShotDelay;

			if (m_BurstCount >= m_BurstSize || magazineSize == 0)
			{
				StopAutoBurstSequence();
			}
		}

		if (m_FireModule.Magazine <= 0)
		{
			StopAutoBurstSequence();
		}
	}

	//	START
	public override void OnStart(float baseFireDispersion, float baseCamDeviation)
	{
		if (m_CurrentDelay <= 0.0f && m_BurstCount < m_BurstSize)
		{
			float fireDispersion = (!m_ApplyDeviation) ? baseFireDispersion : 0.0f;
			m_FireFunction(fireDispersion, baseCamDeviation);

			m_BaseFireDispersion = fireDispersion;
			m_BaseCamDeviation = baseCamDeviation;

			m_BurstCount++;
			m_BurstActive = true;
			m_CurrentDelay = m_FireModuleData.ShotDelay;
		}
	}

	//	INTERNAL UPDATE
	public override void OnUpdate(float baseFireDispersion, float baseCamDeviation)
	{

	}

	//	END
	public override void OnEnd(float baseFireDispersion, float baseCamDeviation)
	{

	}
}