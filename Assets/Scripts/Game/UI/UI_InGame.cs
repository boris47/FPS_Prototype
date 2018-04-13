
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour {

	private			Text			timeText		= null;
	private			Text			cycleText		= null;
	private			Text			healthText		= null;

	private			Text			wpnName			= null;
	private			Text			bulletsCount	= null;
	private			Text			fireMode		= null;

	private			Image			staminaBar		= null;

	private			Image			m_EffectFrame	= null;

	private			bool			m_IsActive		= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void	Awake()
	{
		cycleText		= transform.GetChild(0).GetChild(0).GetComponent<Text>();
		timeText		= transform.GetChild(0).GetChild(1).GetComponent<Text>();
		healthText		= transform.GetChild(0).GetChild(2).GetComponent<Text>();

		wpnName			= transform.GetChild(1).GetChild(0).GetComponent<Text>();
		bulletsCount	= transform.GetChild(1).GetChild(1).GetComponent<Text>();
		fireMode		= transform.GetChild(1).GetChild(2).GetComponent<Text>();
		staminaBar		= transform.GetChild(1).GetChild(3).GetChild(1).GetComponent<Image>();

		m_EffectFrame	= transform.GetChild(2).GetComponent<Image>();

		InvokeRepeating( "PrintTime", 0.3f, 0.2f );
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
#if UNITY_EDITOR
		if ( UnityEditor.EditorApplication.isPlaying && GameManager.IsChangingScene == false )
			m_IsActive = true;
#endif
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
	// GetEffectFrame
	public	Image	GetEffectFrame()
	{
		return m_EffectFrame;
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
		bulletsCount.text	= WeaponManager.Instance.CurrentWeapon.Magazine.ToString();
		fireMode.text		= WeaponManager.Instance.CurrentWeapon.FireMode.ToString();
	}
	

	//////////////////////////////////////////////////////////////////////////
	// PrintTime
	private	void	PrintTime()
	{
		if ( m_IsActive == false )
			return;

		timeText.text	= WeatherSystem.WeatherManager.Instance.GetTimeAsString( -1f );
		cycleText.text	= WeatherSystem.WeatherManager.Instance.CurrentCycleName;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void	Update()
	{
		if ( m_IsActive == false )
			return;

		staminaBar.fillAmount = Player.Instance.Stamina;
	}

}
