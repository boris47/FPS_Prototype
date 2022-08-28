
using System.Linq;
using UnityEngine;

namespace Entities.Player.Components
{
	[RequireComponent(typeof(PlayerGrabInteractor))]
	[RequireComponent(typeof(PlayerUseInteractor))]
	[RequireComponent(typeof(PlayerAreaInteractor))]
	public class PlayerInteractionManager : PlayerEntityComponent
	{
		[SerializeField, ReadOnly]
		private					PlayerInteractor				m_CurrentValidInteractor			= null;

		[SerializeField, ReadOnly]
		private					PlayerInteractor[]				m_Interactors						= null;


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			UpdateInteractors();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();

			gameObject.GetOrAddIfNotFound<PlayerGrabInteractor>();
			gameObject.GetOrAddIfNotFound<PlayerUseInteractor>();
			gameObject.GetOrAddIfNotFound<PlayerAreaInteractor>();

			UpdateInteractors();
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();
		
			foreach (PlayerInteractor interactor in m_Interactors)
			{
				if (interactor.IsNotNull())
				{
					interactor.enabled = true;
				}
			}

			InputHandler.RegisterButtonCallbacks(this, Owner.Configs.UseAction, UseInteractable, null, null);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnDisable()
		{
			base.OnDisable();
		
			foreach (PlayerInteractor interactor in m_Interactors)
			{
				if (interactor.IsNotNull())
				{
					interactor.enabled = false;
				}
			}

			InputHandler.UnRegisterAllCallbacks(this);
		}

		//////////////////////////////////////////////////////////////////////////
		public void UpdateInteractors()
		{
			transform.TrySearchComponents(ESearchContext.LOCAL, out m_Interactors);

			System.Array.Sort(m_Interactors, new PlayerInteractor.InteractorPriorityComparer());
		}

		//////////////////////////////////////////////////////////////////////////
		private void FixedUpdate()
		{
			m_CurrentValidInteractor = m_Interactors.FirstOrDefault(interactor => interactor.IsNotNull() && interactor.HasInteractableAvailable() && interactor.CanInteractWithCurrent());
		}

		//////////////////////////////////////////////////////////////////////////
		private void UseInteractable()
		{
			if (m_CurrentValidInteractor.IsNotNull())
			{
				if (m_CurrentValidInteractor.IsCurrentlyInteracting())
				{
					m_CurrentValidInteractor.StopInteraction();
				}
				else
				{
					m_CurrentValidInteractor.Interact(m_CurrentValidInteractor.GetCurrentInteractable());
				}
			}
		}
	}
}
