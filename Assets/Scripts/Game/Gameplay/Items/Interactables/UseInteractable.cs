
using UnityEngine;
using Entities;

[RequireComponent(typeof(Collider))]
public class UseInteractable : Interactable
{
	/////////////////////////////////////////////////////////////////////////////
	public override bool CanInteract(Entity entity)
	{
		return true;
	}

	public override void OnInteraction(Entity entity)
	{
		m_OnInteraction.Invoke();
	}
}
