
using UnityEngine;

public class TurretStandard : Turret {

	protected override void Update()
	{
		base.Update();

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
