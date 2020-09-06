
using UnityEngine;


public class TurretArmored : Turret {

	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected override void Awake()
	{
		this.m_SectionName = this.GetType().FullName;

		base.Awake();
	}

}
