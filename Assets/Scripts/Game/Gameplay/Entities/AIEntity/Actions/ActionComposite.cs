
using System.Collections.Generic;
using UnityEngine;


namespace Entities.AI.Actions
{
	public abstract class ActionComposite : ActionBase
	{
		[SerializeField, ReadOnly]
		protected				List<ActionBase>			m_Actions							= new List<ActionBase>();

		[SerializeField, ReadOnly]
		protected				uint						m_CurrentIndex						= 0u;

		[SerializeField, ReadOnly]
		protected				bool						m_MustRepeat						= false;


		//////////////////////////////////////////////////////////////////////////
		public void OverrideActiveChildIndex(in uint InChildIndex)
		{
			m_CurrentIndex = (uint)Mathf.Clamp(InChildIndex, 0, m_Actions.Count - 1);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EActionState OnActivation()
		{
			EActionState OutResult = EActionState.RUNNING;
			m_CurrentIndex = 0u;
			return OutResult;
		}

		//////////////////////////////////////////////////////////////////////////
		protected override EActionState OnUpdateAborting()
		{
			return m_Actions.At(m_CurrentIndex).Update();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnTerminate()
		{
			base.OnTerminate();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnAbortRequested(in bool bAbortImmediately)
		{
			m_Actions.At(m_CurrentIndex).RequestAbort(bAbortImmediately);
		}

		//////////////////////////////////////////////////////////////////////////
		protected sealed override void OnReset()
		{
			m_CurrentIndex = 0u;
			m_Actions.ForEach(c => c.ResetAction());
		}
	}
}
