using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WPN_FireModule_Blaster : WPN_FireModule_Barrel
{
	protected override void InternalUpdate( float DeltaTime )
	{
		this.m_WpnFireMode.InternalUpdate( DeltaTime, this.m_Magazine );
	}

	public override void ApplyModifier( Database.Section modifier )
	{
		base.ApplyModifier( modifier );

		// Do actions here
	}


	public	override	void	ResetBaseConfiguration()
	{
		base.ResetBaseConfiguration();

		// Do actions here
	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		base.RemoveModifier( modifier );

		// Do Actions here
	}
	
}
