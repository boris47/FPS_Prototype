
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour {

	private			Text			timeText		= null;
	private			Text			cycleText		= null;
	private			Text			healthText		= null;

	private			Text			wpnName			= null;
	private			Text			bulletsCount	= null;
	private			Text			otherInfo		= null;

	private			Image			staminaBar		= null;

	private			bool			m_IsActive		= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{

	}

	private void Start()
	{
		cycleText		= transform.GetChild(0).GetChild(0).GetComponent<Text>();
		timeText		= transform.GetChild(0).GetChild(1).GetComponent<Text>();
		healthText		= transform.GetChild(0).GetChild(2).GetComponent<Text>();

		wpnName			= transform.GetChild(1).GetChild(0).GetComponent<Text>();
		bulletsCount	= transform.GetChild(1).GetChild(1).GetComponent<Text>();
		otherInfo		= transform.GetChild(1).GetChild(2).GetComponent<Text>();
		staminaBar		= transform.GetChild(1).GetChild(3).GetChild(1).GetComponent<Image>();

		InvokeRepeating( "PrintTime", 1.0f, 1.0f );	
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

		m_IsActive = true;

		Show();
	}


	//////////////////////////////////////////////////////////////////////////
	// Show
	public	void	Show()
	{
		foreach( Transform t in transform )
		{
			t.gameObject.SetActive( true );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Hide
	public	void	Hide()
	{
		foreach( Transform t in transform )
		{
			t.gameObject.SetActive( false );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateUI
	public	void	UpdateUI()
	{
		if ( m_IsActive == false )
			return;

		IEntity player		= Player.Instance as IEntity;

		healthText.text		= Mathf.CeilToInt( player.Health ).ToString();

		wpnName.text		= WeaponManager.Instance.CurrentWeapon.Transform.name;
//		bulletsCount.text	= WeaponManager.Instance.CurrentWeapon.Magazine.ToString();
		otherInfo.text		= WeaponManager.Instance.CurrentWeapon.OtherInfo;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// PrintTime
	private	void	PrintTime()
	{
		if ( m_IsActive == false )
			return;

		if ( WeatherSystem.WeatherManager.Instance != null )
		{
			timeText.text	= WeatherSystem.WeatherManager.Cycles.GetTimeAsString();
			cycleText.text	= WeatherSystem.WeatherManager.Cycles.CurrentCycleName;
		}
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		if ( m_IsActive == false )
			return;

		// Only every 10 frames
		if ( Time.frameCount % 10 == 0 )
			return;

		staminaBar.fillAmount = Player.Instance.Stamina;
	}
	*/
}
