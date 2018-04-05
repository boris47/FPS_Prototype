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
	private		Collider			m_Collider			= null;

	private		RaycastHit			m_RaycastHit		= default( RaycastHit );


	//////////////////////////////////////////////////////////////////////////
	// AWAKE
	private void Awake()
	{
		m_LiveEntity	= transform.parent.GetComponent<LiveEntity>();
		m_Collider		= transform.GetComponent<Collider>();
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
	// Update
	private void Update()
	{
		Debug.DrawLine( transform.position, transform.position - transform.up * 0.2f );

		if ( Physics.Raycast( transform.position, -transform.up, out m_RaycastHit, 0.2f ) )
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
