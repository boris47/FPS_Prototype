using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IScope : IWeaponAttachment {

}

public class Scope : WeaponAttachment, IScope {
	

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
