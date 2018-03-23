
using UnityEngine;
using System.Collections;

public class TurretStandard : Turret {

	protected override void Update()
	{
		base.Update();

		m_ShotTimer -= Time.deltaTime;

		if ( m_ShotTimer > 0 )
				return;

		m_ShotTimer = m_ShotDelay;

		Rigidbody bullet = m_Pool.Get<Rigidbody>();
		bullet.transform.position = m_FirePoint.position;
		bullet.velocity = m_GunTransform.forward * m_BulletSpeed;
		bullet.detectCollisions = true;
		m_FireAudioSource.Play();
	}

	// Hitted by ranged weapon
	public override void OnHit( HitInfo info )	// info = null
	{
		float	damage = Player.Instance.CurrentWeapon.Damage;
		m_Health -= damage;

		if ( m_Health < 0f )
			OnKill();
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
