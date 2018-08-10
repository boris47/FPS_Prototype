
using UnityEngine;

public partial class Player {

	private		float				m_DamageEffect					= 0f;


	public override void OnTargetAquired( TargetInfo_t targetInfo )	{ }
	public override void OnTargetChanged( TargetInfo_t targetInfo ) { }
	public override void OnTargetLost( TargetInfo_t targetInfo )	{ }


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public	override	void	OnHit( IBullet bullet )
	{
		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;
		UI.Instance.InGame.UpdateUI();

		m_DamageEffect = 0.2f;

		if ( m_Health < 0f )
			OnKill();
	}

	public override void OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Override )
	public	override		void	OnKill()
	{
		// remove parent for camera
		CameraControl.Instance.Transform.SetParent( null );

		m_IsActive = false;

		// reset effect
		var settings = CameraControl.Instance.GetPP_Profile.vignette.settings;
		settings.intensity = 0f;
		CameraControl.Instance.GetPP_Profile.vignette.settings = settings;

		// disable weapon actions
		WeaponManager.Instance.CurrentWeapon.Enabled = false;
		WeaponManager.Instance.Enabled = false;
		
		
		// Disable camera updates
		CameraControl.Instance.Enabled = false;

		// Update UI elements
		UI.Instance.InGame.UpdateUI();

		// Turn off player object
		gameObject.SetActive( false );

		// print a message
		print( "U r dead" );
	}
	/*
	
	//////////////////////////////////////////////////////////////////////////
	// OnDashTargetUsed
	private				void	OnDodgeTargetUsed( ref DodgeTarget target )
	{
		if ( m_IsDodging )
			return;

//		m_IsDodging = true;

		if ( m_PreviousDodgeTarget != null && m_PreviousDodgeTarget != target )
		{
			m_PreviousDodgeTarget.OnReset();
		}
		m_PreviousDodgeTarget = target;

		target.Disable();
		target.HideText();

//		StartCoroutine( Dodge( destination: target.transform.position, destinationUp: target.transform.up, falling: false, target: target) ); // Player.DodgeAbility
	}
	*/
}