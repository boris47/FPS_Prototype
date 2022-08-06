
using UnityEngine;


public sealed partial class GameManager : GlobalMonoBehaviourSingleton<GameManager>
{
	public	const			float			THINK_TIMER				= 0.2f; // 200 ms
	private	static			bool			m_QuitRequest			= false;

	private					float			m_ThinkTimer			= 0f;

	protected override void OnInitialize()
	{
		base.OnInitialize();

		Application.logMessageReceived += Application_logMessageReceived;
	}

	private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
	{
		if (System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debug.WriteLine($"{condition}\n{stackTrace}");
		}
	}



	//////////////////////////////////////////////////////////////////////////
	private	void	ResetEvents()
	{
		ResetUpdateEvents();

	///	ResetSaveAndLoadEvens();

		ResetPauseEvents();
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		ResetEvents();

		// Reset Internals
		m_QuitRequest			= false;
		m_ThinkTimer			= 0f;
		m_IsPaused				= false;
	}
}
