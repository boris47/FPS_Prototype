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

	private				ICustomAudioSource				m_AudioSource				= null;
	private				bool							m_WasGrounded				= false;
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
		UnityEngine.Assertions.Assert.IsNotNull(transform.parent);

		transform.parent.TryGetComponent(out m_Entity);
		transform.TryGetComponent(out m_AudioSource);

		m_Collider = GetComponent<Collider>();
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
	private void OnEnable()
	{
		UnityEngine.Assertions.Assert.IsNotNull(GameManager.UpdateEvents);

		GameManager.UpdateEvents.OnFrame += OnFrame;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= OnFrame;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnFrame(float deltaTime)
	{
		bool bIsGrounded = m_CurrentCollider.IsNotNull();
		if (m_WasGrounded != bIsGrounded)
		{
			m_WasGrounded = bIsGrounded;
			m_OnGroundedChange(bIsGrounded);
		}
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
