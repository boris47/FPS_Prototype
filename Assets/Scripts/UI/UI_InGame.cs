
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_InGame : MonoBehaviour, IStateDefiner {

	private			Transform		m_GenericInfosPanel			= null;
	private			Text			m_TimeText					= null;
	private			Text			m_CycleNameText				= null;
	private			Text			m_HealthText				= null;
	private			Text			m_Timetime					= null;

	private			Transform		m_WeaponInfosPanel				= null;
	private			Text			m_WpnNameText				= null;
	private			Text			m_WpnOtherInfoText			= null;

	private			Image			m_StaminaBarImage			= null;
	private			Transform		m_CrosshairTransform		= null;

	private			Image			m_ZoomFrameImage			= null;

	private			Canvas			m_Canvas					= null;


	private			bool			m_IsActive					= false;

	private			bool			m_IsCompletedInitialization	= false;
	private			bool			m_IsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return this.m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return this.name; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		this.m_IsInitialized = true;
		{
			this.m_IsInitialized &= this.transform.childCount > 1;

			this.m_IsInitialized &= this.transform.SearchComponent( ref this.m_Canvas, ESearchContext.LOCAL );

			yield return null;

			if (this.m_IsInitialized )
			{
				this.m_GenericInfosPanel = this.transform.Find( "GenericInfosPanel" );
				{
					this.m_IsInitialized &= this.m_GenericInfosPanel.SearchComponentInChild( 0, ref this.m_CycleNameText );
					this.m_IsInitialized &= this.m_GenericInfosPanel.SearchComponentInChild( 1, ref this.m_TimeText);
					this.m_IsInitialized &= this.m_GenericInfosPanel.SearchComponentInChild( 2, ref this.m_HealthText );
					this.m_IsInitialized &= this.m_GenericInfosPanel.SearchComponentInChild( 3, ref this.m_Timetime );
				}
			}

			yield return null;

			if (this.m_IsInitialized )
			{
				this.m_WeaponInfosPanel = this.transform.Find( "WeaponInfosPanel" );
				{
					this.m_IsInitialized &= this.m_WeaponInfosPanel.SearchComponentInChild( 0, ref this.m_WpnNameText );
					this.m_IsInitialized &= this.m_WeaponInfosPanel.SearchComponentInChild( 2, ref this.m_WpnOtherInfoText );
					this.m_IsInitialized &= this.m_WeaponInfosPanel.SearchComponentInChild( 3, ref this.m_StaminaBarImage );
				}
			}

			yield return null;

			this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_Frame", ref this.m_ZoomFrameImage );

			this.m_IsInitialized &= (this.m_CrosshairTransform = this.transform.Find( "Crosshair" )) != null;
			if (this.m_IsInitialized )
			{
				this.m_ZoomFrameImage.raycastTarget = false;

				UserSettings.VideoSettings.OnResolutionChanged += this.UI_Graphics_OnResolutionChanged;

				this.InvokeRepeating( "PrintTime", 1.0f, 1.0f );	

				CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );

				yield return null;

				this.m_IsCompletedInitialization = true;
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
		return this.m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		this.m_IsActive = true;

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
		this.m_ZoomFrameImage.enabled	= false;
		this.m_ZoomFrameImage.sprite		= null;
		this.m_ZoomFrameImage.color		= Color.clear;
		this.m_ZoomFrameImage.material	= null;
		this.ShowCrosshair();
		this.Show();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private	void	OnDisable()
	{
		this.m_IsActive = false;
	}

	//////////////////////////////////////////////////////////////////////////
	// Show
	public	void	Show()
	{
		this.m_GenericInfosPanel.gameObject.SetActive( true );
		this.m_WeaponInfosPanel.gameObject.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// Hide
	public	void	Hide()
	{
		this.m_GenericInfosPanel.gameObject.SetActive( false );
		this.m_WeaponInfosPanel.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateUI
	public	void	UpdateUI()
	{
		if (this.m_IsActive == false || this.m_IsCompletedInitialization == false )
			return;

		IEntity player				= Player.Instance as IEntity;

		this.m_HealthText.text			= Mathf.CeilToInt( player.Health ).ToString();

		this.m_WpnNameText.text			= WeaponManager.Instance.CurrentWeapon.Transform.name;
//		m_WpnOtherInfoText.text		= WeaponManager.Instance.CurrentWeapon.OtherInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	// ShowCrosshair
	public	void	ShowCrosshair()
	{
		this.m_CrosshairTransform.gameObject.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// HideCrosshair
	public	void	HideCrosshair()
	{
		this.m_CrosshairTransform.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// SetFrame
	public void SetFrame( Image frame )
	{
		if ( frame != null )
		{
			// Size
			this.m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, frame.rectTransform.rect.width  );
			this.m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical,   frame.rectTransform.rect.height );

			this.m_ZoomFrameImage.sprite		= frame.sprite;
			this.m_ZoomFrameImage.color		= frame.color;
			this.m_ZoomFrameImage.material	= frame.material;
			this.m_ZoomFrameImage.enabled	= true;
			this.HideCrosshair();
			this.Hide();
		}
		else
		{
			this.m_ZoomFrameImage.enabled	= false;
			this.m_ZoomFrameImage.sprite		= null;
			this.m_ZoomFrameImage.color		= Color.clear;
			this.m_ZoomFrameImage.material	= null;
			this.ShowCrosshair();
			this.Show();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// PrintTime
	public void FrameFeedBack( float feedback, Vector2 delta )
	{
		if (this.m_ZoomFrameImage.enabled == true )
		{
			this.m_ZoomFrameImage.rectTransform.localScale = Vector3.one * feedback;
			this.m_ZoomFrameImage.rectTransform.position = delta;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// PrintTime
	private	void	PrintTime()
	{
		if (this.m_IsActive == false || this.m_IsCompletedInitialization == false )
			return;

		if ( WeatherSystem.WeatherManager.Instance != null )
		{
			this.m_TimeText.text	= WeatherSystem.WeatherManager.Cycles.GetTimeAsString();
			this.m_CycleNameText.text	= WeatherSystem.WeatherManager.Cycles.CurrentCycleName;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		if (this.m_IsActive == false || this.m_IsCompletedInitialization == false || Player.Instance.IsNotNull() == false )
			return;

		// Only every 10 frames
		if ( Time.frameCount % 10 == 0 )
			return;

		this.m_Timetime.text = Time.timeScale.ToString();

		this.m_StaminaBarImage.fillAmount = Player.Instance.OxygenCurrentLevel / 100f;
	}
	
}
