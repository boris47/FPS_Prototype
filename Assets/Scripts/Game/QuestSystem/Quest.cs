
using UnityEngine;

namespace QuestSystem {

	using System.Collections.Generic;

	public	enum EQuestStatus {
		NONE,
		ACTIVE,
		COMPLETED
	};

	public	enum EQuestScope {
		NONE,
		LOCAL,
		GLOBAL
	}

	public interface IQuest : IStateDefiner<IQuestManager, IQuest> {

		bool			Activate();

		int				TaskCount		{ get; }

		bool			IsCompleted		{ get; }
		EQuestStatus		Status			{ get; }
		EQuestScope		Scope			{ get; }

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


		private	EQuestStatus					m_Status					= EQuestStatus.NONE;
		private	EQuestScope					m_Scope						= EQuestScope.LOCAL;

		//--
		int				IQuest.TaskCount
		{
			get { return this.m_Tasks.Count; }
		}

		//--
		public	bool		IsCompleted	// IQuest
		{
			get { return this.m_IsCompleted; }
		}

		//--
		public bool		IsInitialized	// IStateDefiner
		{
			get { return this.m_IsInitialized; }
		}

		//--
		EQuestStatus		IQuest.Status
		{
			get { return this.m_Status; }
		}

		//--
		EQuestScope		IQuest.Scope
		{
			get { return this.m_Scope; }
		}

		string IStateDefiner<IQuestManager, IQuest>.StateName
		{
			get { return this.name; }
		}
		


		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		public			bool		Initialize( IQuestManager manager, System.Action<IQuest> onCompletionCallback, System.Action<IQuest> dump  )
		{
			if (this.m_IsInitialized == true )
				return true;

			this.m_IsInitialized = true;

			bool result = false;

			// Already assigned
			foreach( ITask t in this.m_Tasks )
			{
				result &= t.Initialize( this, this.OnTaskCompleted );
			}
			this.m_OnCompletionCallback = onCompletionCallback;

			// Registering game events
			GameManager.StreamEvents.OnSave += this.OnSave;
			GameManager.StreamEvents.OnLoad += this.OnLoad;

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
			GameManager.StreamEvents.OnSave -= this.OnSave;
			GameManager.StreamEvents.OnLoad -= this.OnLoad;
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnSave
		protected	virtual		StreamUnit		OnSave( StreamData streamData )
		{
			StreamUnit streamUnit	= streamData.NewUnit(this.gameObject );
			{
				this.m_Tasks.ForEach( t => t.OnSave( streamUnit ) );
			}
			return streamUnit;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnLoad
		protected	virtual		StreamUnit		OnLoad( StreamData streamData )
		{
			StreamUnit streamUnit = null;
			if ( streamData.GetUnit(this.gameObject, ref streamUnit ) )
			{
				this.m_Tasks.ForEach( t => t.OnLoad( streamUnit ) );
			}
			return streamUnit;
		}
		

		//////////////////////////////////////////////////////////////////////////
		// AddTask ( IQuest )
		bool			IQuest.AddTask( Task newTask )
		{
			if ( newTask == null )
				return false;

			if (this.m_Tasks.Contains( newTask ) == true )
				return true;

			newTask.Initialize( this, this.OnTaskCompleted, null );
			this.m_Tasks.Add( newTask );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// RemoveTask ( IQuest )
		bool			IQuest.RemoveTask( Task task )
		{
			if ( task == null )
				return false;

			if (this.m_Tasks.Contains( task ) == false )
				return true;

			this.m_Tasks.Remove( task );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		private void OnQuestCompleted()
		{
			// Internal Flag
			this.m_Status = EQuestStatus.COMPLETED;

			this.m_IsCompleted = true;

			if ( GlobalQuestManager.ShowDebugInfo )
				print( "Completed quest " + this.name );

			// Unity Events
			if (this.m_OnCompletion != null && this.m_OnCompletion.GetPersistentEventCount() > 0 )
				this.m_OnCompletion.Invoke();

			// Internal Delegates
			this.m_OnCompletionCallback(this);

			this.Finalize();

			this.m_Status = EQuestStatus.COMPLETED;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnTaskCompleted
		private	void	OnTaskCompleted( ITask task )
		{
			bool bAreTasksCompleted = this.m_Tasks.TrueForAll( ( Task t ) => { return t.IsCompleted == true; } );
			if ( bAreTasksCompleted == false )
			{
				ITask nextTask = null;
				int nextIndex = (this.m_Tasks.IndexOf( task as Task ) + 1 );
				if ( nextIndex < this.m_Tasks.Count )
				{
					nextTask = this.m_Tasks[nextIndex];
				}
				// Some tasks are completed and sequence is broken, search for a valid target randomly among availables
				else
				{
					nextTask = this.m_Tasks.Find( t => t.IsCompleted == false ) as ITask;
				}

				nextTask.Activate();
				return;
			}

			// Only Called if trurly completed
			this.OnQuestCompleted();
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate
		public	bool	Activate()
		{
			if (this.m_Tasks.Count == 0 )
			{
				return false;
			}

			if ( GlobalQuestManager.ShowDebugInfo )
				print(this.name + " quest activated" );

			this.m_Status = EQuestStatus.ACTIVE;
			this.m_Tasks[ 0 ].Activate();
			return true;
		}
		
	}

}
