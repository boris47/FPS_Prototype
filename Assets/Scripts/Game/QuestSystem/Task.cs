
using UnityEngine;

namespace QuestSystem
{	
	using System.Collections.Generic;

	public class Task : MonoBehaviour
	{
		[SerializeField]
		private	List<Objective_Base>		m_Objectives				= new List<Objective_Base>();
		[SerializeField]
		private GameEvent					m_OnCompletion				= null;
		[SerializeField, ReadOnly]
		private	bool						m_IsCurrentlyActive			= false;
		[SerializeField, ReadOnly]
		private	bool						m_IsCompleted				= false;
		[SerializeField, ReadOnly]
		private	bool						m_IsInitialized				= false;

		private	System.Action<Task>			m_OnCompletionCallback		= delegate { };

		public string						ID							=> name;
		public bool							IsCompleted					=> m_IsCompleted;
		public bool							IsInitialized				=> m_IsInitialized;
		public string						StateName					=> name;


		//////////////////////////////////////////////////////////////////////////
		public void Initialize(System.Action<Task> onCompletionCallback, System.Action<Task> onFailureCallback)
		{
			if (!m_IsInitialized)
			{
				// Already assigned
				if (m_Objectives.Count > 0)
				{
					foreach (Objective_Base o in m_Objectives)
					{
						o.Initialize(this, OnObjectiveCompleted, OnObjectiveFailed);
					}

					if (m_Objectives[m_Objectives.Count - 1].IsOptional)
					{
						Debug.Log($"WARNIGN: Task {name} has last objective set as optional");
					}
				}

				m_OnCompletionCallback = onCompletionCallback;

				m_IsInitialized = true;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public bool ReInit()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		public bool Finalize()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		public virtual void OnSave(StreamUnit streamUnit)
		{
			m_Objectives.ForEach(o => o.OnSave(streamUnit));
		}


		//////////////////////////////////////////////////////////////////////////
		public virtual void OnLoad(StreamUnit streamUnit)
		{
			m_Objectives.ForEach(o => o.OnLoad(streamUnit));
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnTaskCompleted()
		{
			// Internal Flag
			m_IsCompleted = true;

			m_IsCurrentlyActive = false;

			//if ( GlobalQuestManager.ShowDebugInfo )
			//print( "Completed Task " + name );

			// Unity Events
			if (m_OnCompletion.IsNotNull() && m_OnCompletion.GetPersistentEventCount() > 0)
			{
				m_OnCompletion.Invoke();
			}

			// Internal Delegates
			m_OnCompletionCallback(this);

			Finalize();
		}


		//////////////////////////////////////////////////////////////////////////
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

			// Only Called if truly completed
			OnTaskCompleted();
		}


		//////////////////////////////////////////////////////////////////////////
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

			// Only Called if truly completed
			OnTaskCompleted();
		}


		//////////////////////////////////////////////////////////////////////////
		public void AddObjective(Objective_Base newObjective)
		{
			if (newObjective.IsNotNull() && !m_Objectives.Contains(newObjective))
			{
				newObjective.Initialize(this, OnObjectiveCompleted, OnObjectiveFailed);
				m_Objectives.Add(newObjective);
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public void RemoveObjective(Objective_Base objective)
		{
			if (objective.IsNotNull() && m_Objectives.Contains(objective))
			{
				m_Objectives.Remove(objective);
			}
		}


		//////////////////////////////////////////////////////////////////////////
		public void Activate()
		{
			if (m_Objectives.Count > 0)
			{
				m_IsCurrentlyActive = true;
				
				{
					int index = m_Objectives.FindIndex(o => !o.IsCompleted);

					// If task is completed on it's activation call
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

					// If task is completed on it's activation call
					if (index == -1)
					{
						OnTaskCompleted();
					}
					else // Otherwise active the first available objective
					{
						m_Objectives[index].Activate();
					}
				}
			}
			else
			{
				// An empty task resolve in immediately completed
				OnTaskCompleted();
			}
		}
	}
}
