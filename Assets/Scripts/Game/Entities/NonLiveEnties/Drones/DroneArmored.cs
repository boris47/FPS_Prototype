
using UnityEngine;
using System.Collections;

public class DroneArmored : Drone {

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = GetType().FullName;

		base.Awake();
	}

}
