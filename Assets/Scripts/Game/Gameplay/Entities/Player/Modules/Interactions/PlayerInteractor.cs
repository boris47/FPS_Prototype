
using UnityEngine;

namespace Entities.Player.Components
{
	[RequireComponent(typeof(PlayerEntity))]
	public abstract class PlayerInteractor : PlayerEntityComponent, IInteractor
	{
		public delegate void OnInteractableDel(PlayerInteractor interactor, Interactable interactable);
		
		public class InteractorPriorityComparer : System.Collections.Generic.IComparer<PlayerInteractor>
		{
			public int Compare(PlayerInteractor x, PlayerInteractor y)
			{
				return x.Priority < y.Priority ? -1 : 1;
			}
		}
		public static			InteractorPriorityComparer		Comparer							= new InteractorPriorityComparer();


		[SerializeField, ReadOnly]
		private					Interactable					m_CurrentInteractable				= null;

		private event			OnInteractableDel				m_OnInteractableFound					= delegate { };
		private event			OnInteractableDel				m_OnInteractableLost					= delegate { };

		protected				Interactable					CurrentInteractable					=> m_CurrentInteractable;

		public event			OnInteractableDel				OnInteractableFound
		{
			add		{ if (value.IsNotNull()) m_OnInteractableFound += value; }
			remove	{ if (value.IsNotNull()) m_OnInteractableFound -= value; }
		}

		public event			OnInteractableDel				OnInteractableLost
		{
			add		{ if (value.IsNotNull()) m_OnInteractableLost += value; }
			remove	{ if (value.IsNotNull()) m_OnInteractableLost -= value; }
		}

		public abstract			uint							Priority							{ get; }
		private bool m_IsInteracting = false;


		//////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();

			m_CurrentInteractable = null;
		}

		//////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();

			m_CurrentInteractable = null;
		}

		//////////////////////////////////////////////////////////////////
		private void FixedUpdate()
		{
			if (HasInteractableAvailable() && !CanInteractWithCurrent())
			{
				OnInteractableLostInternal(m_CurrentInteractable);
			}
		}


		//////////////////////////////////////////////////////////////////
		// IInteractor START
		public bool HasInteractableAvailable() => m_CurrentInteractable.IsNotNull();
		public Interactable GetCurrentInteractable() => m_CurrentInteractable;
		public bool IsCurrentlyInteracting() => m_IsInteracting;
		public abstract bool CanInteractWith(Interactable interactable);
		public void InteractionStart()
		{
			if (m_CurrentInteractable.InteractionStage == EInteractionStages.READY)
			{
				InteractionStartInternal();
				m_IsInteracting = true;
			}
		}
		public void InteractionUpdate(float deltaTime)
		{	
			if (m_CurrentInteractable.InteractionStage == EInteractionStages.READY)
			{
				if (!m_IsInteracting)
				{
					// Is ready so execute interaction
					InteractionStartInternal();
					m_IsInteracting = true;
				}
			}
			if (m_CurrentInteractable.InteractionStage == EInteractionStages.INTERACTING)
			{
				m_CurrentInteractable.EvaluateRepeat(m_Owner, deltaTime);
			}
			if (m_CurrentInteractable.InteractionStage == EInteractionStages.LOADING)
			{
				m_CurrentInteractable.Load(deltaTime);
			}

			if (m_CurrentInteractable.InteractionStage == EInteractionStages.NONE)
			{
				m_CurrentInteractable.Load(deltaTime);
			}
		}
		public void InteractionEnd()
		{
			if (m_IsInteracting)
			{
				// Ineractable was being interacted with so request internal interaction stop
				InteractionStopInternal();
			}

			// Interactable may be loaded so reset notify stop load for internal handling
			m_CurrentInteractable.AbortLoad();
			m_IsInteracting = false;
		}
		// IInteractor END
		//////////////////////////////////////////////////////////////////


		//////////////////////////////////////////////////////////////////
		public bool CanInteractWithCurrent() => CanInteractWith(m_CurrentInteractable);

		//////////////////////////////////////////////////////////////////
		protected abstract void InteractionStartInternal();
		//////////////////////////////////////////////////////////////////
		protected abstract void InteractionStopInternal();

		//////////////////////////////////////////////////////////////////
		protected void OnInteractableFoundInternal(Interactable interactable)
		{
			if (m_CurrentInteractable.IsNotNull())
			{
				m_CurrentInteractable.OnInteractionEnd(m_Owner);
			}
			m_CurrentInteractable = interactable;
			m_OnInteractableFound(this, interactable);
		}

		//////////////////////////////////////////////////////////////////
		protected void OnInteractableLostInternal(Interactable interactable)
		{
			InteractionEnd();

			m_CurrentInteractable = null;
			m_OnInteractableLost(this, interactable);
		}
	}
}
