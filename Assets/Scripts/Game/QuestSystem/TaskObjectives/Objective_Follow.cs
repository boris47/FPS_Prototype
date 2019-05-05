using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem {

	public class Objective_Follow : Objective_Base {
		
		[SerializeField]
		private		GameObject	m_ObjectToFollow = null;

		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( ITask motherTask, System.Action<IObjective> onCompletionCallback, System.Action<IObjective> onFailureCallback )
		{
			if ( m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			m_OnCompletionCallback = onCompletionCallback;
			m_OnFailureCallback = onFailureCallback;
			motherTask.AddObjective( this );
			
			Entity entity = null;
			bool bIsEntity = Utils.Base.SearchComponent( gameObject, ref entity, SearchContext.LOCAL );
			if ( bIsEntity )
			{
				entity.OnEvent_Killed += OnKill;
			}

			bool bIsGoodResult = true;
			{
				if ( m_ObjectToFollow == null )
				{
					m_ObjectToFollow = gameObject;
				}
			}
			return bIsGoodResult;
		}


		//////////////////////////////////////////////////////////////////////////
		// ReInit ( IStateDefiner )
		public		override	bool		ReInit()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// Finalize ( IStateDefiner )
		public		override	bool		Finalize()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		// OnSave
		public override void OnSave( StreamUnit streamUnit )
		{
			
		}


		//////////////////////////////////////////////////////////////////////////
		// OnLoad
		public override void OnLoad( StreamUnit streamUnit )
		{
			
		}


		//////////////////////////////////////////////////////////////////////////
		// Activate ( IObjective )
		protected		override	void		ActivateInternal()
		{
			UI.Instance.Indicators.EnableIndicator( m_ObjectToFollow, IndicatorType.OBJECT_TO_FOLLOW );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			UI.Instance.Indicators.DisableIndicator( m_ObjectToFollow );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnFollowDoneCompleted
		public	void	OnFollowDoneCompleted()
		{
			Deactivate();

			OnObjectiveCompleted();
		}




		//////////////////////////////////////////////////////////////////////////
		// OnKill
		private void OnKill()
		{
			Deactivate();

			OnObjectiveFailed();
			//OnObjectiveCompleted();
		}

	}

}