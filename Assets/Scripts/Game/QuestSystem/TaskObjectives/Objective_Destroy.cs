
using UnityEngine;


namespace QuestSystem
{
	[RequireComponent(typeof(Entity))]
	public class Objective_Destroy : Objective_Base
	{
		[SerializeField]
		private			Entity				m_Target				= null;


		//////////////////////////////////////////////////////////////////////////
		protected override bool InitializeInternal(Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback)
		{
			if (!m_IsInitialized)
			{
				if (CustomAssertions.IsTrue(Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Target)))
				{
					m_Target.OnEvent_Killed += OnKill;

					m_OnCompletionCallback = onCompletionCallback;
					m_OnFailureCallback = onFailureCallback;
					motherTask.AddObjective(this);
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
		/// <summary> Set as current active to true and add indicator </summary>
		protected override void ActivateInternal()
		{
			UIManager.Indicators.AddIndicator(gameObject, EIndicatorType.TARGET_TO_KILL, bMustBeClamped: true);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Set as current active to false and remove indicator </summary>
		protected override void DeactivateInternal()
		{
			UIManager.Indicators.RemoveIndicator(gameObject);
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnKill(Entity entityKilled)
		{
			Deactivate();

			OnObjectiveCompleted();
		}
	}
}
