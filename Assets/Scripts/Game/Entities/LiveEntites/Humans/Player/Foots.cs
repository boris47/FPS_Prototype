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
		get { return this.m_Collider ?? (this.m_Collider = this.GetComponent<Collider>()); }
	}

	private		Collider			m_CurrentCollider	= null;
	private		ICustomAudioSource	m_AudioSource		= null;


	//////////////////////////////////////////////////////////////////////////
	// AWAKE
	private void Awake()
	{
		this.transform.parent.TryGetComponent(out this.m_LiveEntity);
		this.transform.TryGetComponent(out this.m_AudioSource);
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayStep
	public	void	PlayStep()
	{
		if (this.m_CurrentCollider == null )
			return;

		if ( SurfaceManager.Instance == null )
			return;

		AudioClip footstepClip = SurfaceManager.Instance.GetFootstep(this.m_CurrentCollider, this.transform.position );
		if ( footstepClip == null )
			return;

		this.m_AudioSource.Clip = footstepClip;
		this.m_AudioSource.Play();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter
	private void OnTriggerEnter( Collider other )
	{
		if ( other.isTrigger )
			return;

		this.m_CurrentCollider = other;
		this.m_LiveEntity.IsGrounded = true;
	}

	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit
	private void OnTriggerExit( Collider other )
	{
		if ( other.isTrigger )
			return;

		this.m_LiveEntity.IsGrounded = false;
	}
}
