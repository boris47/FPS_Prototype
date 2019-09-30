using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

	protected	static			T				m_Instance				= default(T);
	public		static			T				Instance
	{
		get { return m_Instance; }
	}

	protected	static			bool			m_IsInitialized			= false;

	

	//////////////////////////////////////////////////////////////////////////
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			m_Instance = FindObjectOfType<T>();
			if ( m_Instance == null )
			{
				m_Instance = new GameObject(typeof(T).Name).AddComponent<T>();
			}
			m_Instance.hideFlags = HideFlags.DontSave;
			m_IsInitialized = true;

			DontDestroyOnLoad( m_Instance );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void Awake()
	{
		// Singleton
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDestroy()
	{
		if ( m_Instance != this )
			return;

		m_IsInitialized = false;
		m_Instance = null;
	}

}