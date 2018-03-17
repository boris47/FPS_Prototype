using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFoots {

	void			PlayStep();

}


public class Foots : MonoBehaviour, IFoots {

	private		LiveEntity			m_LiveEntity		= null;
	private		Collider			m_CurrentCollider	= null;

	private		Vector3				m_PrevPosition		= Vector3.zero;


	//////////////////////////////////////////////////////////////////////////
	// AWAKE
	private void Awake()
	{
		m_LiveEntity	= transform.parent.GetComponent<LiveEntity>();
		m_PrevPosition	= transform.position;
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
	// UNITY

	private void OnTriggerEnter( Collider other )
	{
		m_CurrentCollider = other;
		m_LiveEntity.IsGrounded = true;

		m_LiveEntity.EvaluateFall( m_PrevPosition - transform.position );
		m_PrevPosition = transform.position;
		PlayStep();
	}

	private void OnTriggerStay( Collider other )
	{
		m_CurrentCollider = other;
		m_LiveEntity.IsGrounded = true;
	}


	private void OnTriggerExit( Collider other )
	{
		m_CurrentCollider = null;
		m_LiveEntity.IsGrounded = false;
	}


}
