
using UnityEngine;
using Entities;

[RequireComponent(typeof(Collider))]
public class UseInteractable : Interactable
{
	/////////////////////////////////////////////////////////////////////////////
	protected override void OnInteractionStartInternal(Entity entity)
	{
		m_OnInteractionStart.Invoke();
	}

	/////////////////////////////////////////////////////////////////////////////
	protected override void OnInteractionRepeatedInternal(Entity interactor)
	{
		m_OnInteractionRepeat.Invoke();
	}

	/////////////////////////////////////////////////////////////////////////////
	protected override void OnInteractionEndInternal(Entity entity)
	{
		m_InteractionEnd.Invoke();
	}
}
