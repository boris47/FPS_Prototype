
using System.Linq;
using UnityEngine;

namespace Entities.Player.Components
{
	[RequireComponent(typeof(PlayerUseInteractor))]
	[RequireComponent(typeof(PlayerAreaInteractor))]
	[RequireComponent(typeof(PlayerGrabInteractor))]
	public class PlayerEntityInteractionManager : PlayerEntityComponent
	{
		[SerializeField, ReadOnly]
		private					PlayerUseInteractor				m_RayInteractor						= null;

		[SerializeField, ReadOnly]
		private					PlayerAreaInteractor			m_AreaInteractor					= null;

		[SerializeField, ReadOnly]
		private					PlayerGrabInteractor			m_GrabInteractor					= null;

		[SerializeField, ReadOnly]
		private					PlayerInteractor				m_CurrentValidInteractor			= null;

		private					PlayerInteractor[]				m_Interactors						= new PlayerInteractor[3];


		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();
		
			if (Utils.CustomAssertions.IsTrue(gameObject.TryGetIfNotAssigned(ref m_RayInteractor)))
			{
				m_Interactors[0] = m_AreaInteractor;
			}

			if (Utils.CustomAssertions.IsTrue(gameObject.TryGetIfNotAssigned(ref m_AreaInteractor)))
			{
				m_Interactors[1] = m_RayInteractor;
			}

			if (Utils.CustomAssertions.IsTrue(gameObject.TryGetIfNotAssigned(ref m_GrabInteractor)))
			{
				m_Interactors[2] = m_GrabInteractor;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();

			if (gameObject.TryGetComponent(out m_RayInteractor))
			{
				m_Interactors[0] = m_AreaInteractor;
			}

			if (gameObject.TryGetComponent(out m_AreaInteractor))
			{
				m_Interactors[1] = m_RayInteractor;
			}

			if (gameObject.TryGetComponent(out m_GrabInteractor))
			{
				m_Interactors[2] = m_GrabInteractor;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnEnable()
		{
			base.OnEnable();
		
			foreach (PlayerInteractor interactor in m_Interactors)
			{
				if (interactor.IsNotNull())
				{
					interactor.OnInteractorFound += EvaluateInteractables;
					interactor.OnInteractorLost += EvaluateInteractables;
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
					interactor.OnInteractorFound -= EvaluateInteractables;
					interactor.OnInteractorLost -= EvaluateInteractables;
				}
			}

			InputHandler.UnRegisterAllCallbacks(this);
		}

		//////////////////////////////////////////////////////////////////////////
		private void EvaluateInteractables(Interactable _)
		{
			m_CurrentValidInteractor = m_Interactors.LastOrDefault(interactor => interactor.IsNotNull() && interactor.HasInteractableAvailable() && interactor.CanInteractWithCurrent());
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
