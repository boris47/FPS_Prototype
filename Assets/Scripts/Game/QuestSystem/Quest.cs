
using UnityEngine;

namespace QuestSystem {

	using System.Collections.Generic;

	[System.Serializable]
	public	enum EQuestStatus
	{
		NONE,
		ACTIVE,
		COMPLETED
	};

	[System.Serializable]
	public	enum EQuestScope
	{
		NONE,
		LOCAL,
		GLOBAL
	}


	public class Quest : MonoBehaviour
	{
	
		[SerializeField]
		private	List<Task>					m_Tasks						= new List<Task>();
		[SerializeField]
		private	GameEvent					m_OnCompletion				= null;
		[SerializeField, ReadOnly]
		private	bool						m_IsCompleted				= false;
		[SerializeField, ReadOnly]
		private	bool						m_IsInitialized				= false;
		[SerializeField, ReadOnly]
		private	EQuestStatus				m_Status					= EQuestStatus.NONE;
		[SerializeField, ReadOnly]
		private	EQuestScope					m_Scope						= EQuestScope.LOCAL;

		private	System.Action<Quest>		m_OnCompletionCallback		= delegate { };

		public int							TaskCount					=> m_Tasks.Count;
		public bool							IsCompleted					=> m_IsCompleted;
		public bool							IsInitialized				=> m_IsInitialized;
		public EQuestStatus					Status						=> m_Status;
		public EQuestScope					Scope						=> m_Scope;


		//////////////////////////////////////////////////////////////////////////
		private void Awake()
		{
			if (CustomAssertions.IsNotNull(GameManager.StreamEvents))
			{
				GameManager.StreamEvents.OnSave += OnSave;
				GameManager.StreamEvents.OnLoad += OnLoad;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnDestroy()
		{
			if (GameManager.StreamEvents.IsNotNull())
			{
				GameManager.StreamEvents.OnSave -= OnSave;
				GameManager.StreamEvents.OnLoad -= OnLoad;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public void Initialize(System.Action<Quest> onCompletionCallback, System.Action<Quest> onFailureCallback)
		{
			if (!m_IsInitialized)
			{
				// Already assigned
				foreach (Task t in m_Tasks)
				{
					t.Initialize(OnTaskCompleted, null);
				}
				m_IsInitialized = true;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public bool ReInit()
		{
			return true;
		}

		/*
		//////////////////////////////////////////////////////////////////////////
		public bool Finalize()
		{
			// UnRegistering game events
			GameManager.StreamEvents.OnSave -= OnSave;
			GameManager.StreamEvents.OnLoad -= OnLoad;
			return true;
		}
		*/

		//////////////////////////////////////////////////////////////////////////
		protected virtual bool OnSave(StreamData streamData, ref StreamUnit streamUnit)
		{
			streamUnit = streamData.NewUnit(gameObject);
			{
				foreach (Task task in m_Tasks)
				{
					task.OnSave(streamUnit);
				}
			}
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		protected	virtual		bool		OnLoad( StreamData streamData, ref StreamUnit streamUnit )
		{
			bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
			if (bResult)
			{
				foreach (Task task in m_Tasks)
				{
					task.OnLoad(streamUnit);
				}
			}
			return bResult;
		}


		//////////////////////////////////////////////////////////////////////////
		public void AddTask(Task newTask)
		{
			if (newTask.IsNotNull() && !m_Tasks.Contains(newTask))
			{
				newTask.Initialize(OnTaskCompleted, null);
				m_Tasks.Add(newTask);
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public void RemoveTask(Task task)
		{
			if (task.IsNotNull())
			{
				m_Tasks.Remove(task);
			}
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnQuestCompleted()
		{
			// Internal Flag
			m_Status = EQuestStatus.COMPLETED;

			m_IsCompleted = true;

			// Unity Events
			if (m_OnCompletion.IsNotNull() && m_OnCompletion.GetPersistentEventCount() > 0)
			{
				m_OnCompletion.Invoke();
			}

			// Internal Delegates
			m_OnCompletionCallback(this);

			GameManager.StreamEvents.OnSave -= OnSave;

			//Finalize();

			m_Status = EQuestStatus.COMPLETED;
		}


		//////////////////////////////////////////////////////////////////////////
		private	void	OnTaskCompleted(Task task)
		{
			bool bAreTasksCompleted = m_Tasks.TrueForAll(t => t.IsCompleted);
			if (bAreTasksCompleted)
			{
				// Only Called if truly completed
				OnQuestCompleted();
			}
			else
			{
				Task nextTask = null;
				int nextIndex = (m_Tasks.IndexOf(task) + 1);
				if (m_Tasks.IsValidIndex(nextIndex))
				{
					nextTask = m_Tasks[nextIndex];
				}
				// Some tasks are completed and sequence is broken, search for a valid target randomly among availables
				else
				{
					nextTask = m_Tasks.Find(t => !t.IsCompleted);
				}

				nextTask.Activate();
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public	void	Activate()
		{
			if (m_Tasks.Count > 0)
			{
				m_Status = EQuestStatus.ACTIVE;
				m_Tasks[0].Activate();
			}
			else
			{
				// An empty quest resolve in immediately completed
				OnQuestCompleted();
			}
		}
	}
}
