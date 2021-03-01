
using UnityEngine;
/*
public interface IGrabbable
{
	Transform		Transform		{ get; }
	Interactable	Interactable	{ get; }

	void			OnGrab			();
}
*/

public class Grabbable : Interactable//, IGrabbable
{
	// TRANSFORM
	public	Transform		Transform		=> transform;

//	// INTERACTABLE REF
	private	Interactable	m_Interactable	= null;
	public	Interactable	Interactable	=> m_Interactable;


	
	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		base.Awake();
		m_Interactable = this;
	}
	

	public	void			OnGrab			()
	{

	}
}
