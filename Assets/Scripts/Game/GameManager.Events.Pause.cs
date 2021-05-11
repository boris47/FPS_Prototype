
using UnityEngine;



//	DELEGATES FOR EVENTS
public partial struct GameEvents
{
	// PAUSE
	public	delegate	void		OnPauseSetEvent(bool isPaused);				// PauseEvents.OnPauseSet
}




//////////////////////////////////////////////////////////////////
//							PAUSE								//
//////////////////////////////////////////////////////////////////

public interface IPauseEvents
{
	/// <summary> Events called when game is setting on pause </summary>
		event		GameEvents.OnPauseSetEvent		OnPauseSet;

					void							SetPauseState( bool bPauseState );
					bool							IsPaused{ get; }
}

// PAUSE IMPLEMENTATION
public partial class GameManager : IPauseEvents
{
	private event			GameEvents.OnPauseSetEvent		m_OnPauseSet			= delegate { };

	public	static			IPauseEvents					PauseEvents				=> m_Instance;

	/// <summary> Events called when game is setting on pause </summary>
	event GameEvents.OnPauseSetEvent IPauseEvents.OnPauseSet
	{
		add		{ if (value.IsNotNull())	m_OnPauseSet += value; }
		remove	{ if (value.IsNotNull())	m_OnPauseSet -= value; }
	}

	// Vars
	private					bool				m_IsPaused				= false;
	public					bool				IsPaused				=> m_IsPaused;

	private					float				m_PrevTimeScale			= 1f;
	private					bool				m_PrevCanParseInput		= false;
	private					bool				m_PrevInputEnabled		= false;


	//////////////////////////////////////////////////////////////////////////
	void	IPauseEvents.SetPauseState(bool bIsPauseRequested)
	{
		if (bIsPauseRequested != m_IsPaused)
		{
			m_IsPaused = bIsPauseRequested;
			m_OnPauseSet(m_IsPaused);

		//	GlobalManager.SetCursorVisibility(bIsPauseRequested);
			SoundManager.IsPaused = bIsPauseRequested;
		
			if (bIsPauseRequested)
			{
				m_PrevTimeScale					= Time.timeScale;
				m_PrevCanParseInput				= GlobalManager.InputMgr.HasCategoryEnabled(EInputCategory.CAMERA);
				m_PrevInputEnabled				= InputManager.IsEnabled;

				InputManager.IsEnabled			= false;
				Time.timeScale					= 0f;
				GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, false);

				UIManager.Instance.GoToMenu(UIManager.PauseMenu);
			}
			else
			{
				GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, m_PrevCanParseInput);
				Time.timeScale					= m_PrevTimeScale;
				InputManager.IsEnabled			= m_PrevInputEnabled;

				UIManager.Instance.GoToMenu(UIManager.InGame);
			}
			GlobalManager.Instance.RequireFrameSkip();
		}
	}

	private void ResetPauseEvents()
	{
		// PauseEvents
		m_OnPauseSet = delegate { };
		m_IsPaused = false;
		m_PrevTimeScale = 1f;
		m_PrevCanParseInput = false;
		m_PrevInputEnabled = false;
	}
}
