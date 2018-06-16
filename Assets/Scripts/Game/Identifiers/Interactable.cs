
using UnityEngine;

public interface IInteractable {

	Rigidbody	RigidBody			{ get; }
	Collider	Collider			{ get; }

	bool		CanInteract			{ get; set; }

	void		OnInteraction		();

}

[RequireComponent( typeof(Rigidbody), typeof(Collider) )]
public abstract class Interactable : MonoBehaviour, IInteractable {

	[SerializeField]
	protected	bool		m_CanInteract	= true;
	public		bool		CanInteract		{ get { return m_CanInteract; } set { m_CanInteract = value; } }


	protected	Rigidbody	m_RigidBody		= null;
	public		Rigidbody	RigidBody		{ get { return m_RigidBody; } }

	protected	Collider	m_Collider		= null;
	public		Collider	Collider		{ get { return m_Collider; } }



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Virtual )
	protected	virtual	 void	Awake()
	{
		m_RigidBody = GetComponent<Rigidbody>();
		m_Collider	= GetComponent<Collider>();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnInteraction ( Abstract )
	public abstract void		OnInteraction();

}