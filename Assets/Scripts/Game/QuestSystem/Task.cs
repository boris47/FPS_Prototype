
using UnityEngine;

namespace QuestSystem {
	
	using System.Collections.Generic;

	public interface ITask {

		bool	IsCompleted			{ get; }

		void	AddToQuest			( IQuest quest );

		void	RegisterOnCompletion( System.Action<ITask>	onCompletionCallback );

		bool	AddObjective		( IObjective newObjective );

	}

	public class Task : MonoBehaviour, ITask {

		[SerializeField]
		private	List<Objective_Base>		m_Objectives			= new List<Objective_Base>();

		[SerializeField]
		private GameEvent					m_OnCompletion			= null;

		private	System.Action<ITask>		m_OnCompletionCallback	= delegate { };
		private	bool						m_IsCompleted			= false;
		private	bool						m_IsCurrentlyActive		= false;

		//--
		bool		ITask.IsCompleted
		{
			get { return m_IsCompleted; }
		}




		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			// Already assigned
			foreach( IObjective o in m_Objectives )
			{
				o.RegisterOnCompletion( OnObjectiveCompleted );
				o.Init(); // Init every Objective
			}
//			m_Objectives[0].Enable();
		}


		//////////////////////////////////////////////////////////////////////////
		// AddToQuest ( Interface )
		void	ITask.AddToQuest( IQuest quest )
		{
			quest.AddTask( this );
		}

		//////////////////////////////////////////////////////////////////////////
		// RegisterOnCompletion ( Interface )
		void	ITask.RegisterOnCompletion( System.Action<ITask>	onCompletionCallback )
		{
			m_OnCompletionCallback = onCompletionCallback;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted ( Interface )
		private	void	OnObjectiveCompleted( IObjective objective )
		{
			bool bIsTaskCompleted = true;
			foreach( IObjective o in m_Objectives )
			{
				bIsTaskCompleted &= o.IsCompleted;
			}

			if ( bIsTaskCompleted )
			{
				// Internal Flag
				m_IsCompleted = true;

				if ( GlobalQuestManager.ShowDebugInfo )
					print( "Completed Task " + name );

				// Unity Events
				if ( m_OnCompletion != null && m_OnCompletion.GetPersistentEventCount() > 0 )
					m_OnCompletion.Invoke();

				// Internal Delegates
				m_OnCompletionCallback( this );
			}
			else
			{
				int index = m_Objectives.IndexOf( objective as Objective_Base );
				int nextIndex = ++index;
				if ( nextIndex < m_Objectives.Count )
				{
					m_Objectives[ nextIndex ].Activate();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// AddObjective ( Interface )
		bool	 ITask.AddObjective( IObjective newObjective )
		{
			if ( newObjective == null )
				return false;

			m_Objectives.Add( newObjective as Objective_Base );
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

///			print( name + " task activated" );
			// Activate first objective
			m_Objectives[ 0 ].Activate();
			return true;
		}

		
	}

}
