using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Settings : MonoBehaviour, IStateDefiner {

	
	private	bool			m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		m_bIsInitialized = true;
		{

		}

		if ( m_bIsInitialized )
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
		return m_bIsInitialized;
	}
}
