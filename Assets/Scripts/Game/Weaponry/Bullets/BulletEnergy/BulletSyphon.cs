
using UnityEngine;

public class BulletSyphon : BulletEnergy
{
	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		CustomAssertions.IsTrue(m_BulletMotionType == EBulletMotionType.INSTANT, $"{GetType().Name} can only have motion type {EBulletMotionType.INSTANT.ToString()}");
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame(float deltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	public override void Shoot(in Vector3 origin, in Vector3 direction, in float velocity, in float impactForceMultiplier)
	{
		base.Shoot(origin, direction, velocity, impactForceMultiplier);

		ShootInstant(origin, direction);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnCollisionDetailed(in Vector3 point, in Vector3 normal, in Collider otherCollider)
	{
		// Handled directly in syphon fire module
	}
}
