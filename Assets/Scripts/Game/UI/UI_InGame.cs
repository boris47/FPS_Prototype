
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour {

	private			Transform		m_Panel1					= null;
	private			Text			m_TimeText					= null;
	private			Text			m_CycleNameText				= null;
	private			Text			m_HealthText				= null;

	private			Transform		m_Panel2					= null;
	private			Text			m_WpnNameText				= null;
	private			Text			m_WpnOtherInfoText			= null;

	private			Image			m_StaminaBarImage			= null;
	private			Transform		m_CrosshairTransform		= null;

	private			Image			m_ZoomFrameImage			= null;
	private			float			m_FrameOrigWidth			= 0.0f;
	private			float			m_FrameOrigHeight			= 0.0f;

	private			bool			m_IsActive					= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{

	}

	private void Start()
	{
		bool result = true;

		m_Panel1				= transform.GetChild( 0 );
		{
			result &=	m_Panel1.SearchComponentInChild( 0, ref m_CycleNameText );
			result &=	m_Panel1.SearchComponentInChild( 1, ref m_TimeText);
			result &=	m_Panel1.SearchComponentInChild( 2, ref m_HealthText );
		}


		m_Panel2				= transform.GetChild( 1 );
		{
			result &=	m_Panel2.SearchComponentInChild( 0, ref m_WpnNameText );
			result &=	m_Panel2.SearchComponentInChild( 2, ref m_WpnOtherInfoText );
//			result &=	m_Panel2.SearchComponentInChild( 3, ref m_StaminaBarImage );
		}

		result &= transform.SearchComponentInChild( "UI_Frame", ref m_ZoomFrameImage );

		if ( result )
		{
			m_ZoomFrameImage.raycastTarget = false;
			m_CrosshairTransform	= transform.Find( "Crosshair" );

			InvokeRepeating( "PrintTime", 1.0f, 1.0f );	
		}
		else
		{
			Debug.Log( "UI_InGame: Bad initialization!!!" );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
#if UNITY_EDITOR
		if ( UnityEditor.EditorApplication.isPlaying && GameManager.IsChangingScene == false )
			m_IsActive = true;
#endif
		UI.Instance.EffectFrame.color = Color.clear;

		SoundManager.Instance.OnSceneLoaded();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private	void	OnDisable()
	{
		m_IsActive = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLevelWasLoaded
	private	void	OnLevelWasLoaded( int level )
	{
		if ( level == 0 ) // if returned at main menu using trigger ensure the switch to the main menu
		{
			UI.Instance.GoToMenu( UI.Instance.MainMenu.transform );
			return;
		}

		// if level is greater than 0 we suppose being in a level where ingame UI must be shown
		m_IsActive = true;

		Show();
	}


	//////////////////////////////////////////////////////////////////////////
	// Show
	public	void	Show()
	{
		m_Panel1.gameObject.SetActive( true );
		m_Panel2.gameObject.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// Hide
	public	void	Hide()
	{
		m_Panel1.gameObject.SetActive( false );
		m_Panel2.gameObject.SetActive( false );
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateUI
	public	void	UpdateUI()
	{
		if ( m_IsActive == false )
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
			m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, m_FrameOrigWidth = frame.rectTransform.rect.width );
			m_ZoomFrameImage.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, m_FrameOrigHeight = frame.rectTransform.rect.height );

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
			m_ZoomFrameImage.sprite	= null;
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
		/*
		if ( m_IsActive == false )
			return;

		// Only every 10 frames
		if ( Time.frameCount % 10 == 0 )
			return;

		staminaBar.fillAmount = Player.Instance.Stamina;
		*/
	}
	
}
