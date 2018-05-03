
using UnityEngine;

public class Razor : Weapon
{
	private		Color							m_StartEmissiveColor				= Color.clear;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override void	Awake()
	{
		base.Awake();

		m_StartEmissiveColor = m_Renderer.material.GetColor( "_EmissionColor" );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEndReload ( Override )
	protected override void OnEndReload()
	{
		base.OnEndReload();
		m_Renderer.material.SetColor( "_EmissionColor", m_StartEmissiveColor );
	}


	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	protected override		void	ConfigureShot()
	{
		base.ConfigureShot();

		float interpolant = 1f - ( (float)m_Magazine / (float)m_MagazineCapacity );
		m_Renderer.material.SetColor( "_EmissionColor", Color.Lerp( m_StartEmissiveColor, Color.clear, interpolant ) );
	}

}
