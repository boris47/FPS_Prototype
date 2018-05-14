
using UnityEngine;


public class ControlledDoor : ControlledObject {

	[SerializeField]
	private		bool				m_IsOpen					= false;

	private		Animator			m_Animator					= null;

	private		ICustomAudioSource	m_OpeningSource				= null;
	private		ICustomAudioSource	m_ClosingSource				= null;


	private void Awake()
	{
		m_Animator = GetComponentInChildren<Animator>();

		ICustomAudioSource[] sources = GetComponents<ICustomAudioSource>();
		m_OpeningSource = sources[0];
		m_ClosingSource = ( sources.Length > 1 ) ? sources[1] : sources[0];
	}


	public override void OnActivation()
	{
		m_IsOpen = !m_IsOpen;
		m_Animator.SetBool( "IsOpen", m_IsOpen );

		if ( m_IsOpen == true )
			m_ClosingSource.Play();
		else
			m_OpeningSource.Play();

	}
	
}
