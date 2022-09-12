
using UnityEngine;


[Configurable(nameof(m_Configs), typeof(GameManager))]
public sealed partial class GameManager : GlobalMonoBehaviourSingleton<GameManager>
{
	private	static			bool			m_QuitRequest			= false;

	private					float			m_ThinkTimer			= 0f;

	[SerializeField, ReadOnly]
	private GameManagerConfiguration m_Configs = null;


	protected override void OnInitialize()
	{
		base.OnInitialize();

		Application.logMessageReceived += Application_logMessageReceived;

		Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs));
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
