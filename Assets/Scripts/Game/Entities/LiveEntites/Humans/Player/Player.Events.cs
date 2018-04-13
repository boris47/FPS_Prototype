
using System.Collections;
using UnityEngine;

public partial class Player {

	public override void OnTargetAquired( TargetInfo_t targetInfo )	{ }
	public override void OnTargetChanged( TargetInfo_t targetInfo ) { }
	public override void OnTargetLost( TargetInfo_t targetInfo )	{ }


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public	override	void	OnHit( ref IBullet bullet )
	{
		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;
		UI.Instance.InGame.UpdateUI();

		if ( m_Health < 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Override )
	public	override		void	OnKill()
	{
		print( "U r dead" );
		this.enabled = false;
		CameraControl.Instance.enabled = false;
		UI.Instance.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDashTargetUsed
	private				void	OnDashTargetUsed( ref DashTarget target )
	{
		if ( m_IsDashing )
			return;

		m_IsDashing = true;

		if ( m_PreviousDashTarget != null && m_PreviousDashTarget != target )
		{
			m_PreviousDashTarget.OnReset();
		}
		m_PreviousDashTarget = target;

		target.Disable();
		target.HideText();

		m_RigidBody.velocity = Vector3.zero;

		StartCoroutine( DashMoving( target ) ); // Player.DashAbility
	}

}