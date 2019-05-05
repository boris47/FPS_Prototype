
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
			get { return name; }
		}

		//--
		public bool		IsCompleted
		{
			get { return m_IsCompleted; }
		}

		//--
		public bool IsInitialized	// IStateDefiner
		{
			get { return m_IsInitialized; }
		}


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public				bool		Initialize( IQuest motherQuest, System.Action<ITask> onCompletionCallback, System.Action<ITask> dump )
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool result = false;


			// Already assigned
			if ( m_Objectives.Count > 0 )
			{
				foreach( IObjective o in m_Objectives )
				{
					result &= o.Initialize( this, OnObjectiveCompleted, OnObjectiveFailed ); // Init every Objective
				}

				if ( m_Objectives[ m_Objectives.Count - 1 ].IsOptional )
				{
					Debug.Log( "WARNIGN: Task " + name + " has last objective set as optional" );
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
			m_Objectives.ForEach( o => o.OnSave( streamUnit ) );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnLoad
		public	virtual		void		OnLoad( StreamUnit streamUnit )
		{
			m_Objectives.ForEach( o => o.OnLoad( streamUnit ) );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		private void	OnTaskCompleted()
		{
			// Internal Flag
			m_IsCompleted = true;

			m_IsCurrentlyActive = false;

			if ( GlobalQuestManager.ShowDebugInfo )
				print( "Completed Task " + name );

			// Unity Events
			if ( m_OnCompletion != null && m_OnCompletion.GetPersistentEventCount() > 0 )
				m_OnCompletion.Invoke();

			// Internal Delegates
			m_OnCompletionCallback( this );

			Finalize();
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted
		private	void	OnObjectiveCompleted( IObjective objective )
		{
			objective.Deactivate();

			bool bAreObjectivesCompleted = m_Objectives.TrueForAll( ( Objective_Base o ) => { return o.IsOptional == false && o.IsCompleted == true; } );
			if ( bAreObjectivesCompleted == false )
			{
				IObjective nextObjective = m_Objectives.Find( o => o.IsOptional == false && o.IsCompleted == false );
				if ( m_IsCurrentlyActive && nextObjective != null )
				{
					if ( nextObjective.IsCurrentlyActive == false && nextObjective.IsCompleted == false )
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
		private	void	OnObjectiveFailed( IObjective objective )
		{
			objective.Deactivate();

			bool bAreObjectivesCompleted = m_Objectives.TrueForAll( ( Objective_Base o ) => { return o.IsOptional == false && o.IsCompleted == true; } );
			if ( bAreObjectivesCompleted == false )
			{
				IObjective nextObjective = m_Objectives.Find( o => o.IsOptional == false && o.IsCompleted == false );
				if ( m_IsCurrentlyActive && nextObjective != null )
				{
					if ( nextObjective.IsCurrentlyActive == false && nextObjective.IsCompleted == false )
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
		bool	 ITask.AddObjective( Objective_Base newObjective )
		{
			if ( newObjective == null )
				return false;

			if ( m_Objectives.Contains( newObjective ) == true )
				return true;

			newObjective.Initialize( this, OnObjectiveCompleted, OnObjectiveFailed );
			m_Objectives.Add( newObjective );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// RemoveObjective ( ITask )
		bool	ITask.RemoveObjective( Objective_Base objective )
		{
			if ( objective == null )
				return false;

			if ( m_Objectives.Contains( objective ) == false )
				return false;

			m_Objectives.Remove( objective );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate
		public	bool	Activate()
		{
			if ( m_Objectives.Count == 0 )
			{
				return false;
			}

			m_IsCurrentlyActive = true;

			if ( GlobalQuestManager.ShowDebugInfo )
				print( name + " task activation" );

			{
				int index = m_Objectives.FindIndex( o => o.IsCompleted == false );

				// If task is completed on it's activation call for completion
				if ( index == -1 )
				{
					OnTaskCompleted();
				}
				else // Otherwise active the first available objective
				{
					m_Objectives[index].Activate();
				}
			}
			{
				int index = m_Objectives.FindIndex( o => o.IsOptional == false && o.IsCompleted == false );

				// If task is completed on it's activation call for completion
				if ( index == -1 )
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
