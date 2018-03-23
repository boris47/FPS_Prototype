
using UnityEngine;

public class DashTarget : MonoBehaviour {


	private		LayerMask	m_LayerIgnoreRaycast;
	private		LayerMask	m_Layer_Default;
	private		Renderer	m_Renderer				= null;


	private void Awake()
	{
		m_LayerIgnoreRaycast	= LayerMask.NameToLayer( "Ignore Raycast" );;
		m_Layer_Default			= LayerMask.NameToLayer( "Default" );

		m_Renderer				= GetComponent<Renderer>();
	}


	public	void	OnTargetReached()
	{
		gameObject.layer = m_LayerIgnoreRaycast;
	}

	public	void	OnReset()
	{
		m_Renderer.enabled = true;
		gameObject.layer = m_Layer_Default;
	}

	public	void	Enable()
	{
		m_Renderer.enabled = true;
		gameObject.layer = m_Layer_Default;
	}

	public	void	Disable()
	{
		gameObject.layer = m_LayerIgnoreRaycast;
		m_Renderer.enabled = false;
	}
	
	private void OnTriggerEnter( Collider other )
	{
		if ( other.GetComponent<Player>() )
		{
			Disable();
		}
	}

	private void OnTriggerExit( Collider other )
	{
		if ( other.GetComponent<Player>() )
			Enable();
	}
	
}