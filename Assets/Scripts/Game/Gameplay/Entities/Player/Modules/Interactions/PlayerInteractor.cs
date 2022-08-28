
using UnityEngine;

namespace Entities.Player.Components
{
	[RequireComponent(typeof(PlayerEntity))]
	public abstract class PlayerInteractor : PlayerEntityComponent, IInteractor
	{
		public delegate void OnInteractorDel(Interactable interactable);

		[SerializeField, ReadOnly]
		private				Interactable					m_CurrentInteractable				= null;

		private event		OnInteractorDel					m_OnInteractorFound					= delegate { };
		private event		OnInteractorDel					m_OnInteractorLost					= delegate { };
		protected			Interactable					CurrentInteractable					=> m_CurrentInteractable;

		public event		OnInteractorDel					OnInteractorFound
		{
			add		{ if (value.IsNotNull()) m_OnInteractorFound += value; }
			remove	{ if (value.IsNotNull()) m_OnInteractorFound -= value; }
		}

		public event		OnInteractorDel					OnInteractorLost
		{
			add		{ if (value.IsNotNull()) m_OnInteractorLost += value; }
			remove	{ if (value.IsNotNull()) m_OnInteractorLost -= value; }
		}
		
		//////////////////////////////////////////////////////////////////
		// IInteractor START
		public bool HasInteractableAvailable() => m_CurrentInteractable.IsNotNull();
		public Interactable GetCurrentInteractable() => m_CurrentInteractable;
		public abstract bool IsCurrentlyInteracting();
		public abstract bool CanInteractWith(Interactable interactable);
		public abstract void Interact(Interactable interactable);
		public abstract void StopInteraction();
		// IInteractor END
		//////////////////////////////////////////////////////////////////

		public bool CanInteractWithCurrent() => CanInteractWith(m_CurrentInteractable);


		//////////////////////////////////////////////////////////////////
		protected void OnInteractorFoundInternal(Interactable interactable)
		{
			m_CurrentInteractable = interactable;
			m_OnInteractorFound(interactable);
		}

		//////////////////////////////////////////////////////////////////
		protected void OnInteractorLostInternal(Interactable interactable)
		{
			m_CurrentInteractable = null;
			m_OnInteractorLost(interactable);
		}
	}
}
