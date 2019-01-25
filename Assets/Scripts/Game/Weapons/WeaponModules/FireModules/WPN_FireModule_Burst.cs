using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//////////////////////////////////////////////////////////////////////////
// WPN_FireModule_Burst
[System.Serializable]
public class WPN_FireModule_Burst : WPN_FireModule {

	// Burst
	[SerializeField]
	protected	uint						m_BurstCount				= 0;
	public		uint						BurstCount
	{
		get { return m_BurstCount; }
	}

	[SerializeField]
	protected		uint					m_BurstSize					= 0;
	public		uint						BurstSize
	{
		get { return m_BurstSize; }
	}

	// Deviation
	protected		bool					m_ApplyDeviation			= false;

	//
	public override FireModes FireMode
	{
		get { return FireModes.BURST; }
	}

	// CONTRUCTOR
	public	override void Setup( IWeapon w )
	{
		base.Setup( w );
		string sectionName = this.GetType().FullName;
		Database.Section section = null;
		if ( GameManager.Configs.bGetSection( sectionName, ref section ) )
		{
			m_BurstSize = section.AsInt( "BurstSize", m_BurstSize );
			m_ApplyDeviation = section.AsBool( "ApplyDeviationOnLastShot", m_ApplyDeviation );
		}
	}


	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	void	OnWeaponChange	()
	{
		m_BurstCount = 0;
	}

	protected	override	float	GetFireDispersion()
	{
		return m_BaseFireDispersion;
	}
	protected	override	float	GetCamDeviation()
	{
		return m_BaseCamDeviation;
	}

	//
	public override bool CanBeUsed()
	{
		return base.CanBeUsed() && !( m_BurstCount >= m_BurstSize ) && m_FireDelay <= 0.0f;
	}


	//	INTERNAL UPDATE
	public	override	void	InternalUpdate()
	{
		m_FireDelay -= Time.deltaTime;
	}

	//	START
	public override		void	OnStart()
	{
		if ( CanBeUsed() )
		{
			Shoot( m_BaseFireDispersion, m_BaseCamDeviation );
			m_BurstCount ++;
		}
	}

	//	INTERNAL UPDATE
	public	override	void	OnUpdate()
	{
		if ( CanBeUsed() )
		{
			Shoot( GetFireDispersion(), GetCamDeviation() );
			m_BurstCount ++;
		}
	}

	//	END
	public override		void	OnEnd()
	{
		m_BurstCount = 0;
	}

}





[System.Serializable]
public class WPN_FireModule_BurstAuto : WPN_FireModule_Burst {

	[SerializeField]
	private	bool	m_BurstActive = false;
	

	protected	override	float	GetFireDispersion()
	{
		float fireDispersion = ( m_ApplyDeviation && m_BurstCount == m_BurstSize-1 ) ? m_BaseFireDispersion : 0.0f;
		return fireDispersion;
	}
	protected	override	float	GetCamDeviation()
	{
		return m_BaseCamDeviation;
	}


	//	INTERNAL UPDATE
	public	override	void	InternalUpdate()
	{
		m_FireDelay -= Time.deltaTime;

		if ( CanBeUsed() && m_BurstActive == true )
		{
			Shoot( GetFireDispersion(), GetCamDeviation() );

			m_BurstCount ++;

			if ( m_BurstCount >= m_BurstSize || m_Magazine == 0 )
			{
				m_BurstCount = 0;
				m_BurstActive = false;
			}
		}
	}

	public override bool CanChangeWeapon()
	{
		return m_BurstActive == false;
	}

	//	START
	public override void OnStart()
	{
		if ( CanBeUsed() )
		{
			float fireDispersion = ( !m_ApplyDeviation ) ? m_BaseFireDispersion : 0.0f;
			Shoot( fireDispersion, GetCamDeviation() );

			m_BurstCount ++;
			m_BurstActive = true;
		}
	}

	//	INTERNAL UPDATE
	public override void OnUpdate()
	{

	}

	//	END
	public override void OnEnd()
	{
		
	}
}