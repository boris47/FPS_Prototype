using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutinesManager : SingletonMonoBehaviour<CoroutinesManager> {

	public	class RoutinesSequence {
		private		int						m_CurrentIndex				= 0;
		private		List<IEnumerator>		m_Routines					= new List<IEnumerator>();
		private		MonoBehaviour			m_MonoBehaviour				= null;
		private		IEnumerator				m_CurrentEnumerator			= null;

		public RoutinesSequence( MonoBehaviour monoBehaviour, IEnumerator Routine )
		{
			if ( Routine.IsNotNull() )
				return;

			m_CurrentEnumerator = Routine;
			m_MonoBehaviour = monoBehaviour;
			m_Routines.Add( Routine );
		}

		public RoutinesSequence AddStep( IEnumerator Routine )
		{
			if ( Routine.IsNotNull() )
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

	private	static	uint				m_PendingRoutines	= 0;
	public	static	uint				PendingRoutines
	{
		get { return m_PendingRoutines; }
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnBeforeSceneLoad()
	{ }

	
	/////////////////////////////////////////////////////////////////
	public static  void	AddCoroutineToPendingCount( uint howMany )
	{
		m_PendingRoutines += howMany;
	//	print( "CoroutinesManager::AddCoroutineToPendingCount: Current Count " + m_PendingRoutines );
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

	//	print( "CoroutinesManager::RemoveCoroutineFromPendingCount: Current Count " + m_PendingRoutines );
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
	/// <summary> Start a new coroutine </summary>
	public	static	Coroutine	Start( IEnumerator routine, string debugKey = "" )
	{
		if ( ShowDebugInfo && debugKey.Length > 0 )
		{
			Debug.Log( "Starting coroutine for " + debugKey );
		}
		return Instance?.StartCoroutine( routine );
	}


	////////////////////////////////////////////////////////////////
	/// <summary> Start given coroutine </summary>
	public	static	void	Stop( Coroutine routine )
	{
		Instance?.StopCoroutine( routine );
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Stop all running coroutines </summary>
	public	static	void	StopAll()
	{
		Instance?.StopAllCoroutines();
	}

	/////////////////////////////////////////////////////////////////
	// Create a sequence object, where to add routine and finally start
	public	static	RoutinesSequence	CreateSequence( IEnumerator MainRoutine )
	{
		return new RoutinesSequence( Instance, MainRoutine );
	}
}
