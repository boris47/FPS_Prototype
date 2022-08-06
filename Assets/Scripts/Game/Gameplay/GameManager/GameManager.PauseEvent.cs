
using UnityEngine;



//	DELEGATES FOR EVENTS
public partial struct GameEvents
{
	// PAUSE
	public delegate void OnPauseEvent(bool isPaused);
}




//////////////////////////////////////////////////////////////////
//							PAUSE								//
//////////////////////////////////////////////////////////////////

public interface IPauseEvents
{
	/// <summary> Events called when game is setting on pause </summary>
	event GameEvents.OnPauseEvent OnBeforePause;

	event GameEvents.OnPauseEvent OnAfterPause;

	void SetPauseState(bool bPauseState);
	bool IsPaused { get; }
}

// PAUSE IMPLEMENTATION
public sealed partial class GameManager : IPauseEvents
{
	private event GameEvents.OnPauseEvent m_OnBeforePause = delegate { };

	private event GameEvents.OnPauseEvent m_OnAfterPause = delegate { };


	public static IPauseEvents PauseEvents => m_Instance;

	/// <summary> Events called when game is setting on pause </summary>
	event GameEvents.OnPauseEvent IPauseEvents.OnBeforePause
	{
		add { if (value.IsNotNull()) m_OnBeforePause += value; }
		remove { if (value.IsNotNull()) m_OnBeforePause -= value; }
	}

	/// <summary> Events called when game is setting on pause </summary>
	event GameEvents.OnPauseEvent IPauseEvents.OnAfterPause
	{
		add { if (value.IsNotNull()) m_OnAfterPause += value; }
		remove { if (value.IsNotNull()) m_OnAfterPause -= value; }
	}

	// Vars
	private bool m_IsPaused = false;
	public bool IsPaused => m_IsPaused;

	private float m_PrevTimeScale = 1f;


	//////////////////////////////////////////////////////////////////////////
	void IPauseEvents.SetPauseState(bool bIsPauseRequested)
	{
		if (bIsPauseRequested != m_IsPaused)
		{
			m_IsPaused = bIsPauseRequested;
			m_OnBeforePause(m_IsPaused);

			SoundManager.UpdatePauseState(bIsPauseRequested);

			if (bIsPauseRequested)
			{
				m_PrevTimeScale = Time.timeScale;
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = m_PrevTimeScale;
			}

			m_OnAfterPause(m_IsPaused);
		}
	}

	private void ResetPauseEvents()
	{
		// PauseEvents
		m_OnBeforePause = delegate { };
		m_OnAfterPause = delegate { };
		m_IsPaused = false;
		m_PrevTimeScale = 1f;
	}
}
