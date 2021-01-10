using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class UI_Settings : MonoBehaviour, IStateDefiner {

	
	private	bool			m_IsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return name; }
	}


	//////////////////////////////////////////////////////////////////////////
	public void PreInit() { }

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (m_IsInitialized == true )
			yield break;

		m_IsInitialized = true;
		{

		}

		if (m_IsInitialized )
		{
				
		}
		else
		{
			Debug.LogError( "UI_Settings: Bad initialization!!!" );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator	IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}
}
