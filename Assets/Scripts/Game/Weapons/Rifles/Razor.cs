
using UnityEngine;
using UnityEngine.UI;

public class Razor : Weapon
{
	[Header("Razor Properties")]

	private		Color							m_StartEmissiveColor				= Color.clear;

	private		Canvas							m_Canvas							= null;
	private		Image							m_Panel								= null;
	private		Text							m_AmmoText							= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override	void			Awake()
	{
		base.Awake();

		m_StartEmissiveColor = m_Renderer.material.GetColor( "_EmissionColor" );

		m_Canvas	= GetComponentInChildren<Canvas>();
		m_Panel		= m_Canvas.transform.GetChild(0).GetComponent<Image>();
		m_AmmoText	= m_Panel.transform.GetChild(0).GetComponent<Text>();
		m_AmmoText.text = m_Magazine.ToString();

//		StartCoroutine( ChangeColor( m_Panel ) );
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

	private	Color	colorToSet = new Color();
	private bool inTransition = false;
	System.Collections.IEnumerator ChangeColor( Image image )
	{
		float interpolant = 0f;
		while( true )
		{
			if ( inTransition == false )
			{
				colorToSet.r = Random.value;
				colorToSet.g = Random.value;
				colorToSet.b = Random.value;
				colorToSet.a = Random.Range( 0.2f, 0.3f );
				inTransition = true;
			}

			while( interpolant < 0.05f )
			{
				image.color = Color.Lerp( image.color, colorToSet, interpolant );
				interpolant += Time.unscaledDeltaTime * 0.09f;
				yield return null;
			}
			interpolant = 0f;
			inTransition = false;
			yield return null;
		}
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
		m_Renderer.material.SetColor( "_EmissionColor", m_StartEmissiveColor );
		m_AmmoText.text = m_Magazine.ToString();
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
		m_AmmoText.text = m_Magazine.ToString();
//		UI.Instance.InGame.UpdateUI();

		float interpolant = 1f - ( (float)m_Magazine / (float)m_MagazineCapacity );
		m_Renderer.material.SetColor( "_EmissionColor", Color.Lerp( m_StartEmissiveColor, Color.clear, interpolant ) );
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
