using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutinesManager : SingletonMonoBehaviour<CoroutinesManager> {

	public	class RoutinesSequence {
		private readonly IEnumerator				m_CurrentEnumerator			= null;
		private readonly MonoBehaviour				m_MonoBehaviour				= null;
		private readonly List<IEnumerator>			m_Routines					= new List<IEnumerator>();
		private			int							m_CurrentIndex				= 0;

		public RoutinesSequence( MonoBehaviour monoBehaviour, IEnumerator Routine )
		{
			if ( Routine.IsNotNull() )
				return;

			this.m_CurrentEnumerator = Routine;
			this.m_MonoBehaviour = monoBehaviour;
			this.m_Routines.Add( Routine );
		}

		public RoutinesSequence AddStep( IEnumerator Routine )
		{
			if ( Routine.IsNotNull() )
			{
				this.m_Routines.Add( Routine );
			}
			return this;
		}

		private	IEnumerator StartCO()
		{
			while (this.m_CurrentIndex < this.m_Routines.Count )
			{
				IEnumerator CurrentEnumerator = this.m_Routines[this.m_CurrentIndex ];
				yield return this.m_MonoBehaviour.StartCoroutine( CurrentEnumerator );
				this.m_CurrentIndex ++;
			}

		}

		public	Coroutine	ExecuteSequence()
		{
			return this.m_MonoBehaviour.StartCoroutine(this.StartCO() );
		}

	}

	[SerializeField]
	private	uint				m_PendingRoutines	= 0;
	public	static	uint				PendingRoutines
	{
		get { return Instance.m_PendingRoutines; }
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnBeforeSceneLoad()
	{ }

	
	/////////////////////////////////////////////////////////////////
	public static  void	AddCoroutineToPendingCount( uint howMany )
	{
		Instance.m_PendingRoutines += howMany;
	//	print( "CoroutinesManager::AddCoroutineToPendingCount: Current Count " + m_PendingRoutines );
	}


	/////////////////////////////////////////////////////////////////
	public static  void	RemoveCoroutineFromPendingCount( uint howMany )
	{
		if ( howMany > Instance.m_PendingRoutines)
		{
			Debug.Log( "CoroutinesManager::RemoveCoroutineToPendingCount:Trying to remove more than available pending routines" );
			Debug.Log( "Current Pending Routines are : " + Instance.m_PendingRoutines + ", tried to remove: " + howMany );
			return;
		}

		Instance.m_PendingRoutines -= howMany;

	//	print( "CoroutinesManager::RemoveCoroutineFromPendingCount: Current Count " + m_PendingRoutines );
	}


	/////////////////////////////////////////////////////////////////
	public static IEnumerator	WaitPendingCoroutines()
	{
		yield return null;
		yield return new WaitUntil(() => Instance.m_PendingRoutines == 0);
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
	/// <summary> Create a sequence object, where to add routine and finally start </summary>
	public static	RoutinesSequence	CreateSequence( IEnumerator MainRoutine )
	{
		return new RoutinesSequence( Instance, MainRoutine );
	}
}
