
using UnityEngine;
using System.Collections;

public class DroneArmored : Drone {

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		this.m_SectionName = this.GetType().FullName;

		base.Awake();
	}

}
