
using UnityEngine;

namespace QuestSystem {
	
	using System.Collections.Generic;

	

	public class Task : MonoBehaviour
	{
		[SerializeField]
		private	List<Objective_Base>		m_Objectives				= new List<Objective_Base>();

		[SerializeField]
		private GameEvent					m_OnCompletion				= null;

		private	System.Action<Task>			m_OnCompletionCallback		= delegate { };

		private	bool						m_IsCurrentlyActive			= false;

		[System.NonSerialized]
		private	bool						m_IsCompleted				= false;

		[System.NonSerialized]
		private	bool						m_IsInitialized				= false;


		public string ID => name;

		//--
		public bool IsCompleted => m_IsCompleted;

		//--
		public bool IsInitialized => m_IsInitialized;

		public string StateName => name;


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public				bool		Initialize( Quest motherQuest, System.Action<Task> onCompletionCallback, System.Action<Task> onFailureCallback )
		{
			if (m_IsInitialized)
				return true;

			m_IsInitialized = true;

			bool result = false;


			// Already assigned
			if (m_Objectives.Count > 0)
			{
				foreach (Objective_Base o in m_Objectives)
				{
					result &= o.Initialize(this, OnObjectiveCompleted, OnObjectiveFailed); // Init every Objective
				}

				if (m_Objectives[m_Objectives.Count - 1].IsOptional)
				{
					Debug.Log($"WARNIGN: Task {name} has last objective set as optional");
				}
			}


			m_OnCompletionCallback = onCompletionCallback;

			return result;
		}


		//////////////////////////////////////////////////////////////////////////
		// ReInit ( IStateDefiner )
		public				bool		ReInit()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Finalize ( IStateDefiner )
		public				bool		Finalize()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnSave
		public	virtual		void		OnSave( StreamUnit streamUnit )
		{
			m_Objectives.ForEach(o => o.OnSave(streamUnit));
		}


		//////////////////////////////////////////////////////////////////////////
		// OnLoad
		public	virtual		void		OnLoad( StreamUnit streamUnit )
		{
			m_Objectives.ForEach(o => o.OnLoad(streamUnit));
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		private void	OnTaskCompleted()
		{
			// Internal Flag
			m_IsCompleted = true;

			m_IsCurrentlyActive = false;

			//if ( GlobalQuestManager.ShowDebugInfo )
			//print( "Completed Task " + name );

			// Unity Events
			if (m_OnCompletion.IsNotNull() && m_OnCompletion.GetPersistentEventCount() > 0)
				m_OnCompletion.Invoke();

			// Internal Delegates
			m_OnCompletionCallback(this);

			Finalize();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted
		private void OnObjectiveCompleted(Objective_Base objective)
		{
			objective.Deactivate();

			bool bAreObjectivesCompleted = m_Objectives.TrueForAll(o => !o.IsOptional && o.IsCompleted);
			if (!bAreObjectivesCompleted)
			{
				Objective_Base nextObjective = m_Objectives.Find(o => !o.IsOptional && !o.IsCompleted);
				if (m_IsCurrentlyActive && nextObjective.IsNotNull())
				{
					if (!nextObjective.IsCurrentlyActive && !nextObjective.IsCompleted)
					{
						nextObjective.Activate();
					}
					return;
				}
			}

			// Only Called if trurly completed
			OnTaskCompleted();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted
		private void OnObjectiveFailed(Objective_Base objective)
		{
			objective.Deactivate();

			bool bAreObjectivesCompleted = m_Objectives.TrueForAll(o => !o.IsOptional && o.IsCompleted);
			if (!bAreObjectivesCompleted)
			{
				Objective_Base nextObjective = m_Objectives.Find(o => !o.IsOptional && !o.IsCompleted);
				if (m_IsCurrentlyActive && nextObjective.IsNotNull())
				{
					if (!nextObjective.IsCurrentlyActive && !nextObjective.IsCompleted)
					{
						nextObjective.Activate();
					}
					return;
				}
			}

			// Only Called if trurly completed
			OnTaskCompleted();
		}


		//////////////////////////////////////////////////////////////////////////
		// AddObjective ( ITask )
		public bool AddObjective(Objective_Base newObjective)
		{
			if (newObjective == null)
				return false;

			if (!m_Objectives.Contains(newObjective))
			{
				newObjective.Initialize(this, OnObjectiveCompleted, OnObjectiveFailed);
				m_Objectives.Add(newObjective);
			}
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// RemoveObjective ( ITask )
		public bool RemoveObjective(Objective_Base objective)
		{
			if (objective == null)
				return false;

			if (!m_Objectives.Contains(objective))
				return false;

			m_Objectives.Remove(objective);
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate
		public	bool	Activate()
		{
			if (m_Objectives.Count == 0 )
			{
				return false;
			}

			m_IsCurrentlyActive = true;

			//if ( GlobalQuestManager.ShowDebugInfo )
			//	print(name + " task activation" );

			{
				int index = m_Objectives.FindIndex(o => !o.IsCompleted);

				// If task is completed on it's activation call for completion
				if (index == -1)
				{
					OnTaskCompleted();
				}
				else // Otherwise active the first available objective
				{
					m_Objectives[index].Activate();
				}
			}
			{
				int index = m_Objectives.FindIndex(o => !o.IsOptional && !o.IsCompleted);

				// If task is completed on it's activation call for completion
				if (index == -1)
				{
					OnTaskCompleted();
				}
				else // Otherwise active the first available objective
				{
					m_Objectives[index].Activate();
				}
			}

			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate
		public	bool	Deactivate()
		{
			return true;
		}
	}
}
