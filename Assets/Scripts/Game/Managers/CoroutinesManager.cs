using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutinesManager : GlobalMonoBehaviourSingleton<CoroutinesManager>
{
	public class RoutinesSequence
	{
		private		readonly		IEnumerator			m_CurrentEnumerator				= null;
		private		readonly		MonoBehaviour		m_MonoBehaviour					= null;
		private		readonly		List<IEnumerator>	m_Routines						= new List<IEnumerator>();
		private						int					m_CurrentIndex					= 0;

		public RoutinesSequence(MonoBehaviour monoBehaviour, IEnumerator Routine)
		{
			if (Routine.IsNotNull())
			{
				return;
			}

			m_CurrentEnumerator = Routine;
			m_MonoBehaviour = monoBehaviour;
			m_Routines.Add(Routine);
		}

		public RoutinesSequence AddStep(IEnumerator Routine)
		{
			if (Routine.IsNotNull())
			{
				m_Routines.Add(Routine);
			}
			return this;
		}

		private IEnumerator StartCO()
		{
			while (m_CurrentIndex < m_Routines.Count)
			{
				IEnumerator CurrentEnumerator = m_Routines[m_CurrentIndex];
				yield return m_MonoBehaviour.StartCoroutine(CurrentEnumerator);
				m_CurrentIndex++;
			}
		}

		public Coroutine ExecuteSequence() => m_MonoBehaviour.StartCoroutine(StartCO());
	}

	[SerializeField]
	private			uint				m_PendingRoutines			= 0u;

	[SerializeField, ReadOnly]
	private			List<string>		m_IdentifiersQuueue			= new List<string>();
	public static	uint				PendingRoutines				=> m_Instance.m_PendingRoutines;


	/////////////////////////////////////////////////////////////////
	public static void AddCoroutineToPendingCount(uint howMany, UnityEngine.Object requester)
	{
		if (m_Instance)
		{
			m_Instance.m_PendingRoutines += howMany;
			m_Instance.m_IdentifiersQuueue.Add(requester.name);
		}
		else
		{
			Debug.LogWarning($"AddCoroutineToPendingCount: no instance");
		}
	}


	/////////////////////////////////////////////////////////////////
	public static void RemoveCoroutineFromPendingCount(uint howMany, UnityEngine.Object requester)
	{
		if (m_Instance)
		{
			if (howMany > m_Instance.m_PendingRoutines)
			{
				Debug.LogError($"Trying to remove more than available pending routines, current Pending Routines are : {m_Instance.m_PendingRoutines}, tried to remove: {howMany}");
				return;
			}

			m_Instance.m_PendingRoutines -= howMany;
			Utils.CustomAssertions.IsTrue(m_Instance.m_IdentifiersQuueue.Remove(requester.name));
		}
		else
		{
			Debug.LogWarning($"RemoveCoroutineFromPendingCount: no instance");
		}
	}


	/////////////////////////////////////////////////////////////////
	public static IEnumerator WaitPendingCoroutines()
	{
		yield return null;
		yield return new WaitUntil(() => m_Instance?.m_PendingRoutines == 0);
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Start a new coroutine </summary>
	public static Coroutine Start(IEnumerator routine, string debugKey = "")
	{
		if (ShowDebugInfo && debugKey.Length > 0)
		{
			Debug.Log($"Starting coroutine for {debugKey}");
		}
		return m_Instance?.StartCoroutine(routine);
	}


	////////////////////////////////////////////////////////////////
	/// <summary> Start given coroutine </summary>
	public static void Stop(Coroutine routine)
	{
		m_Instance?.StopCoroutine(routine);
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Stop all running coroutines </summary>
	public static void StopAll()
	{
		m_Instance?.StopAllCoroutines();
	}


	/////////////////////////////////////////////////////////////////
	/// <summary> Create a sequence object, where to add routine and finally start </summary>
	public static RoutinesSequence CreateSequence(IEnumerator MainRoutine) => new RoutinesSequence(m_Instance, MainRoutine);
}
