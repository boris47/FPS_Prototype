using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFoots {

	Collider		Collider { get; }

	void			PlayStep();

}

public class Foots : MonoBehaviour, IFoots {

	private		LiveEntity			m_LiveEntity		= null;

	private		Collider			m_Collider			= null;
	public		Collider			Collider
	{
		get { return m_Collider == null ? m_Collider = GetComponent<Collider>() : m_Collider; }
	}

	private		Collider			m_CurrentCollider	= null;
	private		ICustomAudioSource	m_AudioSource		= null;


	//////////////////////////////////////////////////////////////////////////
	// AWAKE
	private void Awake()
	{
		m_LiveEntity	= transform.parent.GetComponent<LiveEntity>();
		m_AudioSource	= transform.GetComponent<ICustomAudioSource>();
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayStep
	public	void	PlayStep()
	{
		if ( m_CurrentCollider == null )
			return;

		if ( SurfaceManager.Instance == null )
			return;

		AudioClip footstepClip = SurfaceManager.Instance.GetFootstep( m_CurrentCollider, transform.position );
		if ( footstepClip == null )
			return;

		m_AudioSource.Clip = footstepClip;
		m_AudioSource.Play();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( other.isTrigger )
			return;

		m_CurrentCollider = other;
		m_LiveEntity.IsGrounded = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void OnTriggerExit( Collider other )
	{
		if ( other.isTrigger )
			return;

		m_LiveEntity.IsGrounded = false;
	}
}
