
using UnityEngine;

public interface IGrabbable : IInteractable {

	Transform		Transform		{ get; }
	IInteractable	Interactable	{ get; }

	void			OnGrab			();
}


public class Grabbable : Interactable, IGrabbable {

	// TRANSFORM
	public	Transform		Transform		{ get { return transform; } }

//	// INTERACTABLE REF
	private	IInteractable	m_Interactable	= null;
	public	IInteractable	Interactable	{ get { return m_Interactable; } }


	
	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();
		m_Interactable = ( this as IInteractable );
	}
	

	public	void			OnGrab			()
	{

	}
}
