
using UnityEngine;

namespace QuestSystem {

	using System.Collections.Generic;

	public	enum QuestStatus {
		NONE,
		ASSIGNED,
		ACTIVE,
		COMPLETED
	};

	public	enum QuestScope {
		NONE,
		LOCAL,
		GLOBAL
	}

	public interface IQuest {

		bool			Activate();

		bool			IsCompleted		{ get; }
		QuestStatus		Status			{ get; }
		QuestScope		Scope			{ get; }

		bool			AddTask			( ITask newTask );

		void			RegisterOnCompletion( System.Action<IQuest>	onCompletionCallback );

		int				GetTasksCount	();

	}

	public class Quest : MonoBehaviour, IQuest {
		
		[SerializeField]
		private	List<Task>					m_Tasks						= new List<Task>();

		[SerializeField]
		private	GameEvent					m_OnCompletion				= null;


		private	System.Action<IQuest>		m_OnCompletionCallback		= delegate { };
		private	bool						m_IsCompleted				= false;
		private	QuestStatus					m_Status					= QuestStatus.ASSIGNED;
		private	QuestScope					m_Scope						= QuestScope.LOCAL;


		//--
		bool		IQuest.IsCompleted
		{
			get { return m_IsCompleted; }
		}
		//--
		QuestStatus		IQuest.Status
		{
			get { return m_Status; }
		}
		//--
		QuestScope		IQuest.Scope
		{
			get { return m_Scope; }
		}
		


		//////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void Awake()
		{
			// Already assigned
			foreach( ITask t in m_Tasks )
			{
//				t.AddToQuest( this );
				t.RegisterOnCompletion( OnTaskCompleted );
			}
//			LocalQuestManager.Instance.AddQuest( this );

			/*
			// Child Task
			Task[] childTasks = GetComponentsInChildren<Task>();
			if ( childTasks.Length > 0 )
			{
				foreach( ITask t in childTasks )
				{
					t.AddToQuest( this );
					t.RegisterOnCompletion( OnTaskCompleted );
				}

				m_Tasks.AddRange( childTasks );
				m_Tasks[0].Activate();
			}
			*/
		}


		//////////////////////////////////////////////////////////////////////////
		// AddTask ( Interface )
		bool			IQuest.AddTask( ITask newTask )
		{
			if ( newTask == null )
				return false;

			m_Tasks.Add( newTask as Task );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// RegisterOnCompletion ( Interface )
		void			IQuest.RegisterOnCompletion( System.Action<IQuest>	onCompletionCallback )
		{
			m_OnCompletionCallback = onCompletionCallback;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetTasksCount ( Interface )
		int				IQuest.GetTasksCount()
		{
			return m_Tasks.Count;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		private	void	OnTaskCompleted( ITask task )
		{
			bool bIsQuestCompleted = true;
			foreach( ITask t in m_Tasks )
			{
				bIsQuestCompleted &= t.IsCompleted;
			}

			if ( bIsQuestCompleted )
			{
				// Internal Flag
				m_Status = QuestStatus.COMPLETED;

				m_IsCompleted = true;

				if ( GlobalQuestManager.ShowDebugInfo )
					print( "Completed quest " + name );

				// Unity Events
				if ( m_OnCompletion != null && m_OnCompletion.GetPersistentEventCount() > 0 )
					m_OnCompletion.Invoke();

				// Internal Delegates
				m_OnCompletionCallback(this);
			}
			else
			{
				int index = m_Tasks.IndexOf( task as Task );
				int nextIndex = ++index;
				if ( nextIndex < m_Tasks.Count )
				{
					m_Tasks[ nextIndex ].Activate();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate
		public	bool	Activate()
		{
			if ( m_Tasks.Count == 0 )
			{
				return false;
			}

			if ( GlobalQuestManager.ShowDebugInfo )
				print( name + " quest activated" );

			m_Tasks[ 0 ].Activate();
			return true;
		}
		
	}

}
