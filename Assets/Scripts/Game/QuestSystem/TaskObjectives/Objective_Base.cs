

using UnityEngine;

namespace QuestSystem {

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

		protected	System.Action<Objective_Base>	m_OnCompletionCallback		= delegate { };
		protected	System.Action<Objective_Base>	m_OnFailureCallback			= delegate { };
		protected	bool							m_IsCompleted				= false;

		[SerializeField]
		protected	bool							m_IsOptional				= false;

		protected	Task							m_MotherTask				= null;

		protected	EObjectiveState					m_ObjectiveState			= EObjectiveState.NONE;

		protected	bool							m_IsInitialized				= false;

		//--
		public string ID => name;

		//--
		public bool IsOptional => m_IsOptional;

		//--
		public bool IsCompleted => m_IsCompleted;

		//--
		public bool IsCurrentlyActive => m_ObjectiveState == EObjectiveState.ACTIVATED;

		//--
		public bool IsInitialized  => m_IsInitialized;

		public string StateName => name;


		//////////////////////////////////////////////////////////////////////////
		// ( IStateDefiner )
		public		bool		Initialize( Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback )
		{
			m_MotherTask = motherTask;

			return InitializeInternal( motherTask, onCompletionCallback, onFailureCallback );
		}

		protected	abstract	bool	InitializeInternal( Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback );


		//////////////////////////////////////////////////////////////////////////
		// ( IStateDefiner )
		public	abstract	bool		ReInit();


		//////////////////////////////////////////////////////////////////////////
		// ( IStateDefiner )
		public	abstract	bool		Finalize();


		//////////////////////////////////////////////////////////////////////////
		// OnSave ( Abstract )
		public	abstract	void		OnSave( StreamUnit streamUnit );


		//////////////////////////////////////////////////////////////////////////
		// OnLoad ( Abstract )
		public	abstract	void		OnLoad( StreamUnit streamUnit );


		//////////////////////////////////////////////////////////////////////////
		// Activate ( IObjective )
		public		void		Activate()
		{
			m_ObjectiveState = EObjectiveState.ACTIVATED;

			ActivateInternal();
		}

		protected	abstract	void	ActivateInternal();

		
		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		public		void		Deactivate()
		{
			m_ObjectiveState = EObjectiveState.NONE;

			DeactivateInternal();
		}
		
		protected	abstract	void	DeactivateInternal();

		//////////////////////////////////////////////////////////////////////////
		// SetTaskOwner ( IObjective )
		/// <summary> Add this objective to a Task </summary>
		public void			AddToTask( Task task, bool isOptional )
		{
			// If Already assignet to a task, we must remove it before add to another task
			if (m_MotherTask != null )
			{
				m_MotherTask.RemoveObjective( this );
			}

			// If add succeeded
			bool result = task.AddObjective( this );
			if ( result )
			{
				m_MotherTask = task;
			}

			m_IsOptional = isOptional;
		}


		//////////////////////////////////////////////////////////////////////////
		// AddDependency ( IObjective )
		/// <summary> Add another objective as dependency for the completion of this objective. 
		/// The dependencies must result completed in order to se as completed this objective </summary>
		public void			AddDependency(Objective_Base other)
		{
			if ( other.IsCompleted == false && m_Dependencies.Contains( other ) == false )
			{
				m_Dependencies.Add( other );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted
		protected	void			OnObjectiveCompleted()
		{
			// Internal Flag
			m_IsCompleted = true;

			// Unity Events
			if (m_OnCompletion.GetPersistentEventCount() > 0 )
			{
				m_OnCompletion.Invoke();
			}

			// Internal Delegates
			m_OnCompletionCallback( this );

			print( "Completed Objective " + name );

			Finalize();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted
		protected	void	OnObjectiveFailed()
		{
			// Internal Flag
			m_IsCompleted = true;

			// Internal Delegates
			m_OnFailureCallback( this );

			print( "Failed Objective " + name );

			Finalize();
		}
		
	}


}