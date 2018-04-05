
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour {

	public	static UI_InGame	Instance		= null;

	private			Text		timeText		= null;
	private			Text		cycleText		= null;
	private			Text		healthText		= null;

	private			Text		wpnName			= null;
	private			Text		bulletsCount	= null;
	private			Text		fireMode		= null;

	private			Image		staminaBar		= null;

	private			Image		m_EffectFrame	= null;


	private void Awake()
	{
		Instance = this;

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


	public	Image	GetEffectFrame()
	{
		return m_EffectFrame;
	}


	public	void	UpdateUI()
	{
		IEntity player		= Player.Instance as IEntity;

		healthText.text		= Mathf.CeilToInt( player.Health ).ToString();

		wpnName.text		= Player.Instance.CurrentWeapon.Transform.name;
		bulletsCount.text	= Player.Instance.CurrentWeapon.Magazine.ToString();
		fireMode.text		= Player.Instance.CurrentWeapon.FireMode.ToString();
	}

	private	void	PrintTime()
	{
		timeText.text	= WeatherSystem.WeatherManager.Instance.GetTimeAsString( -1f );
		cycleText.text	= WeatherSystem.WeatherManager.Instance.CurrentCycleName;
	}

	private void Update()
	{
		staminaBar.fillAmount = Player.Instance.Stamina;
	}

}
