
using UnityEngine;

public class M4A1 : Weapon {


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			Awake()
	{
		base.Awake();
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	public		override	bool			CanChangeWeapon()
	{
		return base.CanChangeWeapon();
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		return base.OnLoad( streamingData );
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	StreamingUnit	OnSave( StreamingData streamingData )
	{
		return base.OnSave( streamingData );
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			SelectFireFunction()
	{
		base.SelectFireFunction();
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			Update()
	{
		base.Update();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEndReload ( Override )
	protected	override	void			OnEndReload()
	{
		base.OnEndReload();
	}

	
	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	public		override	void			OnWeaponChange()
	{
		base.OnWeaponChange();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSecondaryFire
	protected	override	void			OnSecondaryFire()
	{
		if ( WeaponManager.Instance.Zoomed == false )
			WeaponManager.Instance.ZoomIn( this, m_ZoomOffset, m_ZoomingTime );
		else
			WeaponManager.Instance.ZoomOut();
	}


	//////////////////////////////////////////////////////////////////////////
	// FireSingleMode
	protected	override	void			OnFireSingleMode()
	{
		if ( InputManager.Inputs.Fire1 )
		{
			ConfigureShot();
			m_IsFiring = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireBrustMode
	protected	override	void			OnFireBrustMode()
	{
		// Start of brust
		if ( InputManager.Inputs.Fire1Loop && m_BrustCount < m_BrustSize )
		{
			m_BrustCount ++;

			ConfigureShot();
			m_IsFiring = true;
		}

		// End of brust
		if ( InputManager.Inputs.Fire1Released && m_BrustCount > 0 )
		{
			m_BrustCount = 0;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireAutoMode
	protected	override	void			OnFireAutoMode()
	{
		if ( ( InputManager.Inputs.Fire1Loop ) )
		{
			ConfigureShot();
			m_IsFiring = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	protected	override	void			ConfigureShot()
	{
		m_FireTimer = m_ShotDelay;

		m_Animator.Play( m_FireAnim.name, -1, 0f );
			
		m_Magazine --;

		// BULLET
		IBullet bullet = m_PoolBullets1.GetComponent();

		// POSITION
		Vector3 position = m_FirePoint.position;

		// DIRECTION
		m_DispersionVector.Set( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) );
		m_DispersionVector /= WeaponManager.Instance.Zoomed ? m_ZoomFactor : 1f;

		Vector3 direction = m_FirePoint.forward;

		// AUDIOSOURCE
		ICustomAudioSource audioSource = m_AudioSourceFire1;

		// CAM DISPERSION
		float finalDispersion = m_CamDeviation * bullet.RecoilMult;
		finalDispersion	*= Player.Instance.IsCrouched			? 0.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsMoving				? 1.50f : 1.00f;
		finalDispersion	*= Player.Instance.IsRunning			? 2.00f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.SINGLE )	? 0.50f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.BURST )	? 0.80f : 1.00f;
		finalDispersion *= ( m_FireMode == FireModes.AUTO )		? 1.10f : 1.00f;
		finalDispersion	*= WeaponManager.Instance.Zoomed		? 0.80f : 1.00f;

		// SHOOT
		Shoot( bullet, position, direction, audioSource, finalDispersion );

		// UPDATE UI
		UI.Instance.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// Shoot ( Override )
	protected	override		void		Shoot( IBullet bullet, Vector3 position, Vector3 direction, ICustomAudioSource audioSource, float camDispersion )
	{
		bullet.Shoot( position: position, direction: direction );
		audioSource.Play();
		CameraControl.Instance.ApplyDispersion( camDispersion );
	}
}
