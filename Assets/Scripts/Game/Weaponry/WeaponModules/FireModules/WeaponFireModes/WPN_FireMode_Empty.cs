using UnityEngine;
using System.Collections;

public class WPN_FireMode_Empty : WPN_FireModeBase
{
	public override		EFireMode	FireMode		=> EFireMode.NONE;

	protected override void InternalSetup(in Database.Section fireModeSection, in WPN_FireModule fireModule) { }

	public override bool OnSave(StreamUnit streamUnit) => true;
	public override bool OnLoad(StreamUnit streamUnit) => true;

	public override void OnWeaponChange() { }

	public override void InternalUpdate(float DeltaTime, uint magazineSize) { }

	public override void OnStart(float baseFireDispersion, float baseCamDeviation) { }
	public override void OnUpdate(float baseFireDispersion, float baseCamDeviation) { }
	public override void OnEnd(float baseFireDispersion, float baseCamDeviation) { }
}
