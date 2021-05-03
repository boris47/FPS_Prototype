using UnityEngine;

namespace QuestSystem
{

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

			if ( Utils.Base.TrySearchComponent( gameObject, ESearchContext.LOCAL, out Entity entity ) )
			{
				entity.OnEvent_Killed += OnKill;
			}
			
			return m_ObjectToFollow.IsNotNull();
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
		protected override void ActivateInternal()
		{
			UIManager.Indicators.AddIndicator(m_ObjectToFollow, EIndicatorType.OBJECT_TO_FOLLOW, bMustBeClamped: true);
		}


		//////////////////////////////////////////////////////////////////////////
		// Deactivate ( IObjective )
		protected override void DeactivateInternal()
		{
			UIManager.Indicators.RemoveIndicator(m_ObjectToFollow);
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