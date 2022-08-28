
using UnityEngine;

namespace Entities.Player.Components
{
	public class PlayerUseInteractor : PlayerInteractor
	{
		[SerializeField, ReadOnly]
		private LayerMask m_AimCollisionFilter = 0;

		private Transform m_RaySource = null;



		//////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			m_RaySource = m_Owner.Head;
			m_AimCollisionFilter = 1 << LayerMask.NameToLayer(Interactable.LayerName);
		}

		//////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();

			m_AimCollisionFilter = 1 << LayerMask.NameToLayer(Interactable.LayerName);
		}

		// IInteractor START
		//////////////////////////////////////////////////////////////////
		public override bool IsCurrentlyInteracting()
		{
			return false; // TODO
		}
		//////////////////////////////////////////////////////////////////
		public override bool CanInteractWith(Interactable interactable)
		{
			return (interactable is UseInteractable usable) && usable.CanInteract(m_Owner);
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
		private void FixedUpdate()
		{
			if(Physics.Raycast(m_RaySource.position, m_RaySource.forward, out RaycastHit hitInfo, Owner.Configs.UseDistance, m_AimCollisionFilter, QueryTriggerInteraction.Ignore))
			{
				if (CurrentInteractable == null || hitInfo.transform != CurrentInteractable.transform)
				{
					if (hitInfo.transform.gameObject.TryGetComponent(out Interactable newIteractable))
					{
						if (CanInteractWith(newIteractable))
						{
							OnInteractorFoundInternal(newIteractable);
						}
					}
					else
					{
						if (CurrentInteractable.IsNotNull())
						{
							OnInteractorLostInternal(CurrentInteractable);
						}
					}
				}
			}
			else
			{
				if (CurrentInteractable.IsNotNull())
				{
					OnInteractorLostInternal(CurrentInteractable);
				}
			}
		}
	}
}
