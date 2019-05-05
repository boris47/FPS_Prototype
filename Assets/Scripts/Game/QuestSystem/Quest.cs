
using UnityEngine;

namespace QuestSystem {

	using System.Collections.Generic;

	public	enum QuestStatus {
		NONE,
		ACTIVE,
		COMPLETED
	};

	public	enum QuestScope {
		NONE,
		LOCAL,
		GLOBAL
	}

	public interface IQuest : IStateDefiner<IQuestManager, IQuest> {

		bool			Activate();

		int				TaskCount		{ get; }

		bool			IsCompleted		{ get; }
		QuestStatus		Status			{ get; }
		QuestScope		Scope			{ get; }

		bool			AddTask			( Task newTask );

		bool			RemoveTask		( Task task );
	}

	public class Quest : MonoBehaviour, IQuest {
		
		[SerializeField]
		private	List<Task>					m_Tasks						= new List<Task>();

		[SerializeField]
		private	GameEvent					m_OnCompletion				= null;

		private	System.Action<IQuest>		m_OnCompletionCallback		= delegate { };
		private	bool						m_IsCompleted				= false;
		private	bool						m_IsInitialized				= false;


		private	QuestStatus					m_Status					= QuestStatus.NONE;
		private	QuestScope					m_Scope						= QuestScope.LOCAL;

		//--
		int				IQuest.TaskCount
		{
			get { return m_Tasks.Count; }
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
		// Initialize ( IStateDefiner )
		public			bool		Initialize( IQuestManager manager, System.Action<IQuest> onCompletionCallback, System.Action<IQuest> dump  )
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool result = false;

			// Already assigned
			foreach( ITask t in m_Tasks )
			{
				result &= t.Initialize( this, OnTaskCompleted );
			}
			m_OnCompletionCallback = onCompletionCallback;

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

			newTask.Initialize( this, OnTaskCompleted, null );
			m_Tasks.Add( newTask );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// RemoveTask ( IQuest )
		bool			IQuest.RemoveTask( Task task )
		{
			if ( task == null )
				return false;

			if ( m_Tasks.Contains( task ) == false )
				return true;

			m_Tasks.Remove( task );
			return true;
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

			m_Status = QuestStatus.COMPLETED;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		private	void	OnTaskCompleted( ITask task )
		{
			bool bAreTasksCompleted = m_Tasks.TrueForAll( ( Task t ) => { return t.IsCompleted == true; } );
			if ( bAreTasksCompleted == false )
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

			m_Status = QuestStatus.ACTIVE;
			m_Tasks[ 0 ].Activate();
			return true;
		}
		
	}

}
