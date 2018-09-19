
using UnityEngine;

public partial class Player {

	private		float				m_DamageEffect					= 0f;


	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public		override	void	OnTargetAquired( TargetInfo_t targetInfo )
	{

	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetUpdate ( Override )
	public		override	void	OnTargetUpdate( TargetInfo_t targetInfo )
	{

	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetChanged ( Override )
	public		override	void	OnTargetChanged( TargetInfo_t targetInfo )
	{

	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public		override	void	OnTargetLost( TargetInfo_t targetInfo )
	{

	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public		override	void	OnHit( IBullet bullet )
	{
		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;
		UI.Instance.InGame.UpdateUI();

		m_DamageEffect = 0.2f;

		if ( m_Health < 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public		override	void	OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
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

}