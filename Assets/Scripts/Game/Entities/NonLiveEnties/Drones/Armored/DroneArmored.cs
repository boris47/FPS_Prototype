
using UnityEngine;
using System.Collections;

public class DroneArmored : Drone {

	protected override void Update()
	{

		base.Update();
	}

	// Hitted by ranged weapon
	public override void OnHit( HitInfo info )
	{
		
	}

	// Hitted by closed range weapon
	public override void OnHurt( HurtInfo info )
	{
		
	}

	public override void OnKill( HitInfo info = null )
	{
		m_Pool.Destroy();
		Destroy( gameObject );
	}

}
