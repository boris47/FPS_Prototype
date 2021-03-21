using UnityEngine;


public class Foots : MonoBehaviour
{
	public	delegate	void		OnGroundedChangeEvent      (bool newState);

	[SerializeField, ReadOnly]
	private				Entity							m_Entity					= null;
	[SerializeField, ReadOnly]
	private				Collider						m_Collider					= null;
	[SerializeField, ReadOnly]
	private				Collider						m_CurrentCollider			= null;
	//[SerializeField, ReadOnly]
	//private				bool							m_WasGrounded				= false;

	private				ICustomAudioSource				m_AudioSource				= null;
	private	event		OnGroundedChangeEvent			m_OnGroundedChange			= delegate { };

	public				Collider						Collider					=> m_Collider;
	public		event	OnGroundedChangeEvent			OnEvent_GroundedChanged
	{
		add		{ if (value.IsNotNull()) m_OnGroundedChange += value; }
		remove	{ if (value.IsNotNull()) m_OnGroundedChange -= value; }
	}

	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		CustomAssertions.IsNotNull(transform.parent);

		CustomAssertions.IsTrue(transform.parent.TryGetComponent(out m_Entity));
		CustomAssertions.IsTrue(transform.TryGetComponent(out m_AudioSource));
		CustomAssertions.IsTrue(transform.TryGetComponent(out m_Collider));
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	PlayStep()
	{
		if (m_Entity && m_CurrentCollider)
		{
			Vector3 direction = (m_CurrentCollider.transform.position - m_Entity.transform.position).normalized;
			Vector3 origin = transform.position + (-direction * 0.1f);
			if (SurfaceManager.Instance.TryGetFootstep(out AudioClip footstepClip, m_CurrentCollider, new Ray(origin, direction)))
			{
				m_AudioSource.Clip = footstepClip;
				m_AudioSource.Play();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void UpdateState()
	{
		bool bIsGrounded = m_CurrentCollider.IsNotNull();
	//	if (m_WasGrounded != bIsGrounded)
		{
	//		m_WasGrounded = bIsGrounded;
			m_OnGroundedChange(bIsGrounded);
		}
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter( Collider other )
	{
		if (!other.isTrigger && !m_CurrentCollider)
		{
			m_CurrentCollider = other;
			UpdateState();
		}
	}
	*/
	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerStay(Collider other)
	{
		if (!other.isTrigger)
		{
			m_CurrentCollider = other;
			UpdateState();
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerExit(Collider other)
	{
		if (!other.isTrigger)
		{
			m_CurrentCollider = null;
			UpdateState();
		}
	}
	
}
