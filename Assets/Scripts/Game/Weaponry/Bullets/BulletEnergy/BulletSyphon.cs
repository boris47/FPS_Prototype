
using UnityEngine;

public class BulletSyphon : BulletEnergy
{
	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		CustomAssertions.IsTrue(m_BulletMotionType == EBulletMotionType.INSTANT, $"Syphon bullet can only have motion type { EBulletMotionType.INSTANT.ToString()}");
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnFrame(float deltaTime)
	{

	}


	//////////////////////////////////////////////////////////////////////////
	public override void Shoot(in Vector3 position, in Vector3 direction, in float? velocity, in float? impactForceMultiplier)
	{
		float finalVelocity = (velocity ?? m_Velocity);
		float finalImpactForceMultiplier = impactForceMultiplier ?? m_ImpactForceMultiplier;
		transform.position = position;
		ShootInstant(position, direction, finalVelocity, finalImpactForceMultiplier);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnCollisionDetailed(in Vector3 point, in Vector3 normal, in Collider otherCollider)
	{
		// Handled directly in syphon fire module
	}
}
