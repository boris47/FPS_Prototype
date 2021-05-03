using UnityEngine;

public class BulletBallistic : BulletGeneric
{
	//////////////////////////////////////////////////////////////////////////
	public override void Shoot(in Vector3 origin, in Vector3 direction, in float velocity, in float impactForceMultiplier)
	{
		base.Shoot(origin, direction, velocity, impactForceMultiplier);

		switch (m_BulletMotionType)
		{
			case EBulletMotionType.INSTANT:		ShootInstant(origin, direction);	break;
			case EBulletMotionType.DIRECT:		ShootDirect(origin, direction);		break;
			case EBulletMotionType.PARABOLIC:	ShootParabolic(origin, direction);	break;
		}
	}
}
