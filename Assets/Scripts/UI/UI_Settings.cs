using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class UI_Settings : MonoBehaviour, IStateDefiner {

	
	private	bool			m_IsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return this.m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return this.name; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true )
			yield break;

		this.m_IsInitialized = true;
		{

		}

		if (this.m_IsInitialized )
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
		return this.m_IsInitialized;
	}
}
