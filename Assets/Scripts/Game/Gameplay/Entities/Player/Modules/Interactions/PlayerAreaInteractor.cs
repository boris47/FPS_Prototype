
using System.Collections.Generic;
using UnityEngine;

namespace Entities.Player.Components
{
	public class PlayerAreaInteractor : PlayerInteractor
	{
		public override			uint							Priority							=> 2u;

		// IInteractor START
		//////////////////////////////////////////////////////////////////
		public override bool IsCurrentlyInteracting()
		{
			return false; // TODO
		}

		//////////////////////////////////////////////////////////////////
		public override bool CanInteractWith(Interactable interactable)
		{
			bool bOutResult = false;
			if (interactable is AreaInteractable areaInteractable && areaInteractable.CanInteract(m_Owner))
			{
				if (areaInteractable.ValidLocationRef.IsNotNull())
				{
					Transform validLocationRef = areaInteractable.ValidLocationRef;
					Vector3 direction = validLocationRef.position - m_Owner.Head.position;
					float distance = direction.magnitude + 0.01f;

					// Close enough, geometry hitted
					bool bHitted = Physics.Raycast(m_Owner.Head.position, direction, out RaycastHit hitInfo, distance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
					if (bHitted && hitInfo.transform == validLocationRef)
					{
						bOutResult = true;
					}
					else
					// no geometry hitted but close enough
					if (!bHitted && (distance < Owner.Configs.UseDistance))
					{
						bOutResult = true;
					}
				}
				else
				{
					bOutResult = true; // true becuase we are inside the area and there is not transform as reference for sight check, so we can interact with area
				}
			}
			return bOutResult;
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
		private void OnTriggerEnter(Collider other)
		{
			if (other.transform.TryGetComponent(out AreaInteractable areaInteractable))
			{
				OnInteractorFoundInternal(areaInteractable);
			}
		}

		//////////////////////////////////////////////////////////////////
		private void OnTriggerExit(Collider other)
		{
			if (other.transform.TryGetComponent(out AreaInteractable areaInteractable))
			{
				if (CurrentInteractable == areaInteractable)
				{
					OnInteractorLostInternal(areaInteractable);
				}
			}
		}
	}
}
