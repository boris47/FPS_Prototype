using UnityEngine;
using System.Collections;

public interface IGranadeLauncher : IWeaponAttachment {

}

public class GranadeLauncher : WeaponAttachment, IGranadeLauncher {


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	void	Awake()
	{
		this.m_IsUsable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnActivate()
	{
		if (this.m_IsUsable == false || this.m_IsAttached == false )
			return;

		this.m_IsActive = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDeactivated()
	{
		if (this.m_IsUsable == false || this.m_IsAttached == false )
			return;

		this.m_IsActive = false;
	}
	
}
