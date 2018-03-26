
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour {

	public	static UI_InGame	Instance	= null;

	private	Text timeText = null;
	private	Text cycleText = null;
	private	Text healthText = null;

	private	Text wpnName = null;
	private	Text bulletsCount = null;
	private	Text fireMode = null;


	private void Awake()
	{
		Instance = this;

		cycleText		= transform.GetChild(0).GetChild(0).GetComponent<Text>();
		timeText		= transform.GetChild(0).GetChild(1).GetComponent<Text>();
		healthText		= transform.GetChild(0).GetChild(2).GetComponent<Text>();

		wpnName			= transform.GetChild(1).GetChild(0).GetComponent<Text>();
		bulletsCount	= transform.GetChild(1).GetChild(1).GetComponent<Text>();
		fireMode		= transform.GetChild(1).GetChild(2).GetComponent<Text>();

		InvokeRepeating( "PrintTime", 0.3f, 0.2f );
	}


	public	void	UpdateUI()
	{
		Player player = Player.Instance;

		healthText.text		= Mathf.CeilToInt( player.Health ).ToString();

		wpnName.text		= player.CurrentWeapon.name;
		bulletsCount.text	= player.CurrentWeapon.magazine.ToString();
		fireMode.text		= player.CurrentWeapon.fireMode.ToString();


	}

	private	void	PrintTime()
	{
		timeText.text	= WeatherSystem.WeatherManager.Instance.GetTimeAsString( -1f );
		cycleText.text	= WeatherSystem.WeatherManager.Instance.CurrentCycleName;
	}


}
