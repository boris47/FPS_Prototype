using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFoots {

	Collider		Collider { get; }

	void			PlayStep();

}

// TODO prevent actor not stepping on box borders

public class Foots : MonoBehaviour, IFoots {

	private		LiveEntity			m_LiveEntity		= null;
	public		LiveEntity			Onwer
	{
		get { return m_LiveEntity; }
	}

	public		Collider			Collider
	{
		get { return GetComponent<Collider>(); }
	}

	private		Collider			m_CurrentCollider	= null;
//	private		RaycastHit			m_RaycastHit		= default( RaycastHit );
	private		ICustomAudioSource	m_AudioSource		= null;
//	private		MeshCollider		m_Collider			= null;
//	private		bool				m_IsColliding		= false;


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

		AudioClip footstepClip = SurfaceManager.Instance.GetFootstep( ref m_CurrentCollider, transform.position );
		if ( footstepClip == null )
			return;

		m_AudioSource.Clip = footstepClip;
		m_AudioSource.Play();
	}

	private void OnTriggerEnter( Collider other )
	{
		if ( other.isTrigger == true )
			return;

		m_CurrentCollider = other;

//		print( other.name );
		m_LiveEntity.IsGrounded = true;
	}

	private void OnTriggerExit( Collider other )
	{
		if ( other.isTrigger == true )
			return;

		m_LiveEntity.IsGrounded = false;
	}
}
