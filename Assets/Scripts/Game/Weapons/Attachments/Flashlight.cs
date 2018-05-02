
using UnityEngine;


public interface IFlashLight {
	Transform		Transform { get; }

	bool			Activated { get; }

	void			TurnOn ();
	void			TurnOff();
	void			Toggle();
	void			SetActive( bool state );
}


public class Flashlight : MonoBehaviour, IFlashLight {
	
	Transform	IFlashLight.Transform		{ get { return transform; } }
	bool		IFlashLight.Activated		{ get { return m_Active; } }

	private		Light	m_SpotLight			= null;
	private		bool	m_Active			= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private	void	Awake()
	{
		m_SpotLight = transform.GetComponentInChildren<Light>();
		m_SpotLight.type = LightType.Spot;
		m_SpotLight.intensity = 0.001f;
	}


	//////////////////////////////////////////////////////////////////////////
	// TurnOn
	void	IFlashLight.TurnOn()
	{
		m_SpotLight.intensity = 1f;
	}


	//////////////////////////////////////////////////////////////////////////
	// TurnOff
	void	IFlashLight.TurnOff()
	{
		m_SpotLight.intensity = 0.001f;
	}


	//////////////////////////////////////////////////////////////////////////
	// Toggle
	void	IFlashLight.Toggle()
	{
		m_Active = !m_Active;
		if ( m_Active == true )
		{
			m_SpotLight.intensity = 1f;
		}
		else
		{
			m_SpotLight.intensity = 0.001f;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// SetActive
	void	IFlashLight.SetActive( bool state )
	{
		IFlashLight thisInterface = this as IFlashLight;
		if ( state == true )
			thisInterface.TurnOn();
		else
			thisInterface.TurnOff();

		m_Active = state;
	}


}
