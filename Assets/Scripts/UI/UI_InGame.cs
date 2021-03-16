
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_InGame : UI_Base, IStateDefiner
{
	private				Transform			m_GenericInfosPanel					= null;
	private				Text				m_TimeText							= null;
	private				Text				m_CycleNameText						= null;
	private				Text				m_HealthText						= null;
	private				Text				m_Timetime							= null;

	private				Transform			m_WeaponInfosPanel					= null;
	private				Text				m_WpnNameText						= null;
	private				Text				m_WpnOtherInfoText					= null;
	
	private				Image				m_StaminaBarImage					= null;
	private				Transform			m_CrosshairsTransform				= null;

	private				Image				m_ZoomFrameImage					= null;

	private				Canvas				m_Canvas							= null;

	private				bool				m_IsInitialized						= false;
						bool				IStateDefiner.IsInitialized			=> m_IsInitialized;


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			CustomAssertions.IsTrue(transform.TryGetComponent(out m_Canvas));

			if(CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("UI_Frame", out m_ZoomFrameImage)))
			{
				m_ZoomFrameImage.raycastTarget = false;
			}

			if(CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("GenericInfosPanel", out m_GenericInfosPanel)))
			{
				CustomAssertions.IsTrue(m_GenericInfosPanel.TrySearchComponentByChildIndex(0, out m_CycleNameText));
				CustomAssertions.IsTrue(m_GenericInfosPanel.TrySearchComponentByChildIndex(1, out m_TimeText));
				CustomAssertions.IsTrue(m_GenericInfosPanel.TrySearchComponentByChildIndex(2, out m_HealthText));
				CustomAssertions.IsTrue(m_GenericInfosPanel.TrySearchComponentByChildIndex(3, out m_Timetime));
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("WeaponInfosPanel", out m_WeaponInfosPanel)))
			{
				CustomAssertions.IsTrue(m_WeaponInfosPanel.TrySearchComponentByChildIndex(0, out m_WpnNameText));

				CustomAssertions.IsTrue(m_WeaponInfosPanel.TrySearchComponentByChildIndex(2, out m_WpnOtherInfoText));
				CustomAssertions.IsTrue(m_WeaponInfosPanel.TrySearchComponentByChildIndex(3, out m_StaminaBarImage));		// TODO Find a valid place for this
			}

			CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Crosshairs", out m_CrosshairsTransform));

			UserSettings.VideoSettings.OnResolutionChanged += UI_Graphics_OnResolutionChanged;

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void UI_Graphics_OnResolutionChanged(float newWidth, float newHeight)
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	void	IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		GlobalManager.SetCursorVisibility(false);

		InvokeRepeating("PrintTime", 1.0f, 1.0f);

		// TODO Think better solution instead of this
		UIManager.Minimap.SetTarget(FPSEntityCamera.Instance?.transform);
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnDisable()
	{
		CancelInvoke("PrintTime");
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Show()
	{
		m_GenericInfosPanel.gameObject.SetActive(true);
		m_WeaponInfosPanel.gameObject.SetActive(true);
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Hide()
	{
		m_GenericInfosPanel.gameObject.SetActive(false);
		m_WeaponInfosPanel.gameObject.SetActive(false);
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	UpdateUI()
	{
		CustomAssertions.IsTrue(m_IsInitialized);
		CustomAssertions.IsNotNull(Player.Instance);

		Entity player				= Player.Instance;
		m_HealthText.text			= Mathf.CeilToInt(player.Health).ToString();
		m_WpnNameText.text			= WeaponManager.Instance.CurrentWeapon.Transform.name;
		m_WpnOtherInfoText.text		= WeaponManager.Instance.CurrentWeapon.OtherInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	ShowCrosshairs()
	{
		m_CrosshairsTransform.gameObject.SetActive(true);
	}


	//////////////////////////////////////////////////////////////////////////
	public UI_BaseCrosshair EnableCrosshair(System.Type crosshairType)
	{
		UI_BaseCrosshair crosshair = m_CrosshairsTransform.GetComponentInChildren(crosshairType, includeInactive: true) as UI_BaseCrosshair;
		CustomAssertions.IsNotNull(crosshair);
		crosshair.AddRef();
		return crosshair;
	}


	//////////////////////////////////////////////////////////////////////////
	public void RemoveCrosshair(UI_BaseCrosshair crosshair)
	{
		crosshair.RemoveRef();
	}


	//////////////////////////////////////////////////////////////////////////
	public void HideCrosshairs()
	{
		m_CrosshairsTransform.gameObject.SetActive(false);
	}


	//////////////////////////////////////////////////////////////////////////
	public void SetFrame(Image frame)
	{
		if (frame.IsNotNull())
		{
			// Size
			m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, frame.rectTransform.rect.width  );
			m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical,   frame.rectTransform.rect.height );

			m_ZoomFrameImage.sprite		= frame.sprite;
			m_ZoomFrameImage.color		= frame.color;
			m_ZoomFrameImage.material	= frame.material;
			m_ZoomFrameImage.enabled	= true;
			HideCrosshairs();
			Hide();
		}
		else
		{
			m_ZoomFrameImage.enabled	= false;
			m_ZoomFrameImage.sprite		= null;
			m_ZoomFrameImage.color		= Color.clear;
			m_ZoomFrameImage.material	= null;
			ShowCrosshairs();
			Show();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void FrameFeedBack( float feedback, Vector2 delta )
	{
		if (m_ZoomFrameImage.enabled)
		{
			m_ZoomFrameImage.rectTransform.localScale = Vector3.one * feedback;
			m_ZoomFrameImage.rectTransform.position = delta;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	PrintTime()
	{
		CustomAssertions.IsNotNull(WeatherSystem.WeatherManager.Instance);

		m_TimeText.text			= WeatherSystem.WeatherManager.Cycles.GetTimeAsString();
		m_CycleNameText.text	= WeatherSystem.WeatherManager.Cycles.CurrentCycleName;
	}


	//////////////////////////////////////////////////////////////////////////
	private void	Update()
	{
		// Only every 10 frames
		if ( Time.frameCount % 10 == 0 )
			return;

		m_Timetime.text = Time.timeScale.ToString();
	}
	
}
