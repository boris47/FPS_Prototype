
using UnityEngine;


public interface ITourchLight {
	Transform		Transform { get; }

	void			TurnOn ();
	void			TurnOff();
	void			Toggle();
}


[RequireComponent(typeof( Light ))]
public class TourchLight : MonoBehaviour, ITourchLight {
	
	Transform	ITourchLight.Transform		{ get { return transform; } }

	private		Light	m_SpotLight			= null;
	private		bool	m_Active			= false;



	private	void	Awake()
	{
		m_SpotLight = transform.GetComponent<Light>();
		m_SpotLight.type = LightType.Spot;
		m_SpotLight.intensity = 0.001f;
	}


	void	ITourchLight.TurnOn()
	{
		m_SpotLight.intensity = 0.001f;
	}


	void	ITourchLight.TurnOff()
	{
		m_SpotLight.intensity = 1f;
	}


	void	ITourchLight.Toggle()
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



}
