
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour {

	private	Text timeText = null;
	private	Text cycleText = null;
	private	Text healthText = null;

	private void Start()
	{

		cycleText	= transform.GetChild(0).GetChild(0).GetComponent<Text>();
		timeText	= transform.GetChild(0).GetChild(1).GetComponent<Text>();
		healthText	= transform.GetChild(0).GetChild(2).GetComponent<Text>();
		InvokeRepeating( "PrintTime", 0.3f, 0.2f );
	}

	private	void	PrintTime()
	{
		timeText.text	= WeatherSystem.WeatherManager.Instance.GetTimeAsString( -1f );
		cycleText.text	= WeatherSystem.WeatherManager.Instance.CurrentCycleName;
		healthText.text	= Player.Instance.Health.ToString();
	}


}
