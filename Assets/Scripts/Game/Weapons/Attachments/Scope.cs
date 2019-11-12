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
		m_IsUsable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnActivate()
	{
		if ( m_IsUsable == false || m_IsAttached == false )
			return;

		m_IsActive = true;
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnDeactivated()
	{
		if ( m_IsUsable == false || m_IsAttached == false )
			return;

		m_IsActive = false;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override void	SetActive( bool state )
	{
		if ( m_IsUsable == false || m_IsAttached == false )
			return;

		m_IsActive = state;

		if ( m_IsActive == true )
		{
			OnActivate();
		}
		else
		{
			OnDeactivated();
		}
	}
	
}
