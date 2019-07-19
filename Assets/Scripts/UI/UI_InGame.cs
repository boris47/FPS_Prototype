
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour, IStateDefiner {

	private			Transform		m_GenericInfosPanel			= null;
	private			Text			m_TimeText					= null;
	private			Text			m_CycleNameText				= null;
	private			Text			m_HealthText				= null;
	private			Text			m_Timetime					= null;

	private			Transform		m_WeaponInfosPanel				= null;
	private			Text			m_WpnNameText				= null;
	private			Text			m_WpnOtherInfoText			= null;

//	private			Image			m_StaminaBarImage			= null;
	private			Transform		m_CrosshairTransform		= null;

	private			Image			m_ZoomFrameImage			= null;

	private			Canvas			m_Canvas					= null;


	private			bool			m_IsActive					= false;

	private			bool			m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		m_bIsInitialized = true;
		{
			m_bIsInitialized &= transform.childCount > 1;

			m_bIsInitialized &= transform.SearchComponent( ref m_Canvas, SearchContext.LOCAL );

			if ( m_bIsInitialized )
			{
				m_GenericInfosPanel = transform.Find( "GenericInfosPanel" );
				{
					m_bIsInitialized &=	m_GenericInfosPanel.SearchComponentInChild( 0, ref m_CycleNameText );
					m_bIsInitialized &=	m_GenericInfosPanel.SearchComponentInChild( 1, ref m_TimeText);
					m_bIsInitialized &=	m_GenericInfosPanel.SearchComponentInChild( 2, ref m_HealthText );
					m_bIsInitialized &= m_GenericInfosPanel.SearchComponentInChild( 3, ref m_Timetime );
				}
			}

			if ( m_bIsInitialized )
			{
				m_WeaponInfosPanel = transform.Find( "WeaponInfosPanel" );
				{
					m_bIsInitialized &=	m_WeaponInfosPanel.SearchComponentInChild( 0, ref m_WpnNameText );
					m_bIsInitialized &=	m_WeaponInfosPanel.SearchComponentInChild( 2, ref m_WpnOtherInfoText );
	//				m_bIsInitialized &=	m_Panel2.SearchComponentInChild( 3, ref m_StaminaBarImage );
				}
			}

			m_bIsInitialized &= transform.SearchComponentInChild( "UI_Frame", ref m_ZoomFrameImage );

			m_bIsInitialized &= ( m_CrosshairTransform = transform.Find( "Crosshair" )) != null;
			if ( m_bIsInitialized )
			{
				m_ZoomFrameImage.raycastTarget = false;

				UI_Graphics.OnResolutionChanged += UI_Graphics_OnResolutionChanged;

				InvokeRepeating( "PrintTime", 1.0f, 1.0f );	
			}
			else
			{
				Debug.LogError( "UI_InGame: Bad initialization!!!" );
			}
		}
	}



	//////////////////////////////////////////////////////////////////////////
	private void UI_Graphics_OnResolutionChanged( float newWidth, float newHeight )
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator	IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		m_IsActive = true;

//		UI.Instance.EffectFrame.color = Color.clear;

//		SoundManager.Instance.OnSceneLoaded();

		// Reset Ingame UI
//		InternalReset();

		GlobalManager.SetCursorVisibility( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// Reset
	private void InternalReset()
	{
		m_ZoomFrameImage.enabled	= false;
		m_ZoomFrameImage.sprite		= null;
		m_ZoomFrameImage.color		= Color.clear;
		m_ZoomFrameImage.material	= null;
		ShowCrosshair();
		Show();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private	void	OnDisable()
	{
		m_IsActive = false;
	}

	//////////////////////////////////////////////////////////////////////////
	// Show
	public	void	Show()
	{
		m_GenericInfosPanel.gameObject.SetActive( true );
		m_WeaponInfosPanel.gameObject.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// Hide
	public	void	Hide()
	{
		m_GenericInfosPanel.gameObject.SetActive( false );
		m_WeaponInfosPanel.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateUI
	public	void	UpdateUI()
	{
		if ( m_IsActive == false || m_bIsInitialized == false )
			return;

		IEntity player				= Player.Instance as IEntity;

		m_HealthText.text			= Mathf.CeilToInt( player.Health ).ToString();

		m_WpnNameText.text			= WeaponManager.Instance.CurrentWeapon.Transform.name;
		m_WpnOtherInfoText.text		= WeaponManager.Instance.CurrentWeapon.OtherInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	// ShowCrosshair
	public	void	ShowCrosshair()
	{
		m_CrosshairTransform.gameObject.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// HideCrosshair
	public	void	HideCrosshair()
	{
		m_CrosshairTransform.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// SetFrame
	public void SetFrame( Image frame )
	{
		if ( frame != null )
		{
			// Size
			m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, frame.rectTransform.rect.width  );
			m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical,   frame.rectTransform.rect.height );

			m_ZoomFrameImage.sprite		= frame.sprite;
			m_ZoomFrameImage.color		= frame.color;
			m_ZoomFrameImage.material	= frame.material;
			m_ZoomFrameImage.enabled	= true;
			HideCrosshair();
			Hide();
		}
		else
		{
			m_ZoomFrameImage.enabled	= false;
			m_ZoomFrameImage.sprite		= null;
			m_ZoomFrameImage.color		= Color.clear;
			m_ZoomFrameImage.material	= null;
			ShowCrosshair();
			Show();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// PrintTime
	public void FrameFeedBack( float feedback, Vector2 delta )
	{
		if ( m_ZoomFrameImage.enabled == true )
		{
			m_ZoomFrameImage.rectTransform.localScale = Vector3.one * feedback;
			m_ZoomFrameImage.rectTransform.position = delta;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// PrintTime
	private	void	PrintTime()
	{
		if ( m_IsActive == false )
			return;

		if ( WeatherSystem.WeatherManager.Instance != null )
		{
			m_TimeText.text	= WeatherSystem.WeatherManager.Cycles.GetTimeAsString();
			m_CycleNameText.text	= WeatherSystem.WeatherManager.Cycles.CurrentCycleName;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		if ( m_IsActive == false || m_bIsInitialized == false )
			return;

		// Only every 10 frames
		if ( Time.frameCount % 10 == 0 )
			return;

		m_Timetime.text = Time.timeScale.ToString();
//		staminaBar.fillAmount = Player.Instance.Stamina;
		
	}
	
}
