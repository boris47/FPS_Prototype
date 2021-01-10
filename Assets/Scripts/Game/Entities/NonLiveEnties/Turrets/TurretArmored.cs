
using UnityEngine;


public class TurretArmored : Turret {

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = GetType().FullName;

		base.Awake();
	}

}
