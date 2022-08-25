using UnityEngine;


public class SwimVolumeConfigs : ConfigurationBase
{
	[SerializeField]
	private				float							m_FloatingForce						= 7f;



	public				float							FloatingForce						=> m_FloatingForce;
}
