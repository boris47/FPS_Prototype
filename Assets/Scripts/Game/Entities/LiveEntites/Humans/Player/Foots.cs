using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFoots {

	void			PlayStep();

}


public class Foots : MonoBehaviour, IFoots {

	private		LiveEntity			m_LiveEntity		= null;
	public		LiveEntity			Onwer
	{
		get { return m_LiveEntity; }
	}
	private		Collider			m_CurrentCollider	= null;


	//////////////////////////////////////////////////////////////////////////
	// AWAKE
	private void Awake()
	{
		m_LiveEntity	= transform.parent.GetComponent<LiveEntity>();
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayStep
	public	void	PlayStep()
	{
		if ( m_CurrentCollider == null )
			return;

		AudioClip footstepClip = SurfaceManager.Instance.GetFootstep( ref m_CurrentCollider, transform.position );
		if ( footstepClip == null )
			return;

		AudioSource.PlayClipAtPoint( footstepClip, transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// FixedUpdate
	private void FixedUpdate()
	{
		m_LiveEntity.IsGrounded = false;
		m_CurrentCollider = null;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerStay
	private void OnTriggerStay( Collider other )
	{
		m_CurrentCollider = other;
		m_LiveEntity.IsGrounded = true;
	}

}
