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
	private		RaycastHit			m_RaycastHit		= default( RaycastHit );
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

		AudioClip footstepClip = SurfaceManager.Instance.GetFootstep( ref m_CurrentCollider, transform.position );
		if ( footstepClip == null )
			return;

		m_AudioSource.Clip = footstepClip;
		m_AudioSource.Play();
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private	void	Update()
	{
		// Check only if falling or moving
//		if ( m_LiveEntity.IsFalling == false || m_LiveEntity.IsMoving == false )
//			return;

		Debug.DrawLine( transform.position, transform.position - transform.up * 1.7f );

		if ( Physics.Raycast( transform.position, -transform.up, out m_RaycastHit, 1.7f ) )
		{
			m_CurrentCollider = m_RaycastHit.collider;
		}
		else
		{
			m_CurrentCollider = null;
		}

		bool isGrounded = m_CurrentCollider != null;
		if ( m_LiveEntity.IsGrounded == false && isGrounded )
			PlayStep();

		m_LiveEntity.IsGrounded = isGrounded;
	}

}
