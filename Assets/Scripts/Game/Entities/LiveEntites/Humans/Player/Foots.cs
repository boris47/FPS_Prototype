﻿using System.Collections;
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

	private		IEntity				m_Entity			= null;
	private		Collider			m_CurrentCollider	= null;
	private		RaycastHit			m_RaycastHit		= default( RaycastHit );
	private		ICustomAudioSource	m_AudioSource		= null;


	//////////////////////////////////////////////////////////////////////////
	// AWAKE
	private void Awake()
	{
		m_LiveEntity	= transform.parent.GetComponent<LiveEntity>();
		m_Entity		= m_LiveEntity as IEntity;
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

		Debug.DrawLine( transform.position, transform.position - transform.up * ( m_Entity.PhysicCollider.height * 0.3f ) );
		
		bool hasCollision = Physics.Raycast( transform.position, -transform.up, out m_RaycastHit, ( m_Entity.PhysicCollider.height * 0.3f ) );

		if ( hasCollision && m_RaycastHit.distance < m_Entity.PhysicCollider.height * 0.2f )
		{
			m_CurrentCollider = m_RaycastHit.collider;
		}
		else
		{
			m_CurrentCollider = null;
		}

		bool isGrounded = m_CurrentCollider != null;
		if ( m_LiveEntity.IsGrounded == false && isGrounded )
		{
//			CameraControl.Instance.ApplyFallFeedback( 5f, 1f, 0f );
			PlayStep();
		}

		m_LiveEntity.IsGrounded = isGrounded;
	}

}
