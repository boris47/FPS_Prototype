using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutinesManager : MonoBehaviour {

	public	class RoutinesSequence {
		private		int						m_CurrentIndex				= 0;
		private		List<IEnumerator>		m_Routines					= new List<IEnumerator>();
		private		MonoBehaviour			m_MonoBehaviour				= null;
		private		IEnumerator				m_CurrentEnumerator			= null;

		public RoutinesSequence( MonoBehaviour monoBehaviour, IEnumerator Routine )
		{
			if ( Routine == null )
				return;

			m_CurrentEnumerator = Routine;
			m_MonoBehaviour = monoBehaviour;
			m_Routines.Add( Routine );
		}

		public RoutinesSequence AddStep( IEnumerator Routine )
		{
			if ( Routine != null )
			{
				m_Routines.Add( Routine );
			}
			return this;
		}

		private	IEnumerator StartCO()
		{
			while ( m_CurrentIndex < m_Routines.Count )
			{
				IEnumerator CurrentEnumerator = m_Routines[ m_CurrentIndex ];
				yield return m_MonoBehaviour.StartCoroutine( CurrentEnumerator );
				m_CurrentIndex ++;
			}

		}

		public	void	Start()
		{
			m_MonoBehaviour.StartCoroutine( StartCO() );
		}

	}

	private	static	CoroutinesManager	m_Instance			= null;

	private	static	bool				m_IsInitialized		= false;

	private	static	bool				m_ShowDebugInfo		= false;

	private	static	List<Coroutine>		m_PendingRoutines	= new List<Coroutine>();

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

	public static  bool	AddCoroutineToPendingList( Coroutine routine )
	{
		if ( m_PendingRoutines.Contains( routine ) )
			return false;

		m_PendingRoutines.Add( routine );
		return true;
	}

	public static IEnumerator	WaitPendingCoroutines()
	{
		foreach( Coroutine routine in m_PendingRoutines )
		{
			yield return routine;
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
		if ( GlobalManager.Configs.bGetSection( "DebugInfos", ref debugInfosSection ) )
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

	/////////////////////////////////////////////////////////////////
	// Create a sequence object, where to add routine and finally start
	public	static	RoutinesSequence	CreateSequence( IEnumerator MainRoutine )
	{
		Initialize();

		return new RoutinesSequence( m_Instance, MainRoutine );
	}
}
