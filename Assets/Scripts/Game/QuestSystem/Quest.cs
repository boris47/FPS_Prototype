
using UnityEngine;

namespace QuestSystem {

	using System.Collections.Generic;

	[System.Serializable]
	public	enum EQuestStatus
	{
		NONE,
		ACTIVE,
		COMPLETED
	};

	[System.Serializable]
	public	enum EQuestScope
	{
		NONE,
		LOCAL,
		GLOBAL
	}


	public class Quest : MonoBehaviour
	{
	
		[SerializeField]
		private	List<Task>					m_Tasks						= new List<Task>();

		[SerializeField]
		private	GameEvent					m_OnCompletion				= null;

		private	System.Action<Quest>		m_OnCompletionCallback		= delegate { };

		[System.NonSerialized]
		private	bool						m_IsCompleted				= false;
		[System.NonSerialized]
		private	bool						m_IsInitialized				= false;

		[System.NonSerialized]
		private	EQuestStatus				m_Status					= EQuestStatus.NONE;
		[System.NonSerialized]
		private	EQuestScope					m_Scope						= EQuestScope.LOCAL;

		//--
		public int							TaskCount					=> m_Tasks.Count;

		// IQuest
		public bool							IsCompleted					=> m_IsCompleted;

		// IStateDefiner
		public bool							IsInitialized				=> m_IsInitialized;

		//--
		public EQuestStatus					Status						=> m_Status;

		//--
		public EQuestScope					Scope						=> m_Scope;

		



		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public			bool		Initialize( System.Action<Quest> onCompletionCallback, System.Action<Quest> onFailureCallback  )
		{
			if (m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			bool result = false;

			// Already assigned
			foreach( Task t in m_Tasks )
			{
				result &= t.Initialize( this, OnTaskCompleted, null);
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
			StreamUnit streamUnit	= streamData.NewUnit(gameObject );
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
			if ( streamData.GetUnit(gameObject, ref streamUnit ) )
			{
				m_Tasks.ForEach( t => t.OnLoad( streamUnit ) );
			}
			return streamUnit;
		}
		

		//////////////////////////////////////////////////////////////////////////
		// AddTask ( IQuest )
		public bool			AddTask( Task newTask )
		{
			if ( newTask == null )
				return false;

			if (m_Tasks.Contains( newTask ) == true )
				return true;

			newTask.Initialize( this, OnTaskCompleted, null );
			m_Tasks.Add( newTask );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// RemoveTask ( IQuest )
		public bool			RemoveTask( Task task )
		{
			if ( task == null )
				return false;

			if (m_Tasks.Contains( task ) == false )
				return true;

			m_Tasks.Remove( task );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		private void OnQuestCompleted()
		{
			// Internal Flag
			m_Status = EQuestStatus.COMPLETED;

			m_IsCompleted = true;

			if ( GlobalQuestManager.ShowDebugInfo )
				print( "Completed quest " + name );

			// Unity Events
			if (m_OnCompletion != null && m_OnCompletion.GetPersistentEventCount() > 0 )
				m_OnCompletion.Invoke();

			// Internal Delegates
			m_OnCompletionCallback(this);

			Finalize();

			m_Status = EQuestStatus.COMPLETED;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		private	void	OnTaskCompleted( Task task )
		{
			bool bAreTasksCompleted = m_Tasks.TrueForAll( ( Task t ) => { return t.IsCompleted == true; } );
			if ( bAreTasksCompleted == false )
			{
				Task nextTask = null;
				int nextIndex = (m_Tasks.IndexOf( task as Task ) + 1 );
				if ( nextIndex < m_Tasks.Count )
				{
					nextTask = m_Tasks[nextIndex];
				}
				// Some tasks are completed and sequence is broken, search for a valid target randomly among availables
				else
				{
					nextTask = m_Tasks.Find( t => t.IsCompleted == false );
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
			if (m_Tasks.Count == 0 )
			{
				return false;
			}

			if ( GlobalQuestManager.ShowDebugInfo )
				print(name + " quest activated" );

			m_Status = EQuestStatus.ACTIVE;
			m_Tasks[ 0 ].Activate();
			return true;
		}
		
	}

}
