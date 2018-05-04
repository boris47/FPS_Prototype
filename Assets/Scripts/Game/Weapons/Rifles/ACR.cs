
using UnityEngine;

public class ACR : Weapon
{

	[SerializeField]
	private             int					m_BuckshotSize          = 6;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			Awake()
	{
		base.Awake();

		SelectFireFunction();
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
		m_FireMode = FireModes.SINGLE;
		base.SelectFireFunction();
		m_FireMode = FireModes.SINGLE;
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
			for ( int i = 0; i < m_BuckshotSize; i++ )
			{
				ConfigureShot();
				m_IsFiring = true;
			}
			m_Magazine --;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// FireBrustMode
	protected	override	void			OnFireBrustMode()
	{}


	//////////////////////////////////////////////////////////////////////////
	// FireAutoMode
	protected	override	void			OnFireAutoMode()
	{}

	public float maxDeviation = 0.2f;
	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	protected	override	void			ConfigureShot()
	{
		m_FireTimer = m_ShotDelay;

		m_Animator.Play( m_FireAnim.name, -1, 0f );

		// BULLET
		IBullet bullet = m_PoolBullets1.GetComponent();

		// POSITION
		Vector3 position = m_FirePoint.position;

		// DIRECTION
		m_DispersionVector.Set
		(
			Random.Range( -maxDeviation, maxDeviation ),
			Random.Range( -maxDeviation, maxDeviation ),
			Random.Range( -maxDeviation, maxDeviation )
		);

		Vector3 direction = ( m_FirePoint.forward + m_DispersionVector ).normalized;

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
