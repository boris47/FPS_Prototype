
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entities.Player.Components
{
	[RequireComponent(typeof(PlayerGrabInteractor))]
	[RequireComponent(typeof(PlayerUseInteractor))]
	[RequireComponent(typeof(PlayerAreaInteractor))]
	public class PlayerInteractionManager : PlayerEntityComponent
	{
		private class PairedInteractable
		{
			public readonly Interactable Interactable = null;
			public readonly PlayerInteractor Interactor = null;

			public PairedInteractable(Interactable InInteractable, PlayerInteractor InInteractor)
			{
				Interactable = InInteractable;
				Interactor = InInteractor;
			}
		}

		[SerializeField, ReadOnly]
		private					PlayerInteractor				m_CurrentInteractor			= null;

		[SerializeField, ReadOnly]
		private					PlayerInteractor[]				m_Interactors						= null;



		//////////////////////////////////////////////////////////////////////////
		protected override void Awake()
		{
			base.Awake();

			transform.TrySearchComponents(ESearchContext.LOCAL, out m_Interactors);

			System.Array.Sort(m_Interactors, PlayerInteractor.Comparer);
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnValidate()
		{
			base.OnValidate();

			gameObject.GetOrAddIfNotFound<PlayerGrabInteractor>();
			gameObject.GetOrAddIfNotFound<PlayerUseInteractor>();
			gameObject.GetOrAddIfNotFound<PlayerAreaInteractor>();

			System.Array.Sort(m_Interactors, PlayerInteractor.Comparer);
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
					interactor.OnInteractableFound += OnInteractableFound;
					interactor.OnInteractableLost += OnInteractableLost;
				}
			}

			InputHandler.RegisterButtonCallbacks(this, Owner.Configs.UseAction, OnInteractionStart, OnInteractionContinue, OnInteractionEnd);
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
					
					if (interactor.HasInteractableAvailable())
					{
						interactor.InteractionEnd();
					}

					interactor.OnInteractableFound -= OnInteractableFound;
					interactor.OnInteractableLost -= OnInteractableLost;
				}
			}

			InputHandler.UnRegisterAllCallbacks(this);
		}

		private List<PairedInteractable> m_PairedAvailableInteractables = new List<PairedInteractable>();

		
		//////////////////////////////////////////////////////////////////////////
		private void OnInteractableFound(PlayerInteractor interactor, Interactable interactable)
		{
			if (m_PairedAvailableInteractables.TryFind(out PairedInteractable pair, out int index, pair => pair.Interactable == interactable))
			{
				Debug.LogError($"OnInteractableFound: The interactable {pair.Interactable.name} alread exist found by interactor {pair.Interactor.name} but has been found again by {interactor.name}");
			}
			else
			{
				m_PairedAvailableInteractables.Add(new PairedInteractable(interactable, interactor));
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnInteractableLost(PlayerInteractor interactor, Interactable interactable)
		{
			if (m_PairedAvailableInteractables.TryFind(out PairedInteractable pair, out int index, pair => pair.Interactable == interactable))
			{
				m_PairedAvailableInteractables.RemoveAt(index);
			}
		}

		private PairedInteractable m_CurrentInteractionPair = null;

		//////////////////////////////////////////////////////////////////////////
		private void OnInteractionStart()
		{
			m_CurrentInteractionPair = m_PairedAvailableInteractables.LastOrDefault();
			if (m_CurrentInteractionPair.IsNotNull())
			{
				m_CurrentInteractor = m_CurrentInteractionPair.Interactor;
				m_CurrentInteractionPair.Interactor.InteractionStart();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnInteractionContinue(float deltaTime)
		{
			if (m_CurrentInteractionPair.IsNotNull() && m_CurrentInteractionPair.Interactor.HasInteractableAvailable())
			{
				m_CurrentInteractionPair.Interactor.InteractionUpdate(deltaTime);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnInteractionEnd()
		{
			if (m_CurrentInteractionPair.IsNotNull() && m_CurrentInteractionPair.Interactor.HasInteractableAvailable())
			{
				m_CurrentInteractionPair.Interactor.InteractionEnd();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void FixedUpdate()
		{
		//	m_CurrentInteractable = m_PairedAvailableInteractables.LastOrDefault()?.Interactable ?? null;

			/*
			static bool EvaluateInteractor(PlayerInteractor interactor) => interactor.IsNotNull() && interactor.HasInteractableAvailable() && interactor.CanInteractWithCurrent();

			// Current interactor has changed
			PlayerInteractor currentChoosenInteractor = m_Interactors.FirstOrDefault(EvaluateInteractor);
			currentChoosenInteractor = currentChoosenInteractor.IsNotNull() ? currentChoosenInteractor : m_Interactors[0];
			if (m_CurrentInteractor != currentChoosenInteractor)
			{
				// Stop interaction with previous interactor
				if (m_CurrentInteractor.HasInteractableAvailable())
				{
					m_CurrentInteractor.StopInteraction();
				}

				// Assign new valid interactor
				m_CurrentInteractor = currentChoosenInteractor;
			}
			*/
		}
	}
}
