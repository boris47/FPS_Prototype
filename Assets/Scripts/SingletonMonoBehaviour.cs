using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

	private		static			SingletonMonoBehaviour<T>				m_Instance				= null;
	public		static			T										Instance
	{
		get {
		//	if ( m_Instance == null )
			{
				Initialize();
				UnityEngine.Assertions.Assert.IsNotNull
				(
					m_Instance,
					typeof(T).Name + " has been not initialized correctly!!"
				);
			}
			return m_Instance as T;
		}
	}

	protected	static			bool			m_IsInitialized			= false;

	protected					bool			m_bIsMarkedForDelete	= false;

	protected	static			bool			m_ShowDebugInfo			= false;

	
	//////////////////////////////////////////////////////////////////////////
//	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	protected	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			m_Instance = FindObjectOfType<SingletonMonoBehaviour<T>>();
			if ( m_Instance == null )
			{
				m_Instance = new GameObject( typeof(T).Name ).AddComponent<T>() as SingletonMonoBehaviour<T>;
			}
//			m_Instance.hideFlags = HideFlags.DontSave;
			m_IsInitialized = true;

			DontDestroyOnLoad( m_Instance );

			Database.Section debugInfosSection = null;
			if ( GlobalManager.Configs.bGetSection( "DebugInfos", ref debugInfosSection ) )
			{
				m_ShowDebugInfo = debugInfosSection.AsBool( typeof(T).Name, false );
			}

			m_Instance.OnFirstGetCall();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected abstract void OnFirstGetCall();


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDestroy()
	{
		if ( m_Instance != this )
			return;

		m_IsInitialized = false;
		m_Instance = null;
	}

}