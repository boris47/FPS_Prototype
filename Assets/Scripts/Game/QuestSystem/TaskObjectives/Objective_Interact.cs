
using UnityEngine;

namespace QuestSystem {

	public class Objective_Interact : Objective_Base
	{	
		[SerializeField]
		private	Interactable	m_Interactable = null;


		//////////////////////////////////////////////////////////////////////////
		protected override bool InitializeInternal(Task motherTask, System.Action<Objective_Base> onCompletionCallback, System.Action<Objective_Base> onFailureCallback)
		{
			if (!m_IsInitialized)
			{
				if (CustomAssertions.IsTrue(Utils.Base.TrySearchComponent(gameObject, ESearchContext.LOCAL, out m_Interactable)))
				{
					//m_Interactable.CanInteract = true;
					m_Interactable.OnInteractionCallback += OnInteraction;
					m_Interactable.OnRetroInteractionCallback += OnRetroInteraction;

					m_OnCompletionCallback = onCompletionCallback;
					m_OnFailureCallback = onFailureCallback;
					motherTask.AddObjective(this);
				}

				m_IsInitialized = true;
			}
			return m_IsInitialized;
		}


		//////////////////////////////////////////////////////////////////////////
		public		override	bool		ReInit()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		public		override	bool		Finalize()
		{
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		public override void OnSave( StreamUnit streamUnit )
		{
			
		}


		//////////////////////////////////////////////////////////////////////////
		public override void OnLoad( StreamUnit streamUnit )
		{
			
		}


		//////////////////////////////////////////////////////////////////////////
		protected override void ActivateInternal()
		{
			UIManager.Indicators.AddIndicator(m_Interactable.Collider.gameObject, EIndicatorType.OBJECT_TO_INTERACT, bMustBeClamped: true);
		}


		//////////////////////////////////////////////////////////////////////////
		protected override void DeactivateInternal()
		{
			UIManager.Indicators.RemoveIndicator(gameObject);
		}


		//////////////////////////////////////////////////////////////////////////
		private void	OnInteraction()
		{
			Deactivate();

			OnObjectiveCompleted();
		}


		//////////////////////////////////////////////////////////////////////////
		private void OnRetroInteraction()
		{
			// Require dependencies to be completed
			if (m_Dependencies.Count > 0 && m_Dependencies.FindIndex(o => o.IsCompleted == false) > -1)
			{
				// Our dependencies ask for this objective to be completed, so we are going to deactivate them
				m_Dependencies.ForEach(d => d.Deactivate());

				// and activate again this
				Activate();

				m_IsCompleted = false;
			}
		}
	}
}