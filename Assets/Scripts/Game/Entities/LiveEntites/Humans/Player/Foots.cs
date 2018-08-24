using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFoots {
	
	void			OnFrame();

	void			PlayStep();

	bool			IsTouching { get; }

}

// TODO prevent actor not stepping on box borders

public class Foots : MonoBehaviour, IFoots {

	bool		IFoots.IsTouching { get { return m_IsColliding; } }

	private		LiveEntity			m_LiveEntity		= null;
	public		LiveEntity			Onwer
	{
		get { return m_LiveEntity; }
	}

	private		IEntity				m_Entity			= null;
	private		Collider			m_CurrentCollider	= null;
	private		RaycastHit			m_RaycastHit		= default( RaycastHit );
	private		ICustomAudioSource	m_AudioSource		= null;
//	private		MeshCollider		m_Collider			= null;
	private		bool				m_IsColliding		= false;


	//////////////////////////////////////////////////////////////////////////
	// AWAKE
	private void Awake()
	{
		m_LiveEntity	= transform.parent.GetComponent<LiveEntity>();
		m_Entity		= m_LiveEntity as IEntity;
		m_AudioSource	= transform.GetComponent<ICustomAudioSource>();
//		m_Collider		= GetComponent<MeshCollider>();
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
	// OnFrame
	public	void	OnFrame()
	{
		return;
		const float offset = 0.05f;

		Vector3 startLine = transform.position;
		Vector3 endLine   = transform.position - transform.up * offset;
		Debug.DrawLine( startLine, endLine );

		bool isGrounded = Physics.Linecast( startLine, endLine, out m_RaycastHit );
		if ( isGrounded  )
		{
			m_CurrentCollider = m_RaycastHit.collider;
		}
		else
		{
			m_CurrentCollider = null;
		}

		if ( m_LiveEntity.IsGrounded == false && isGrounded )
		{
//			CameraControl.Instance.ApplyFallFeedback( 5f, 1f, 0f );
			PlayStep();
		}

		m_IsColliding = isGrounded;

//		m_LiveEntity.IsGrounded = isGrounded;
	}

	private void OnTriggerEnter( Collider other )
	{
		if ( other.isTrigger == true )
			return;

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
