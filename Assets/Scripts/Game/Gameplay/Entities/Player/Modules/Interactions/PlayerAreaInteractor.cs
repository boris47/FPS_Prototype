
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Player.Components
{
	public class PlayerAreaInteractor : PlayerInteractor, IAreaInteractor
	{
		// IInteractor START
		//////////////////////////////////////////////////////////////////
		public override bool IsCurrentlyInteracting()
		{
			return false; // TODO
		}
		//////////////////////////////////////////////////////////////////
		public override bool CanInteractWith(Interactable interactable)
		{
			return (interactable is AreaInteractable area) && area.CanInteract(m_Owner);
		}
		//////////////////////////////////////////////////////////////////
		public override void Interact(Interactable interactable)
		{
			interactable.OnInteraction(m_Owner);
		}
		//////////////////////////////////////////////////////////////////
		public override void StopInteraction()
		{

		}
		// IInteractor END


		//////////////////////////////////////////////////////////////////
		public void AddPotentialInteraction(AreaInteractable areaInteractable)
		{
			OnInteractorFoundInternal(areaInteractable);
		}

		//////////////////////////////////////////////////////////////////
		public void RemovePotentialInteraction(AreaInteractable areaInteractable)
		{
			if (CurrentInteractable == areaInteractable)
			{
				OnInteractorLostInternal(areaInteractable);
			}
		}
	}
}
