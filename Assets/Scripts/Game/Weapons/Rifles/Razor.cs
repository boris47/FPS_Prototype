
using UnityEngine;

public class Razor : Weapon
{
	private		Color							m_StartEmissiveColor				= Color.clear;


	//////////////////////////////////////////////////////////////////////////
	// Awake ( Override )
	protected	override void	Awake()
	{
		base.Awake();

		m_StartEmissiveColor = m_Renderer.material.GetColor( "_EmissionColor" );
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	protected override		void Update()
	{
		m_FireTimer -= Time.deltaTime;
		
		if ( InputManager.Inputs.ItemAction1 && m_InTransition == false && m_IsRecharging == false )
		{
			if ( WeaponManager.Instance.Zoomed == false )
				WeaponManager.Instance.ZoomIn( this, m_ZoomOffset, m_ZoomingTime );
			else
				WeaponManager.Instance.ZoomOut();
		}

		// Reloading
		if ( m_LockTimer > 0f )
		{
			m_LockTimer -= Time.deltaTime;
			return;
		}
		
		// Just after reload
		if ( m_LockTimer < 0f )
		{
//			anim.speed = m_AnimatorStdSpeed;
			m_LockTimer = 0f;
			if ( m_IsRecharging == true )
			{
				m_IsRecharging = false;
				m_Magazine = m_MagazineCapacity;
				UI.Instance.InGame.UpdateUI();
				m_Renderer.material.SetColor( "_EmissionColor", m_StartEmissiveColor );
			}
			m_BrustCount = 0;
			m_NeedRecharge = false;
		}


		// Fire mode cycle
		if ( InputManager.Inputs.ItemAction2 )
		{
			if ( m_FireMode == FireModes.AUTO )
				m_FireMode = FireModes.SINGLE;
			else
				m_FireMode ++;

			UI.Instance.InGame.UpdateUI();
		}

		// End For Brust mode
		if ( m_BrustCount > 0 && InputManager.Inputs.Fire1Released )
		{
			m_BrustCount = 0;
		}

		if ( m_FireTimer > 0 )
			return;

		m_IsFiring = false;

		if ( m_FirstFireAvaiable && m_Magazine > 0 && m_InTransition == false && m_NeedRecharge == false )
		{
			switch ( m_FireMode )
			{
				case FireModes.SINGLE:
					FireSingleMode();
					break;
				case FireModes.BURST:
					FireBrustMode();
					break;
				case FireModes.AUTO:
					FireAutoMode();
					break;
			}
		}
		

		if ( Player.Instance.IsRunning && WeaponManager.Instance.Zoomed && m_InTransition == false )
		{
			WeaponManager.Instance.ZoomOut();
		}

		if ( m_Magazine <= 0 || ( InputManager.Inputs.Reload && m_Magazine < m_MagazineCapacity ) || m_NeedRecharge )
		{
//			m_AnimatorStdSpeed = anim.speed;
//			anim.speed = 2f;

			if ( WeaponManager.Instance.Zoomed )
			{
				if ( m_InTransition == false )
				{
					WeaponManager.Instance.ZoomOut();
					m_NeedRecharge = true;
				}
				return;
			}

			m_Animator.Play( m_ReloadAnim.name, -1, 0f );
			m_LockTimer = m_ReloadAnim.length; // / 2f;
			m_IsRecharging = true;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// ConfigureShot
	protected override		void	ConfigureShot( bool fireFirst )
	{
		base.ConfigureShot( fireFirst );

		float interpolant = 1f - ( (float)m_Magazine / (float)m_MagazineCapacity );
		m_Renderer.material.SetColor( "_EmissionColor", Color.Lerp( m_StartEmissiveColor, Color.clear, interpolant ) );
	}

}
