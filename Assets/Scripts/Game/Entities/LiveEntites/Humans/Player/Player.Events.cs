
using System.Collections;
using UnityEngine;

public partial class Player {

	private		float				m_DamageEffect					= 0f;


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

		m_DamageEffect = 0.2f;

		if ( m_Health < 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Override )
	public	override		void	OnKill()
	{
		var settings = CameraControl.Instance.GetPP_Profile.vignette.settings;
		settings.intensity = 0f;
		CameraControl.Instance.GetPP_Profile.vignette.settings = settings;


		print( "U r dead" );
		CameraControl.Instance.enabled = false;
		UI.Instance.InGame.UpdateUI();
		gameObject.SetActive( false );
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