

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

	public interface IObjective : IStateDefiner<ITask, IObjective> {

		string			ID { get; }

		bool			IsCompleted				{ get; }

		bool			IsOptional				{ get; }

		bool			IsCurrentlyActive		{ get; }

		/// <summary> Add this objective to a Task, can be set as optioanl </summary>
		void			AddToTask				( ITask task, bool isOptional = false );

		/// <summary> Add another objective as dependency for the completion of this objective. 
		/// The dependencies must result completed in order to se as completed this objective </summary>
		void			AddDependency			( Objective_Base other );

		/// <summary> Set as current active to true and add indicator </summary>
		void			Activate();

		/// <summary> Set as current active to false and remove indicator </summary>
		void			Deactivate();
	}


	public abstract class Objective_Base : MonoBehaviour, IObjective {

		[SerializeField]
		private GameEvent							m_OnCompletion				= new GameEvent();

		[SerializeField]
		protected	List<Objective_Base>			m_Dependencies				= new List<Objective_Base>();

		protected	System.Action<IObjective>		m_OnCompletionCallback		= delegate { };
		protected	System.Action<IObjective>		m_OnFailureCallback			= delegate { };
		protected	bool							m_IsCompleted				= false;

		[SerializeField]
		protected	bool							m_IsOptional				= false;

		protected	ITask							m_MotherTask					= null;

		protected	EObjectiveState					m_ObjectiveState			= EObjectiveState.NONE;

		protected	bool							m_IsInitialized				= false;

		//--
		public string			ID
		{
			get { return this.name; }
		}

		//--
		public	bool			IsOptional
		{
			get { return this.m_IsOptional; }
		}

		//--
		public	bool			IsCompleted
		{
			get { return this.m_IsCompleted; }
		}

		//--
		public	bool			IsCurrentlyActive
		{
			get { return this.m_ObjectiveState == EObjectiveState.ACTIVATED; }
		}

		//--
		public bool IsInitialized	// IStateDefiner
		{
			get { return this.m_IsInitialized; }
		}

		string IStateDefiner<ITask, IObjective>.StateName
		{
			get { return this.name; }
		}


		//////////////////////////////////////////////////////////////////////////
		// ( IStateDefiner )
		public		bool		Initialize( ITask motherTask, System.Action<IObjective> onCompletionCallback, System.Action<IObjective> onFailureCallback )
		{
			this.m_MotherTask = motherTask;

			return this.InitializeInternal( motherTask, onCompletionCallback, onFailureCallback );
		}

		protected	abstract	bool	InitializeInternal( ITask motherTask, System.Action<IObjective> onCompletionCallback, System.Action<IObjective> onFailureCallback );


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
			this.m_ObjectiveState = EObjectiveState.ACTIVATED;

			this.ActivateInternal();
		}

		protected	abstract	void	ActivateInternal();

		
		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		public		void		Deactivate()
		{
			this.m_ObjectiveState = EObjectiveState.NONE;

			this.DeactivateInternal();
		}
		
		protected	abstract	void	DeactivateInternal();

		//////////////////////////////////////////////////////////////////////////
		// SetTaskOwner ( IObjective )
		/// <summary> Add this objective to a Task </summary>
		void			IObjective.AddToTask( ITask task, bool isOptional )
		{
			// If Already assignet to a task, we must remove it before add to another task
			if (this.m_MotherTask != null )
			{
				this.m_MotherTask.RemoveObjective( this );
			}

			// If add succeeded
			bool result = task.AddObjective( this );
			if ( result )
			{
				this.m_MotherTask = task;
			}

			this.m_IsOptional = isOptional;
		}


		//////////////////////////////////////////////////////////////////////////
		// AddDependency ( IObjective )
		/// <summary> Add another objective as dependency for the completion of this objective. 
		/// The dependencies must result completed in order to se as completed this objective </summary>
		void			IObjective.AddDependency(Objective_Base other)
		{
			if ( other.IsCompleted == false && this.m_Dependencies.Contains( other ) == false )
			{
				this.m_Dependencies.Add( other );
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted
		protected	void			OnObjectiveCompleted()
		{
			// Internal Flag
			this.m_IsCompleted = true;

			// Unity Events
			if (this.m_OnCompletion.GetPersistentEventCount() > 0 )
			{
				this.m_OnCompletion.Invoke();
			}

			// Internal Delegates
			this.m_OnCompletionCallback( this );

			print( "Completed Objective " + this.name );

			this.Finalize();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted
		protected	void	OnObjectiveFailed()
		{
			// Internal Flag
			this.m_IsCompleted = true;

			// Internal Delegates
			this.m_OnFailureCallback( this );

			print( "Failed Objective " + this.name );

			this.Finalize();
		}
		
	}


}