
using UnityEngine;
using System.Collections;

public class TurretArmored : Turret {

	protected override void Update()
	{
		base.Update();

		m_ShotTimer -= Time.deltaTime;
		if ( m_ShotTimer > 0 )
				return;

		if ( m_CurrentTarget == null )
			return;

		m_ShotTimer = m_ShotDelay;

		Bullet bullet = m_Pool.Get<Bullet>();
		bullet.enabled = true;
		bullet.transform.position = m_FirePoint.position;
		bullet.MaxLifeTime = 5f;
		bullet.SetVelocity( m_GunTransform.forward * m_BulletSpeed );
		bullet.SetActive( true );
		
		m_FireAudioSource.Play();
	}

	// Hitted by ranged weapon
	public override void OnHit( ref Entity who, float damage )
	{
		m_Health -= damage;

		if ( m_Health < 0f )
			OnKill();
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
		if ( m_Targets.Count == 0 )
		{
			m_CurrentTarget = null;
			return;
		}

		if ( m_Targets.Count > 0 )
			m_CurrentTarget = Entity.GetBestTarget( ref m_Targets, transform.position );
	}

}
