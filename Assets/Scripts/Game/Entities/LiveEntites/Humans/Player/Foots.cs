using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFoots
{
	Collider		Collider { get; }
	void			PlayStep();
}

public class Foots : MonoBehaviour, IFoots
{
	private		LiveEntity			m_LiveEntity		= null;

	private		Collider			m_Collider			= null;
	public		Collider			Collider
	{
		get { return m_Collider ?? (m_Collider = GetComponent<Collider>()); }
	}

	private		Collider			m_CurrentCollider	= null;
	private		ICustomAudioSource	m_AudioSource		= null;


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		UnityEngine.Assertions.Assert.IsNotNull
		(
			transform.parent,
			$"Foots without parent"
		);
		if (transform.parent)
		{
			transform.parent.TryGetComponent(out m_LiveEntity);
			transform.TryGetComponent(out m_AudioSource);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	PlayStep()
	{
		if (m_LiveEntity && m_CurrentCollider)
		{
			Vector3 direction = m_LiveEntity.transform.rotation.GetVector( Vector3.down );
			if (SurfaceManager.Instance.TryGetFootstep(out AudioClip footstepClip, m_CurrentCollider, new Ray(transform.position, direction)))
			{
				m_AudioSource.Clip = footstepClip;
				m_AudioSource.Play();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter( Collider other )
	{
		if ( other.isTrigger )
			return;

		m_CurrentCollider = other;
		m_LiveEntity.IsGrounded = true;
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit( Collider other )
	{
		if ( other.isTrigger )
			return;

		m_LiveEntity.IsGrounded = false;
	}
}
