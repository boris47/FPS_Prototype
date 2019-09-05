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

		public	Coroutine	ExecuteSequence()
		{
			return m_MonoBehaviour.StartCoroutine( StartCO() );
		}

	}

	private	static	CoroutinesManager	m_Instance			= null;

	private	static	bool				m_IsInitialized		= false;

	private	static	bool				m_ShowDebugInfo		= false;

	private	static	uint				m_PendingRoutines	= 0;

	/////////////////////////////////////////////////////////////////
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
	public static  void	AddCoroutineToPendingCount( uint howMany )
	{
		m_PendingRoutines += howMany;
		print( "CoroutinesManager::AddCoroutineToPendingCount: Current Count " + m_PendingRoutines );
	}


	/////////////////////////////////////////////////////////////////
	public static  void	RemoveCoroutineFromPendingCount( uint howMany )
	{
		if ( howMany > m_PendingRoutines )
		{
			Debug.Log( "CoroutinesManager::RemoveCoroutineToPendingCount:Trying to remove more than available pending routines" );
			Debug.Log( "Current Pending Routines are : " + m_PendingRoutines + ", tried to remove: " + howMany );
			return;
		}

		m_PendingRoutines -= howMany;

		print( "CoroutinesManager::RemoveCoroutineFromPendingCount: Current Count " + m_PendingRoutines );
	}


	/////////////////////////////////////////////////////////////////
	public static IEnumerator	WaitPendingCoroutines()
	{
		yield return null;

		while ( m_PendingRoutines > 0 )
		{
			yield return null;
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
	public	static	Coroutine	Start( IEnumerator routine, string debugKey = "" )
	{
		Initialize();
		if ( debugKey.Length > 0 )
		{
//			Debug.Log( "Starting coroutine for " + debugKey );
		}
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
