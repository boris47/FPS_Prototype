
using UnityEngine;

namespace QuestSystem {
	
	using System.Collections.Generic;

	public interface ITask : IStateDefiner<IQuest, ITask> {

		bool	Activate			();

		bool	Deactivate			();

		bool	IsCompleted			{ get; }

		bool	AddObjective		( Objective_Base newObjective );

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
		public				bool		Initialize( IQuest motherQuest, System.Action<ITask> onCompletionCallback )
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool result = false;

			// Already assigned
			foreach( IObjective o in m_Objectives )
			{
				result &= o.Initialize( this, OnObjectiveCompleted ); // Init every Objective
			}

//			motherQuest.AddTask( this );

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

			bool bIsTaskCompleted = true;
			foreach( IObjective o in m_Objectives )
			{
				bIsTaskCompleted &= o.IsCompleted;
			}

			if ( bIsTaskCompleted == false )
			{
				if ( m_IsCurrentlyActive )
				{
					IObjective nextObjective = null;
					int nextIndex = ( m_Objectives.IndexOf( objective as Objective_Base ) + 1 );
					if ( nextIndex < m_Objectives.Count )
					{
						nextObjective = m_Objectives[ nextIndex ];
					}
					// Some objectives are completed and sequence is broken, search for a valid target randomly among availables
					else
					{
						nextObjective = m_Objectives.Find( o => o.IsCompleted == false ) as IObjective;
					}
					if ( nextObjective.IsCurrentlyActive == false && nextObjective.IsCompleted == false )
					{
						nextObjective.Activate();
					}
				}
				return;
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

			newObjective.Initialize( this, OnObjectiveCompleted );
			m_Objectives.Add( newObjective );
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
