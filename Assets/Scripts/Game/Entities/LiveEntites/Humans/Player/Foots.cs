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
	private		Entity				m_Entity			= null;

	private		Collider			m_Collider			= null;
	public		Collider			Collider			=> m_Collider;

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
			transform.parent.TryGetComponent(out m_Entity);
			transform.TryGetComponent(out m_AudioSource);
		}

		m_Collider = GetComponent<Collider>();
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	PlayStep()
	{
		if (m_Entity && m_CurrentCollider)
		{
			Vector3 direction = -m_Entity.transform.up;
			Vector3 origin = transform.position + (-direction * 0.1f);
			if (SurfaceManager.Instance.TryGetFootstep(out AudioClip footstepClip, m_CurrentCollider, new Ray(origin, direction)))
			{
				m_AudioSource.Clip = footstepClip;
				m_AudioSource.Play();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		m_Entity.IsGrounded = m_CurrentCollider.IsNotNull();
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter( Collider other )
	{
		if (!other.isTrigger && !m_CurrentCollider)
		{
			m_CurrentCollider = other;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerStay(Collider other)
	{
		if (!other.isTrigger)
		{
			m_CurrentCollider = other;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit( Collider other )
	{
		if (!other.isTrigger)
		{
			m_CurrentCollider = null;
		}
	}
}
