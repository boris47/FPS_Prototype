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
			if (this.m_IsInitialized == true )
				return true;

			this.m_IsInitialized = true;

			this.m_OnCompletionCallback = onCompletionCallback;
			this.m_OnFailureCallback = onFailureCallback;
			motherTask.AddObjective( this );
			
			Entity entity = null;
			bool bIsEntity = Utils.Base.SearchComponent(this.gameObject, ref entity, ESearchContext.LOCAL );
			if ( bIsEntity )
			{
				entity.OnEvent_Killed += this.OnKill;
			}

			bool bIsGoodResult = true;
			{
				if (this.m_ObjectToFollow == null )
				{
					this.m_ObjectToFollow = this.gameObject;
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
			UIManager.Indicators.EnableIndicator(this.m_ObjectToFollow, EIndicatorType.OBJECT_TO_FOLLOW, bMustBeClamped: true );
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected		override	void		DeactivateInternal()
		{
			UIManager.Indicators.DisableIndicator(this.m_ObjectToFollow );
		}


		//////////////////////////////////////////////////////////////////////////
		// OnFollowDoneCompleted
		public	void	OnFollowDoneCompleted()
		{
			this.Deactivate();

			this.OnObjectiveCompleted();
		}




		//////////////////////////////////////////////////////////////////////////
		// OnKill
		private void OnKill( Entity entityKilled )
		{
			this.Deactivate();

			this.OnObjectiveFailed();
			//OnObjectiveCompleted();
		}

	}

}