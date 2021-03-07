
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_InGame : UI_Base, IStateDefiner
{

	private			Transform		m_GenericInfosPanel				= null;
	private			Text			m_TimeText						= null;
	private			Text			m_CycleNameText					= null;
	private			Text			m_HealthText					= null;
	private			Text			m_Timetime						= null;

	private			Transform		m_WeaponInfosPanel				= null;
	private			Text			m_WpnNameText					= null;
	private			Text			m_WpnOtherInfoText				= null;

	private			Image			m_StaminaBarImage				= null;
	private			Transform		m_CrosshairsTransform			= null;

	private			Image			m_ZoomFrameImage				= null;

	private			Canvas			m_Canvas						= null;

	private			bool			m_IsActive						= false;

	private			bool			m_IsCompletedInitialization		= false;
	private			bool			m_IsInitialized					= false;

					bool			IStateDefiner.IsInitialized		=> m_IsInitialized;
					string			IStateDefiner.StateName			=> name;



	//////////////////////////////////////////////////////////////////////////
	public void PreInit()
	{
		m_IsInitialized = true;
		m_IsInitialized &= transform.childCount > 1;

		m_IsInitialized &= transform.TrySearchComponent(ESearchContext.LOCAL, out m_Canvas);

		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_Frame", out m_ZoomFrameImage );

		if ( m_IsInitialized &= transform.TrySearchComponentByChildName( "GenericInfosPanel", out m_GenericInfosPanel ) )
		{
			m_IsInitialized &= m_GenericInfosPanel.TrySearchComponentByChildIndex( 0, out m_CycleNameText );
			m_IsInitialized &= m_GenericInfosPanel.TrySearchComponentByChildIndex( 1, out m_TimeText );
			m_IsInitialized &= m_GenericInfosPanel.TrySearchComponentByChildIndex( 2, out m_HealthText );
			m_IsInitialized &= m_GenericInfosPanel.TrySearchComponentByChildIndex( 3, out m_Timetime );
		}

		if ( m_IsInitialized &= transform.TrySearchComponentByChildName( "WeaponInfosPanel", out m_WeaponInfosPanel ) )
		{
			m_IsInitialized &= m_WeaponInfosPanel.TrySearchComponentByChildIndex( 0, out m_WpnNameText );
			m_IsInitialized &= m_WeaponInfosPanel.TrySearchComponentByChildIndex( 2, out m_WpnOtherInfoText );
			m_IsInitialized &= m_WeaponInfosPanel.TrySearchComponentByChildIndex( 3, out m_StaminaBarImage );
		}

		m_IsInitialized &= transform.TrySearchComponentByChildName( "Crosshairs", out m_CrosshairsTransform );
	}

	//////////////////////////////////////////////////////////////////////////
	IEnumerator IStateDefiner.Initialize()
	{
		if (m_IsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		if ( m_IsInitialized )
		{
			m_ZoomFrameImage.raycastTarget = false;

			UserSettings.VideoSettings.OnResolutionChanged += UI_Graphics_OnResolutionChanged;

			InvokeRepeating( "PrintTime", 1.0f, 1.0f );

			CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );

			yield return null;

			m_IsCompletedInitialization = true;
		}
		else
		{
			Debug.LogError( "UI_InGame: Bad initialization!!!" );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void UI_Graphics_OnResolutionChanged( float newWidth, float newHeight )
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	IEnumerator	IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
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
	private void InternalReset()
	{
		m_ZoomFrameImage.enabled	= false;
		m_ZoomFrameImage.sprite		= null;
		m_ZoomFrameImage.color		= Color.clear;
		m_ZoomFrameImage.material	= null;
		ShowCrosshairs();
		Show();
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnDisable()
	{
		m_IsActive = false;
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Show()
	{
		m_GenericInfosPanel.gameObject.SetActive( true );
		m_WeaponInfosPanel.gameObject.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Hide()
	{
		m_GenericInfosPanel.gameObject.SetActive( false );
		m_WeaponInfosPanel.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	UpdateUI()
	{
		if (m_IsActive == false || m_IsCompletedInitialization == false )
			return;

		Entity player				= Player.Instance;
		m_HealthText.text			= Mathf.CeilToInt( player.Health ).ToString();
		m_WpnNameText.text			= WeaponManager.Instance.CurrentWeapon.Transform.name;
//		m_WpnOtherInfoText.text		= WeaponManager.Instance.CurrentWeapon.OtherInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	ShowCrosshairs()
	{
		m_CrosshairsTransform.gameObject.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	public	UI_BaseCrosshair	EnableCrosshair(System.Type crosshairType)
	{
		UI_BaseCrosshair crosshair = m_CrosshairsTransform.GetComponentInChildren(crosshairType, includeInactive: true) as UI_BaseCrosshair;
		crosshair?.AddRef();
		return crosshair;
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	RemoveCrosshair(UI_BaseCrosshair crosshair)
	{
		crosshair.RemoveRef();
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	HideCrosshairs()
	{
		m_CrosshairsTransform.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetFrame( Image frame )
	{
		if ( frame != null )
		{
			// Size
			m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, frame.rectTransform.rect.width  );
			m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical,   frame.rectTransform.rect.height );

			m_ZoomFrameImage.sprite	= frame.sprite;
			m_ZoomFrameImage.color		= frame.color;
			m_ZoomFrameImage.material	= frame.material;
			m_ZoomFrameImage.enabled	= true;
			HideCrosshairs();
			Hide();
		}
		else
		{
			m_ZoomFrameImage.enabled	= false;
			m_ZoomFrameImage.sprite	= null;
			m_ZoomFrameImage.color		= Color.clear;
			m_ZoomFrameImage.material	= null;
			ShowCrosshairs();
			Show();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void FrameFeedBack( float feedback, Vector2 delta )
	{
		if (m_ZoomFrameImage.enabled == true )
		{
			m_ZoomFrameImage.rectTransform.localScale = Vector3.one * feedback;
			m_ZoomFrameImage.rectTransform.position = delta;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	PrintTime()
	{
		if (m_IsActive == false || m_IsCompletedInitialization == false )
			return;

		if ( WeatherSystem.WeatherManager.Instance != null )
		{
			m_TimeText.text	= WeatherSystem.WeatherManager.Cycles.GetTimeAsString();
			m_CycleNameText.text	= WeatherSystem.WeatherManager.Cycles.CurrentCycleName;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void	Update()
	{
		if (m_IsActive == false || m_IsCompletedInitialization == false || Player.Instance.IsNotNull() == false )
			return;

		// Only every 10 frames
		if ( Time.frameCount % 10 == 0 )
			return;

		m_Timetime.text = Time.timeScale.ToString();

	//	m_StaminaBarImage.fillAmount = Player.Instance.OxygenCurrentLevel / 100f;
	}
	
}
