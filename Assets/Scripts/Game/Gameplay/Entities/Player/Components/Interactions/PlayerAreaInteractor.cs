
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entities.Player.Components
{
	public class PlayerAreaInteractor : PlayerInteractor
	{
		public override			uint							Priority							=> 2u;

		// IInteractor START
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
		protected override void InteractionStartInternal()
		{
			CurrentInteractable.OnInteractionStart(m_Owner);
		}
		//////////////////////////////////////////////////////////////////
		protected override void InteractionStopInternal()
		{
			CurrentInteractable.OnInteractionEnd(m_Owner);
		}
		// IInteractor END

		private List<AreaInteractable> m_AvailableInteractors = new List<AreaInteractable>();


		//////////////////////////////////////////////////////////////////
		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.TryGetComponent(out AreaInteractable areaInteractable))
			{
				m_AvailableInteractors.Add(areaInteractable);

				GameManager.CyclesEvents.OnPhysicFrame -= OnPhysicFrame;
				GameManager.CyclesEvents.OnPhysicFrame += OnPhysicFrame;
			//	OnInteractableFoundInternal(areaInteractable);
			}
		}

		//////////////////////////////////////////////////////////////////
		private void OnPhysicFrame(float FixedDeltaTime)
		{
			if (CurrentInteractable.IsNull())
			{
				AreaInteractable candidate = null;
				foreach (AreaInteractable areaInteractable in m_AvailableInteractors)
				{
					if (CanInteractWith(areaInteractable))
					{
						candidate = areaInteractable;
						break;
					}
				}

				if (candidate.IsNotNull())
				{
					OnInteractableFoundInternal(candidate);
				}
			}
		}

		//////////////////////////////////////////////////////////////////
		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.TryGetComponent(out AreaInteractable areaInteractable))
			{
				m_AvailableInteractors.Remove(areaInteractable);

				if (m_AvailableInteractors.Count == 0)
				{
					GameManager.CyclesEvents.OnPhysicFrame -= OnPhysicFrame;
				}

				if (CurrentInteractable == areaInteractable)
				{
					OnInteractableLostInternal(areaInteractable);
				}
			}
		}
	}
}
