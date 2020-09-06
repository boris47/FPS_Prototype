
using UnityEngine;

namespace QuestSystem {
	
	using System.Collections.Generic;

	public interface ITask : IStateDefiner<IQuest, ITask> {

		string ID { get; }

		bool	Activate			();

		bool	Deactivate			();

		bool	IsCompleted			{ get; }

		bool	AddObjective		( Objective_Base newObjective );

		bool	RemoveObjective		( Objective_Base objective );
	}

	public class Task : MonoBehaviour, ITask {

		[SerializeField]
		private	List<Objective_Base>		m_Objectives				= new List<Objective_Base>();

		[SerializeField]
		private GameEvent					m_OnCompletion				= null;

		private	System.Action<ITask>		m_OnCompletionCallback		= delegate { };
		private	bool						m_IsCompleted				= false;
		private	bool						m_IsCurrentlyActive			= false;
		private	bool						m_IsInitialized				= false;

		public string			ID
		{
			get { return this.name; }
		}

		//--
		public bool		IsCompleted
		{
			get { return this.m_IsCompleted; }
		}

		//--
		public bool IsInitialized	// IStateDefiner
		{
			get { return this.m_IsInitialized; }
		}

		string IStateDefiner<IQuest, ITask>.StateName
		{
			get { return this.name; }
		}


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public				bool		Initialize( IQuest motherQuest, System.Action<ITask> onCompletionCallback, System.Action<ITask> dump )
		{
			if (this.m_IsInitialized == true )
				return true;

			this.m_IsInitialized = true;

			bool result = false;


			// Already assigned
			if (this.m_Objectives.Count > 0 )
			{
				foreach( IObjective o in this.m_Objectives )
				{
					result &= o.Initialize( this, this.OnObjectiveCompleted, this.OnObjectiveFailed ); // Init every Objective
				}

				if (this.m_Objectives[this.m_Objectives.Count - 1 ].IsOptional )
				{
					Debug.Log( "WARNIGN: Task " + this.name + " has last objective set as optional" );
				}
			}


			this.m_OnCompletionCallback = onCompletionCallback;

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
			this.m_Objectives.ForEach( o => o.OnSave( streamUnit ) );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnLoad
		public	virtual		void		OnLoad( StreamUnit streamUnit )
		{
			this.m_Objectives.ForEach( o => o.OnLoad( streamUnit ) );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		private void	OnTaskCompleted()
		{
			// Internal Flag
			this.m_IsCompleted = true;

			this.m_IsCurrentlyActive = false;

			if ( GlobalQuestManager.ShowDebugInfo )
				print( "Completed Task " + this.name );

			// Unity Events
			if (this.m_OnCompletion != null && this.m_OnCompletion.GetPersistentEventCount() > 0 )
				this.m_OnCompletion.Invoke();

			// Internal Delegates
			this.m_OnCompletionCallback( this );

			this.Finalize();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted
		private	void	OnObjectiveCompleted( IObjective objective )
		{
			objective.Deactivate();

			bool bAreObjectivesCompleted = this.m_Objectives.TrueForAll( ( Objective_Base o ) => { return o.IsOptional == false && o.IsCompleted == true; } );
			if ( bAreObjectivesCompleted == false )
			{
				IObjective nextObjective = this.m_Objectives.Find( o => o.IsOptional == false && o.IsCompleted == false );
				if (this.m_IsCurrentlyActive && nextObjective != null )
				{
					if ( nextObjective.IsCurrentlyActive == false && nextObjective.IsCompleted == false )
					{
						nextObjective.Activate();
					}
					return;
				}
			}

			// Only Called if trurly completed
			this.OnTaskCompleted();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted
		private	void	OnObjectiveFailed( IObjective objective )
		{
			objective.Deactivate();

			bool bAreObjectivesCompleted = this.m_Objectives.TrueForAll( ( Objective_Base o ) => { return o.IsOptional == false && o.IsCompleted == true; } );
			if ( bAreObjectivesCompleted == false )
			{
				IObjective nextObjective = this.m_Objectives.Find( o => o.IsOptional == false && o.IsCompleted == false );
				if (this.m_IsCurrentlyActive && nextObjective != null )
				{
					if ( nextObjective.IsCurrentlyActive == false && nextObjective.IsCompleted == false )
					{
						nextObjective.Activate();
					}
					return;
				}
			}

			// Only Called if trurly completed
			this.OnTaskCompleted();
		}


		


		//////////////////////////////////////////////////////////////////////////
		// AddObjective ( ITask )
		bool	 ITask.AddObjective( Objective_Base newObjective )
		{
			if ( newObjective == null )
				return false;

			if (this.m_Objectives.Contains( newObjective ) == true )
				return true;

			newObjective.Initialize( this, this.OnObjectiveCompleted, this.OnObjectiveFailed );
			this.m_Objectives.Add( newObjective );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// RemoveObjective ( ITask )
		bool	ITask.RemoveObjective( Objective_Base objective )
		{
			if ( objective == null )
				return false;

			if (this.m_Objectives.Contains( objective ) == false )
				return false;

			this.m_Objectives.Remove( objective );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate
		public	bool	Activate()
		{
			if (this.m_Objectives.Count == 0 )
			{
				return false;
			}

			this.m_IsCurrentlyActive = true;

			if ( GlobalQuestManager.ShowDebugInfo )
				print(this.name + " task activation" );

			{
				int index = this.m_Objectives.FindIndex( o => o.IsCompleted == false );

				// If task is completed on it's activation call for completion
				if ( index == -1 )
				{
					this.OnTaskCompleted();
				}
				else // Otherwise active the first available objective
				{
					this.m_Objectives[index].Activate();
				}
			}
			{
				int index = this.m_Objectives.FindIndex( o => o.IsOptional == false && o.IsCompleted == false );

				// If task is completed on it's activation call for completion
				if ( index == -1 )
				{
					this.OnTaskCompleted();
				}
				else // Otherwise active the first available objective
				{
					this.m_Objectives[index].Activate();
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
