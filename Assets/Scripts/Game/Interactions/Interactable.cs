﻿
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
		get { return m_HasInteracted; }
	}

	//-
	public			bool				HasRetroInteraction
	{
		get { return m_HasRetroInteraction; }
		set { HasRetroInteraction = value; }
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
		get { return m_CanInteract; }
		set { m_CanInteract = value; }
	}


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
	// OnInteractionToggle
	public 	virtual		void		OnInteractionToggle()
	{
		// Only Available if has a retro interaction action
		if ( m_HasRetroInteraction == false )
			return;

		if ( m_HasInteracted == false )
		{
			m_OnInteractionCallback();
			m_OnInteraction.Invoke();
		}
		else
		{
			m_OnRetroInteractionCallback();
			m_OnRetroInteraction.Invoke();
		}

		m_HasInteracted = !m_HasInteracted;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnInteraction
	public	virtual		void		OnInteraction()
	{
		m_OnInteractionCallback();
		m_OnInteraction.Invoke();
		m_HasInteracted = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnRetroInteraction
	public	virtual		void		OnRetroInteraction()
	{
		m_OnRetroInteractionCallback();
		m_OnRetroInteraction.Invoke();
		m_HasInteracted = false;
	}

}