
using UnityEngine;


public class BulletSyphon : BulletEnergy
{
	protected override void Awake()
	{
		base.Awake();
	}
	
	protected override void Update()
	{
		
	}

	protected override void OnCollisionDetailed(in Vector3 point, in Vector3 normal, in Collider otherCollider)
	{
		
	}

	public		override	void	Shoot( Vector3 position, Vector3 direction, float? velocity )
	{
		if (m_BulletMotionType != EBulletMotionType.INSTANT)
		{
			Debug.LogError( $"Syphon bullet can only have motion type {EBulletMotionType.INSTANT.ToString()}" );
		}
		ShootInstant( position, direction, velocity );
	}
	
}
