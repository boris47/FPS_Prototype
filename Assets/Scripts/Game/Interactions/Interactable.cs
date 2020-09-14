
using UnityEngine;

public interface IInteractable {

	event Interactable.OnInteractionDel	OnInteractionCallback;

	event Interactable.OnInteractionDel	OnRetroInteractionCallback;

	bool		HasRetroInteraction {get; set;}

	bool		HasInteracted		{ get; }

	Rigidbody	RigidBody			{ get; }
	Collider	Collider			{ get; }

	bool		CanInteract			{ get; set; }

	void		OnInteractionToggle();
	void		OnInteraction();
	void		OnRetroInteraction();

}

[RequireComponent( typeof(Rigidbody), typeof(Collider) )]
public class Interactable : MonoBehaviour, IInteractable {

	public	delegate	void OnInteractionDel();

	[SerializeField]
	protected		bool			m_HasRetroInteraction				= false;

	[SerializeField]
	protected		GameEvent		m_OnInteraction						= new GameEvent();

	[SerializeField]
	protected		GameEvent		m_OnRetroInteraction				= new GameEvent();
	
	[SerializeField]
	protected		bool			m_CanInteract						= true;

	protected		bool			m_HasInteracted						= false;
	public			bool				HasInteracted
	{
		get { return this.m_HasInteracted; }
	}

	//-
	public			bool				HasRetroInteraction
	{
		get { return this.m_HasRetroInteraction; }
		set { this.m_HasRetroInteraction = value; }
	}

	//-
	protected	event OnInteractionDel	m_OnInteractionCallback			= delegate { };
	public		event OnInteractionDel  OnInteractionCallback
	{
		add		{	if ( value != null )	m_OnInteractionCallback += value; }
		remove	{	if ( value != null )	m_OnInteractionCallback -= value; }
	}

	//-
	protected	event OnInteractionDel	m_OnRetroInteractionCallback	= delegate { };
	public		event OnInteractionDel  OnRetroInteractionCallback
	{
		add		{	if ( value != null )	m_OnRetroInteractionCallback += value; }
		remove	{	if ( value != null )	m_OnRetroInteractionCallback -= value; }
	}


	//-
	public		bool		CanInteract
	{
		get { return this.m_CanInteract; }
		set { this.m_CanInteract = value; }
	}


	protected	Rigidbody	m_RigidBody		= null;
	public		Rigidbody	RigidBody		{ get { return this.m_RigidBody; } }

	protected	Collider	m_Collider		= null;
	public		Collider	Collider		{ get { return this.m_Collider; } }



	//////////////////////////////////////////////////////////////////////////
	// Awake ( Virtual )
	protected	virtual	 void	Awake()
	{
		this.TryGetComponent(out this.m_RigidBody);
		this.TryGetComponent(out this.m_Collider);
	}

	//////////////////////////////////////////////////////////////////////////
	// OnInteractionToggle
	public 	virtual		void		OnInteractionToggle()
	{
		// Only Available if has a retro interaction action
		if (this.m_HasRetroInteraction == false )
			return;

		if (this.m_HasInteracted == false )
		{
			m_OnInteractionCallback();
			this.m_OnInteraction.Invoke();
		}
		else
		{
			m_OnRetroInteractionCallback();
			this.m_OnRetroInteraction.Invoke();
		}

		this.m_HasInteracted = !this.m_HasInteracted;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnInteraction
	public	virtual		void		OnInteraction()
	{
		m_OnInteractionCallback();
		this.m_OnInteraction.Invoke();
		this.m_HasInteracted = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnRetroInteraction
	public	virtual		void		OnRetroInteraction()
	{
		m_OnRetroInteractionCallback();
		this.m_OnRetroInteraction.Invoke();
		this.m_HasInteracted = false;
	}

}