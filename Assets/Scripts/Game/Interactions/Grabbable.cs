
using UnityEngine;

public interface IGrabbable : IInteractable {

	Transform		Transform		{ get; }
	IInteractable	Interactable	{ get; }

	void			OnGrab			();
}


public class Grabbable : Interactable, IGrabbable {

	// TRANSFORM
	public	Transform		Transform		{ get { return this.transform; } }

//	// INTERACTABLE REF
	private	IInteractable	m_Interactable	= null;
	public	IInteractable	Interactable	{ get { return this.m_Interactable; } }


	
	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();
		this.m_Interactable = ( this as IInteractable );
	}
	

	public	void			OnGrab			()
	{

	}
}
