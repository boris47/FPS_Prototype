
using UnityEngine;

public class WPN_FireMode_Single : WPN_FireModeBase
{
	public	override	EFireMode				FireMode					=> EFireMode.SINGLE;


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
			m_FireFunction(baseFireDispersion * m_FireModeData.DispersionMultiplier, baseCamDeviation * m_FireModeData.DeviationMultiplier);
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
