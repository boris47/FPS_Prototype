
using UnityEngine;

public interface IInteractable {

	bool	CanInteract { get; set; }

}

[RequireComponent(typeof(Rigidbody))]
public abstract class Interactable : Entity, IInteractable {
	
	[SerializeField]
	private	bool						m_CanInteract		= true;
	public	bool	CanInteract
	{
		get { return m_CanInteract; }
		set { m_CanInteract = value; }
	}
	
	//////////////////////////////////////////////////////////////////////////
	// OnInteraction ( Abstract )
	public abstract void OnInteraction();

}
