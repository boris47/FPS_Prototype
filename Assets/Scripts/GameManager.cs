
using CFG_Reader;
using UnityEngine;

[System.Serializable]
public class GameEvent      : UnityEngine.Events.UnityEvent { }

public	delegate	void	OnTriggerCall( Collider collider );



public partial class GameManager : MonoBehaviour {

    /// <summary> Use this to identity is executing in editor or in build </summary>
#if UNITY_EDITOR
    public	const	bool InEditor = true;
#else
	public	const	bool InEditor = false;
#endif

	public	static	Reader			Settings				= null;

	public	static	Reader			Configs					= null;

	public	static	InputManager	InputMgr				= null;

	public	static	GameManager		Instance				= null;

	public	static	bool			IsChangingScene			= false;

	public	static	bool			CanSave					= true;

	public	static	bool			IsPaused				= false;

	[SerializeField]
	private	bool					m_HideCursor			= true;


	private	bool					m_QuitRequest			= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private	void	Awake ()
	{
		// SINGLETON
		if ( Instance != null )
		{
			print( "WeaponManager: Object set inactive" );
			gameObject.SetActive( false );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		// Internal classes
		InputMgr = new InputManager();
		Settings = new Reader();
		Configs = new Reader();
#if UNITY_EDITOR
		Settings.LoadFile( "Assets/Resources/Settings.txt" );
#else
		Settings.LoadFile( "Settings" );
#endif

#if UNITY_EDITOR
		Configs.LoadFile( "Assets/Resources/Configs/All.txt" );
#else
		Configs.LoadFile( "Configs\\All" );
#endif

		if ( m_HideCursor )
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private	void	Update()
	{
		// Save
		if ( Input.GetKeyDown( KeyCode.F5 ) && CanSave == true )
		{
			Save();
		}

		// Load
		if ( Input.GetKeyDown( KeyCode.F9 ) )
		{
			Load();
		}

		// Update inputs
		InputMgr.Update();

		// APPLICATION QUIT REQUEST
		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
			IsPaused = !IsPaused;
			UI.Instance.TooglePauseMenu();

			Cursor.visible = IsPaused;
			Cursor.lockState = IsPaused == true ? CursorLockMode.None : CursorLockMode.Locked;
			/*
			m_QuitRequest = true;
			print( "Quit request" );
			if ( m_SaveLoadCO != null )
			{
				print( "Wait End Encryption" );
			}
			*/
		}


		if ( m_QuitRequest == true )
		{
			if ( m_SaveLoadCO == null )
			{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
			}
		}
	}

}
