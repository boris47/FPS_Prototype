
using UnityEngine;
using System.Collections;

public class DroneStandard : Drone {

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();
	}

}
