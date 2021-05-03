using UnityEngine;

namespace QuestSystem
{
	public class Objective_Follow : Objective_Base
	{
		[SerializeField]
		private			GameObject			m_ObjectToFollow			= null;

		//////////////////////////////////////////////////////////////////////////
		protected override bool InitializeInternal(Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback)
		{
			if (!m_IsInitialized)
			{
				CustomAssertions.IsNotNull(m_ObjectToFollow);

				m_OnCompletionCallback = onCompletionCallback;
				m_OnFailureCallback = onFailureCallback;
				motherTask.AddObjective(this);

				if (Utils.Base.TrySearchComponent(m_ObjectToFollow, ESearchContext.LOCAL, out Entity entity))
				{
					entity.OnEvent_Killed += OnKill;
				}
				m_IsInitialized = true;
			}
			return m_IsInitialized;
		}


		//////////////////////////////////////////////////////////////////////////
		public override bool ReInit()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		public override bool Finalize()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		public override void OnSave(StreamUnit streamUnit)
		{

		}


		//////////////////////////////////////////////////////////////////////////
		public override void OnLoad(StreamUnit streamUnit)
		{

		}


		//////////////////////////////////////////////////////////////////////////
		protected override void ActivateInternal()
		{
			UIManager.Indicators.AddIndicator(m_ObjectToFollow, EIndicatorType.OBJECT_TO_FOLLOW, bMustBeClamped: true);
		}


		//////////////////////////////////////////////////////////////////////////
		protected override void DeactivateInternal()
		{
			UIManager.Indicators.RemoveIndicator(m_ObjectToFollow);
		}


		//////////////////////////////////////////////////////////////////////////
		public void OnFollowDoneCompleted()
		{
			Deactivate();

			OnObjectiveCompleted();
		}




		//////////////////////////////////////////////////////////////////////////
		private void OnKill(Entity entityKilled)
		{
			Deactivate();

			OnObjectiveFailed();
			//OnObjectiveCompleted();
		}
	}
}