using UnityEngine;
using System.Collections;

public interface IGranadeLauncher : IWeaponAttachment {

}

public class GranadeLauncher : WeaponAttachment, IGranadeLauncher {


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	void	Awake()
	{
		m_IsUsable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnActivate()
	{
		if ( m_IsUsable == false || m_IsAttached == false )
			return;

		m_IsActive = true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDeactivated()
	{
		if ( m_IsUsable == false || m_IsAttached == false )
			return;

		m_IsActive = false;
	}
	
}
