
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

	public		override	void	Shoot( Vector3 position, Vector3 direction, float velocity )
	{
		if (this.m_BulletMotionType != EBulletMotionType.INSTANT)
		{
			Debug.LogError( $"Syphon bullet can only have motion type {EBulletMotionType.INSTANT.ToString()}" );
		}
		this.ShootInstant( position, direction, velocity );
	}
	
}
