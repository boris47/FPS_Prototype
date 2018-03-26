
using UnityEngine;
using System.Collections;

public class DroneArmored : Drone {

	protected override void Update()
	{

		base.Update();
	}


	// Hitted by ranged weapon
	public override void OnHit( ref Entity who, float damage )
	{
		
	}


	// Hitted by closed range weapon
	public override void OnHurt( ref Entity who, float damage )
	{
		
	}


	public override void OnKill()
	{
		base.OnKill();
		m_Pool.Destroy();
		Destroy( gameObject );
	}


	public override void OnThink()
	{
		
	}

}
