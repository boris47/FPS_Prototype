
using UnityEngine;


namespace Entities.AI.Actions
{
	public enum EActionState
	{
		INACTIVE, RUNNING, ABORTING, FAILED, COMPLETED, ABORTED
	}

	public abstract class ActionBase
	{
		[SerializeField, ReadOnly]
		private EActionState m_ActionState = EActionState.INACTIVE;


		public EActionState ActionState => m_ActionState;


		//////////////////////////////////////////////////////////////////////////
		public static bool IsFinished(in ActionBase InActionBase) => InActionBase.m_ActionState >= EActionState.FAILED;

		//////////////////////////////////////////////////////////////////////////
		public void SetActionState(in EActionState InActionState)
		{
			m_ActionState = InActionState;
		}


		//////////////////////////////////////////////////////////////////////////
		public EActionState Update()
		{
			if (m_ActionState == EActionState.INACTIVE)
			{
				m_ActionState = OnActivation();
				Utils.CustomAssertions.IsTrue(m_ActionState != EActionState.INACTIVE);
			}

			if (m_ActionState == EActionState.RUNNING)
			{
				m_ActionState = OnUpdate();
			}

			if (m_ActionState == EActionState.ABORTING)
			{
				m_ActionState = OnUpdateAborting();
				Utils.CustomAssertions.IsTrue(m_ActionState == EActionState.ABORTING || m_ActionState == EActionState.ABORTED);
			}

			if (ActionBase.IsFinished(this))
			{
				OnTerminate();
			}
			return m_ActionState;
		}

		//////////////////////////////////////////////////////////////////////////
		public void RequestAbort(in bool bAbortImmediately)
		{
			m_ActionState = EActionState.ABORTING;

			OnAbortRequested(bAbortImmediately);

			if (bAbortImmediately)
			{
				m_ActionState = EActionState.ABORTED;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void ResetAction()
		{
			m_ActionState = EActionState.INACTIVE;

			OnReset();
		}


		//////////////////////////////////////////////////////////////////////////
		protected virtual EActionState OnActivation() => EActionState.RUNNING;
		protected virtual EActionState OnUpdate() => EActionState.COMPLETED;
		protected virtual EActionState OnUpdateAborting() => EActionState.ABORTED;
		protected virtual void OnTerminate() { }
		protected virtual void OnAbortRequested(in bool bAbortImmediately) { }
		protected virtual void OnReset() { }

	}
}
