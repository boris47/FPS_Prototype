
using UnityEngine;


public class ControlledDoor : ControlledObject
{
	[SerializeField, ReadOnly]
	private		bool				m_IsOpen					= false;

	[SerializeField]
	private		Animator			m_Animator					= null;

	[SerializeField]
	private		CustomAudioSource	m_OpeningSource				= null;

	[SerializeField]
	private		CustomAudioSource	m_ClosingSource				= null;


	private void Awake()
	{
		UnityEngine.Assertions.Assert.IsNotNull(m_Animator);
		UnityEngine.Assertions.Assert.IsNotNull(m_OpeningSource);
		UnityEngine.Assertions.Assert.IsNotNull(m_ClosingSource);

		enabled = m_Animator.IsNotNull() && m_OpeningSource.IsNotNull() && m_ClosingSource.IsNotNull();
	}


	public override void OnActivation()
	{
		if (enabled)
		{
			if (m_IsOpen)
			{
				m_ClosingSource.Play();
			}
			else
			{
				m_OpeningSource.Play();
			}

			m_IsOpen = !m_IsOpen;
			m_Animator.SetBool("IsOpen", m_IsOpen);
		}
	}
}
