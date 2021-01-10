using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem {

	public class Objective_Follow : Objective_Base
	{	
		[SerializeField]
		private		GameObject	m_ObjectToFollow = null;

		//////////////////////////////////////////////////////////////////////////
		// Initialize ( IStateDefiner )
		protected		override	bool		InitializeInternal( Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback )
		{
			if (m_IsInitialized == true )
				return true;

			m_IsInitialized = true;

			m_OnCompletionCallback = onCompletionCallback;
			m_OnFailureCallback = onFailureCallback;
			motherTask.AddObjective( this );

			if ( Utils.Base.SearchComponent( gameObject, out Entity entity, ESearchContext.LOCAL ) )
			{
				entity.OnEvent_Killed += OnKill;
			}

			bool bIsGoodResult = true;
			{
				if (m_ObjectToFollow == null )
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
			UIManager.Indicators.EnableIndicator(m_ObjectToFollow, EIndicatorType.OBJECT_TO_FOLLOW, bMustBeClamped: true );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			UIManager.Indicators.DisableIndicator(m_ObjectToFollow );
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
		private void OnKill( Entity entityKilled )
		{
			Deactivate();

			OnObjectiveFailed();
			//OnObjectiveCompleted();
		}

	}

}