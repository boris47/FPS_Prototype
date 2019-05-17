
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

	private			UI_Minimap		m_UI_Minimap				= null;

	private			Image			m_ZoomFrameImage			= null;
	private			float			m_FrameOrigWidth			= 0.0f;
	private			float			m_FrameOrigHeight			= 0.0f;

	private			bool			m_IsActive					= false;

	private			bool			m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	public	UI_Minimap	UI_Minimap
	{
		get { return m_UI_Minimap; }
	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized )
			return true;

		m_bIsInitialized = true;
		{
			m_bIsInitialized &= transform.childCount > 1;

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

				InvokeRepeating( "PrintTime", 1.0f, 1.0f );	
			}
			else
			{
				Debug.LogError( "UI_InGame: Bad initialization!!!" );
			}
		}
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	bool IStateDefiner.ReInit()
	{
		return m_bIsInitialized;
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

		bool result = transform.SearchComponentInChild( "Minimap", ref m_UI_Minimap );

		UI.Instance.EffectFrame.color = Color.clear;

		SoundManager.Instance.OnSceneLoaded();

		// Reset Ingame UI
		InternalReset();
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
//		if ( m_IsActive == false )
//			return;

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


/*
 * 
 * private void DrawTargets()
    {
        Vector3 pos, campos;
        Texture2D tex;
        float size;
        Rect rect = new Rect(0,0,0,0);
 
        foreach (Pawn pawn in m_Targets)
        {
            pos = camera.WorldToScreenPoint(pawn.transform.position);
            campos = pawn.transform.position - camera.transform.position;
 
            size = Mathf.Clamp(10000 / (pos.z + 1), 8, 32);
 
            tex = targetsTex;
 
            if (size <= 16) tex = targetsTex2;
 
 
            if (Vector3.Dot(camera.transform.TransformDirection(Vector3.forward), campos) > 0)
            {
                if (pos.x <= 0) tex = pawn.m_TargetLeft;
                else if (pos.x >= camera.pixelWidth) tex = pawn.m_TargetRight;
                else if (pos.y <= 0) tex = pawn.m_TargetDown;
                else if (pos.y >= camera.pixelHeight) tex = pawn.m_TargetUp;
 
                rect.x = Mathf.Clamp(pos.x, 0, camera.pixelWidth) - size / 2;
                rect.y = camera.pixelHeight - (Mathf.Clamp(pos.y, 0, camera.pixelHeight) + size / 2);
            }
            else
            {
                if (pos.x <= camera.pixelWidth / 2)
                {
                    tex = pawn.m_TargetLeft;
                    rect.x = 0;
                    rect.y = camera.pixelHeight - (Mathf.Clamp(pos.y, 0, camera.pixelHeight) + size / 2);
                }
                else
                {
                    tex = pawn.m_TargetRight;
                    rect.x = 1;
                    rect.y = camera.pixelHeight - (Mathf.Clamp(pos.y, 0, camera.pixelHeight) + size / 2);
                }
 
                if (pos.y <= camera.pixelHeight / 2)
                {
                    tex = pawn.m_TargetDown;
                    rect.y = 0;
                    rect.x = Mathf.Clamp(pos.x, 0, camera.pixelWidth) - size / 2;
                }
                else
                {
                    tex = pawn.m_TargetUp;
                    rect.y = 1;
                    rect.x = Mathf.Clamp(pos.x, 0, camera.pixelWidth) - size / 2;
                }
 
            }
 
            rect.width = rect.height = size;
 
            GUI.DrawTexture(rect, tex);
        }
    }
 * 
 * 
 */

	private void FixedUpdate()
	{
//		Transform taretTransform = FindObjectOfType<NonLiveEntity>().transform;

		


		/*
		if (ViewportPoint.x > 0.5F)
            print("target is on the right side!");
        else
            print("target is on the left side!");
		*/

		/*

		System.Func<Vector3, Vector3> Vector3Maxamize = delegate(Vector3 vector)
		{
			Vector3 returnVector = vector;
			float max = 0;
			max = vector.x > max ? vector.x : max;
			max = vector.y > max ? vector.y : max;
			max = vector.z > max ? vector.z : max;
			returnVector /= max;
			return returnVector;
		};


		bool bIsOutOfScreen = ViewportPoint.x > 1f || ViewportPoint.y > 1f || ViewportPoint.x < 0f || ViewportPoint.y < 0f;
		if (ViewportPoint.z < 0)
        {
			ViewportPoint.x = 1f - ViewportPoint.x;
			ViewportPoint.y = 1f - ViewportPoint.y;
			ViewportPoint.z = 0;
			ViewportPoint = Vector3Maxamize(ViewportPoint);
        }

		if ( bIsOutOfScreen )
		{
			Vector3 targetPosLocal = camera.transform.InverseTransformPoint(taretTransform.position);
			float targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg - 90;

			m_CrosshairTransform.eulerAngles = new Vector3(0, 0, targetAngle);
		}

		ViewportPoint = camera.ViewportToScreenPoint(ViewportPoint);
        ViewportPoint.x = Mathf.Clamp(ViewportPoint.x, 5f, Screen.width - 5f);
        ViewportPoint.y = Mathf.Clamp(ViewportPoint.y, 5f, Screen.height - 5f);
		
		m_CrosshairTransform.position = ViewportPoint;
		*/
		
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		if ( m_IsActive == false )
			return;

		// Only every 10 frames
		if ( Time.frameCount % 10 == 0 )
			return;

		m_Timetime.text = Time.timeScale.ToString();
//		staminaBar.fillAmount = Player.Instance.Stamina;
		
	}
	
}
