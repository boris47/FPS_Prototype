
using UnityEngine;


public interface IFlashLight {
	Transform		Transform { get; }

	bool			Activated { get; }

	void			TurnOn ();
	void			TurnOff();
	void			Toggle();
	void			SetActive( bool state );
}


[System.Serializable]
public class Flashlight : WeaponAttachment, IFlashLight {
	
	// INTERFACE START
	Transform	IFlashLight.Transform		{ get { return transform; } }
	bool		IFlashLight.Activated		{ get { return m_Active; } }
	// INTERFACE END

	protected		Light	m_SpotLight			= null;
	protected		bool	m_Active			= false;
	protected		bool	m_CanBeUsed			= true;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	protected	void	Awake()
	{
		m_CanBeUsed = transform.SearchComponent( ref m_SpotLight, SearchContext.CHILDREN );
		if ( m_CanBeUsed )
		{
			m_SpotLight.type = LightType.Spot;
			m_SpotLight.intensity = 0.001f;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// TurnOn
	public	void	TurnOn()
	{
		if ( m_CanBeUsed == false )
			return;

		m_SpotLight.intensity = 1f;
	}


	//////////////////////////////////////////////////////////////////////////
	// TurnOff
	public	void	TurnOff()
	{
		if ( m_CanBeUsed == false )
			return;

		m_SpotLight.intensity = 0.001f;
}

	//////////////////////////////////////////////////////////////////////////
	// Toggle
	public	void	Toggle()
	{
		if ( m_CanBeUsed == false )
			return;

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
	public	void	SetActive( bool state )
	{
		if ( m_CanBeUsed == false )
			return;

		IFlashLight thisInterface = this as IFlashLight;
		if ( state == true )
			thisInterface.TurnOn();
		else
			thisInterface.TurnOff();

		m_Active = state;
	}


}
