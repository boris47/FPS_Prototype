using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFoots {

	void PlayStep();

}


public class Foots : MonoBehaviour, IFoots {

	public		LiveEntity		Parent
	{
		set;
		private get;
	}

	private	AudioSource			m_AudioSource		= null;
	private	Collider			m_CurrentCollider	= null;
	private	Vector3				m_CurrentCollPoint	= Vector3.zero;


	//////////////////////////////////////////////////////////////////////////
	// START
	private void Start()
	{	
		m_AudioSource = GetComponent<AudioSource>();
	}


	//////////////////////////////////////////////////////////////////////////
	// PlayStep
	public	void	PlayStep()
	{
		if ( m_CurrentCollider == null )
			return;

		AudioClip footstepClip = SurfaceManager.Instance.GetFootstep( m_CurrentCollider, m_CurrentCollPoint );
		if( footstepClip != null )
		{
			if ( m_AudioSource.isPlaying )
				m_AudioSource.Stop();
			m_AudioSource.clip = footstepClip;
			m_AudioSource.Play();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// UNITY
	private void OnTriggerEnter( Collider other )
	{
		if ( other.tag != "Terrain" )
			return;

		m_CurrentCollider = other;
		m_CurrentCollPoint = transform.position;
		if ( Parent != null )
		{
			Parent.Grounded = true;
		}
	}


	private void OnTriggerExit( Collider other )
	{
		if ( other.tag != "Terrain" )
			return;

		m_CurrentCollider = null;

		if ( Parent != null )
		{
			Parent.Grounded = false;
		}
	}


}
