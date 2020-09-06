
using UnityEngine;


public class ControlledDoor : ControlledObject {

	[SerializeField]
	private		bool				m_IsOpen					= false;

	private		Animator			m_Animator					= null;

	private		ICustomAudioSource	m_OpeningSource				= null;
	private		ICustomAudioSource	m_ClosingSource				= null;


	private void Awake()
	{
		this.m_Animator = this.GetComponentInChildren<Animator>();

		ICustomAudioSource[] sources = this.GetComponents<ICustomAudioSource>();
		this.m_OpeningSource = sources[0];
		this.m_ClosingSource = ( sources.Length > 1 ) ? sources[1] : sources[0];
	}


	public override void OnActivation()
	{
		this.m_IsOpen = !this.m_IsOpen;
		this.m_Animator.SetBool( "IsOpen", this.m_IsOpen );

		if (this.m_IsOpen == true )
			this.m_ClosingSource.Play();
		else
			this.m_OpeningSource.Play();

	}
	
}
