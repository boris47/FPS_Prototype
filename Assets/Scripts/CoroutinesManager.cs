using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutinesManager : MonoBehaviour {

	private	static	CoroutinesManager	m_Instance			= null;

	private	static	bool				m_IsInitialized		= false;

	private	static	bool				m_ShowDebugInfo		= false;

	// 
	private	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			GameObject go = new GameObject();
			go.hideFlags = HideFlags.HideAndDontSave;
			m_Instance = go.AddComponent<CoroutinesManager>();

			m_IsInitialized = true;
		}
	}



	/////////////////////////////////////////////////////////////////
	private void Awake()
	{	// Singleton
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		DontDestroyOnLoad( this );
		m_Instance = this;
		m_IsInitialized = true;

		Database.Section debugInfosSection = null;
		if ( GameManager.Configs.bGetSection( "DebugInfos", ref debugInfosSection ) )
		{
			m_ShowDebugInfo = debugInfosSection.AsBool( "CoroutinesManager", false);
		}
	}

	
	/////////////////////////////////////////////////////////////////
	/// <summary> Start a new coroutine </summary>
	public	static	Coroutine	Start( IEnumerator routine )
	{
		Initialize();
		return m_Instance.StartCoroutine( routine );
	}


	////////////////////////////////////////////////////////////////
	/// <summary> Start given coroutine </summary>
	public	static	void	Stop( Coroutine routine )
	{
		Initialize();
		m_Instance.StopCoroutine( routine );
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Stop all running coroutines </summary>
	public	static	void	StopAll()
	{
		Initialize();
		m_Instance.StopAllCoroutines();
	}

}
