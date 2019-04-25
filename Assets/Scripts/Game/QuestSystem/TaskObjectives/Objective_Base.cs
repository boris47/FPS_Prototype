

using UnityEngine;

namespace QuestSystem {

	public interface IObjective : IStateDefiner {

		bool			IsCompleted				{get; }

		void			AddToTask				( ITask task );

		void			Activate();

		void			Deactivate();

		void			RegisterOnCompletion	( System.Action<Objective_Base>	onCompletionCallback );

		void			OnObjectiveCompleted	();
	}


	public abstract class Objective_Base : MonoBehaviour, IObjective {

		[SerializeField]
		private GameEvent							m_OnCompletion				= null;

		protected	System.Action<Objective_Base>	m_OnCompletionCallback		= delegate { };
		protected	bool							m_IsCompleted				= false;
		protected	bool							m_IsCurrentlyActive			= false;

		protected	bool							m_IsInitialized				= false;

		//--
		bool			IObjective.IsCompleted
		{
			get { return m_IsCompleted; }
		}

		//--
		public bool IsInitialized	// IStateDefiner
		{
			get { return m_IsInitialized; }
		}



		//////////////////////////////////////////////////////////////////////////
		public	abstract	bool		Initialize();

		//////////////////////////////////////////////////////////////////////////
		public	abstract	bool		ReInit();

		//////////////////////////////////////////////////////////////////////////
		public	abstract	bool		Finalize();

		//////////////////////////////////////////////////////////////////////////
		public	abstract	void		Activate();

		//////////////////////////////////////////////////////////////////////////
		public	abstract	void		Deactivate();
		

		//////////////////////////////////////////////////////////////////////////
		// SetTaskOwner ( Interface )
		void			IObjective.AddToTask( ITask task )
		{
			task.AddObjective( this );
		}


		//////////////////////////////////////////////////////////////////////////
		// RegisterOnCompletion ( Interface )
		void		IObjective.RegisterOnCompletion( System.Action<Objective_Base>	onCompletionCallback )
		{
			m_OnCompletionCallback = onCompletionCallback;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnObjectiveCompleted ( Interface )
		public	void			OnObjectiveCompleted()
		{
			// Internal Flag
			m_IsCompleted = true;

			// Unity Events
			if ( m_OnCompletion != null && m_OnCompletion.GetPersistentEventCount() > 0 )
				m_OnCompletion.Invoke();

			// Internal Delegates
			m_OnCompletionCallback( this );

			print( "Completed Objective " + name );
		}

		
		
		/*
		//////////////////////////////////////////////////////////////////////////
		private void OnGUIo()
		{
			if ( m_IsTextureLoaded && m_IsCurrentlyActive && GameManager.IsPaused == false )
			{
				DrawUIElementOnObjectives( transform, m_Texture.texture, ref m_DrawRect );

				m_IconTransform.position = m_DrawRect;

//				GUI.DrawTexture( m_DrawRect, m_Texture );
			}
		}
		*/
	}

}