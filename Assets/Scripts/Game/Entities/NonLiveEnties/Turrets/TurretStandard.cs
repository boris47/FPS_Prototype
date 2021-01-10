
using UnityEngine;


public class TurretStandard : Turret {
	
	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = GetType().FullName;

		base.Awake();
	}

}
