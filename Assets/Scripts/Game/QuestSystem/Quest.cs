
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

	public interface IQuest : IStateDefiner {

		bool			Activate();

		bool			IsCompleted		{ get; }
		QuestStatus		Status			{ get; }
		QuestScope		Scope			{ get; }

		bool			AddTask			( Task newTask );

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
		private	bool						m_IsInitialized				= false;



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
		
		//--
		public	bool		IsCompleted	// IQuest
		{
			get { return m_IsCompleted; }
		}

		//--
		public bool		IsInitialized	// IStateDefiner
		{
			get { return m_IsInitialized; }
		}


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public			bool		Initialize()
		{
			bool result = false;

			// Already assigned
			foreach( ITask t in m_Tasks )
			{
				t.RegisterOnCompletion( OnTaskCompleted );
				result &= t.Initialize(this);
			}

			// Registering game events
			GameManager.StreamEvents.OnSave += OnSave;
			GameManager.StreamEvents.OnLoad += OnLoad;

			return result;
		}


		//////////////////////////////////////////////////////////////////////////
		// ReInit ( IStateDefiner )
		public			bool		ReInit()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Finalize ( IStateDefiner )
		public			bool		Finalize()
		{
			// UnRegistering game events
			GameManager.StreamEvents.OnSave -= OnSave;
			GameManager.StreamEvents.OnLoad -= OnLoad;
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnSave
		protected	virtual		StreamUnit		OnSave( StreamData streamData )
		{
			StreamUnit streamUnit	= streamData.NewUnit( gameObject );
			{
				m_Tasks.ForEach( t => t.OnSave( streamUnit ) );
			}
			return streamUnit;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnLoad
		protected	virtual		StreamUnit		OnLoad( StreamData streamData )
		{
			StreamUnit streamUnit = null;
			if ( streamData.GetUnit( gameObject, ref streamUnit ) )
			{
				m_Tasks.ForEach( t => t.OnLoad( streamUnit ) );
			}
			return streamUnit;
		}
		

		//////////////////////////////////////////////////////////////////////////
		// AddTask ( IQuest )
		bool			IQuest.AddTask( Task newTask )
		{
			if ( newTask == null )
				return false;

			if ( m_Tasks.Contains( newTask ) == true )
				return true;

			newTask.Initialize(this);
			m_Tasks.Add( newTask );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// RegisterOnCompletion ( IQuest )
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
		private void OnQuestCompleted()
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

			Finalize();
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

			if ( bIsQuestCompleted == false )
			{
				ITask nextTask = null;
				int nextIndex = ( m_Tasks.IndexOf( task as Task ) + 1 );
				if ( nextIndex < m_Tasks.Count )
				{
					nextTask = m_Tasks[nextIndex];
				}
				// Some tasks are completed and sequence is broken, search for a valid target randomly among availables
				else
				{
					nextTask = m_Tasks.Find( t => t.IsCompleted == false ) as ITask;
				}

				nextTask.Activate();
				return;
			}

			// Only Called if trurly completed
			OnQuestCompleted();

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
