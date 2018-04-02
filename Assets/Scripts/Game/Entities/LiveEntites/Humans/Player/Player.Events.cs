
using System.Collections;
using UnityEngine;

public partial class Player {

	public	override		void	OnHit( ref Entity who, float damage )
	{
		Health -= damage;
		UI_InGame.Instance.UpdateUI();

		if ( Health < 0f )
			OnKill();
	}


	public	override		void	OnHurt( ref Entity who, float damage )
	{
		Health -= damage;
		UI_InGame.Instance.UpdateUI();

		if ( Health < 0f )
			OnKill();
	}


	public	override		void	OnKill()
	{
		print( "U r dead" );
		this.enabled = false;
		CameraControl.Instance.enabled = false;
		UI_InGame.Instance.UpdateUI();
	}


	private					void	OnDashTargetUsed( ref DashTarget target )
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