
using UnityEngine;

public interface IInteractable {

	Rigidbody	RigidBody			{ get; }
	Collider	Collider			{ get; }

	bool		CanInteract			{ get; set; }

	void		OnInteraction();

}

[RequireComponent( typeof(Rigidbody), typeof(Collider) )]
public class Interactable : MonoBehaviour, IInteractable {

	public	delegate	void OnInteractionDel();


	[SerializeField]
	protected	GameEvent	m_OnInteraction	= null;

	[SerializeField]
	protected	bool		m_CanInteract	= true;


	protected	event OnInteractionDel	m_OnInteractionCallback = delegate { };
	public	event OnInteractionDel OnInteractionCallback
	{
		add		{	if ( value != null )	m_OnInteractionCallback += value; }
		remove	{	if ( value != null )	m_OnInteractionCallback -= value; }
	}






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
	// TriggerOnInteraction
	public	void TriggerOnInteraction()
	{
		this.OnInteraction();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnInteraction
	public	virtual	void		OnInteraction()
	{
		if ( m_OnInteraction != null )
			m_OnInteraction.Invoke();

		m_OnInteractionCallback();
	}

}