using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFoots {

	void			PlayStep();

}


public class Foots : MonoBehaviour, IFoots {

	private		LiveEntity			m_Player			= null;
	private		Collider			m_CurrentCollider	= null;



	//////////////////////////////////////////////////////////////////////////
	// AWAKE
	private void Awake()
	{
		m_Player = transform.parent.GetComponent<LiveEntity>();
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayStep
	public	void	PlayStep()
	{
		if ( m_CurrentCollider == null )
			return;

		AudioClip footstepClip = SurfaceManager.Instance.GetFootstep( m_CurrentCollider, transform.position );
		if ( footstepClip == null )
			return;

		AudioSource.PlayClipAtPoint( footstepClip, transform.position );
	}


	//////////////////////////////////////////////////////////////////////////
	// UNITY

	private void OnTriggerEnter( Collider other )
	{
		m_CurrentCollider = other;
		m_Player.Grounded = true;
		PlayStep();
	}

	private void OnTriggerStay( Collider other )
	{
		m_CurrentCollider = other;
		m_Player.Grounded = true;
	}


	private void OnTriggerExit( Collider other )
	{
		m_CurrentCollider = null;
		m_Player.Grounded = false;
	}


}
