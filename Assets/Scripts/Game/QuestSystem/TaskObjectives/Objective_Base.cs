

using UnityEngine;

namespace QuestSystem
{
	using System.Collections.Generic;

	public	enum EObjectiveState
	{
		NONE,
		ACTIVATED,
		COMPLETED,
		FAILED
	}

	public abstract class Objective_Base : MonoBehaviour
	{
		[SerializeField]
		private GameEvent							m_OnCompletion				= new GameEvent();
		[SerializeField]
		protected	List<Objective_Base>			m_Dependencies				= new List<Objective_Base>();
		[SerializeField, ReadOnly]
		protected	bool							m_IsCompleted				= false;
		[SerializeField]
		protected	bool							m_IsOptional				= false;
		[SerializeField, ReadOnly]
		protected	Task							m_MotherTask				= null;
		[SerializeField, ReadOnly]
		protected	EObjectiveState					m_ObjectiveState			= EObjectiveState.NONE;
		[SerializeField, ReadOnly]
		protected	bool							m_IsInitialized				= false;

		protected	System.Action<Objective_Base>	m_OnCompletionCallback		= delegate { };
		protected	System.Action<Objective_Base>	m_OnFailureCallback			= delegate { };

		public string								ID							=> name;

		public bool									IsOptional					=> m_IsOptional;

		public bool									IsCompleted					=> m_IsCompleted;

		public bool									IsCurrentlyActive			=> m_ObjectiveState == EObjectiveState.ACTIVATED;

		public bool									IsInitialized				=> m_IsInitialized;

		public string								StateName					=> name;


		//////////////////////////////////////////////////////////////////////////
		public bool Initialize(Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback)
		{
			m_MotherTask = motherTask;

			return InitializeInternal(motherTask, onCompletionCallback, onFailureCallback);
		}


		//////////////////////////////////////////////////////////////////////////
		protected abstract bool InitializeInternal(Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback);


		//////////////////////////////////////////////////////////////////////////
		public	abstract	bool		ReInit();


		//////////////////////////////////////////////////////////////////////////
		public	abstract	bool		Finalize();


		//////////////////////////////////////////////////////////////////////////
		public	abstract	void		OnSave( StreamUnit streamUnit );


		//////////////////////////////////////////////////////////////////////////
		public	abstract	void		OnLoad( StreamUnit streamUnit );


		//////////////////////////////////////////////////////////////////////////
		public void Activate()
		{
			m_ObjectiveState = EObjectiveState.ACTIVATED;

			ActivateInternal();
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract	void	ActivateInternal();


		//////////////////////////////////////////////////////////////////////////
		public void Deactivate()
		{
			m_ObjectiveState = EObjectiveState.NONE;

			DeactivateInternal();
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract	void	DeactivateInternal();

		//////////////////////////////////////////////////////////////////////////
		public void AddToTask(Task task, bool isOptional)
		{
			// If Already assigned to a task, we must remove it before add to another task
			if (m_MotherTask.IsNotNull())
			{
				m_MotherTask.RemoveObjective(this);
			}

			m_MotherTask = task;
			m_IsOptional = isOptional;

			task.AddObjective(this);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Add another objective as dependency for the completion of this objective. 
		/// The dependencies must result completed in order to set as completed this objective </summary>
		public void AddDependency(Objective_Base other)
		{
			if (!other.IsCompleted && !m_Dependencies.Contains(other))
			{
				m_Dependencies.Add(other);
			}
		}


		//////////////////////////////////////////////////////////////////////////
		protected void OnObjectiveCompleted()
		{
			// Internal Flag
			m_IsCompleted = true;

			// Unity Events
			if (m_OnCompletion.GetPersistentEventCount() > 0)
			{
				m_OnCompletion.Invoke();
			}

			// Internal Delegates
			m_OnCompletionCallback(this);

			print("Completed Objective " + name);

			Finalize();
		}


		//////////////////////////////////////////////////////////////////////////
		protected void OnObjectiveFailed()
		{
			// Internal Flag
			m_IsCompleted = true;

			// Internal Delegates
			m_OnFailureCallback(this);

			print("Failed Objective " + name);

			Finalize();
		}
	}
}