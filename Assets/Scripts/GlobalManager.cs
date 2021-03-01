using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
class EditorInitializer
{
	static EditorInitializer ()
	{
		// Assets/Resources/Scriptables/WeatherCollection.asset
		string assetPath = System.IO.Path.Combine( WeatherSystem.WindowWeatherEditor.ASSETS_SCRIPTABLES_PATH, $"{WeatherSystem.WeatherManager.RESOURCES_WEATHERSCOLLECTION}.asset");
		if (System.IO.File.Exists(assetPath))
		{
			WeatherSystem.Weathers weathers = UnityEditor.AssetDatabase.LoadAssetAtPath<WeatherSystem.Weathers>( assetPath );
			UnityEngine.Assertions.Assert.IsNotNull
			(
				weathers,
				"Cannot preload weather cycles"
			);
			Debug.Log( "Weathers cycles preloaded!" );
		}

		UnityEditor.EditorApplication.playModeStateChanged += (UnityEditor.PlayModeStateChange currentState) =>
		{
			if (currentState == UnityEditor.PlayModeStateChange.EnteredEditMode)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		};
	}
	
/*	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void LoadLoadingScene()
	{
		if (!SceneManager.GetSceneByBuildIndex( (int)ESceneEnumeration.LOADING).isLoaded)
		{
			UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Loading.unity", UnityEditor.SceneManagement.OpenSceneMode.Additive);
		}
	}*/
}
#endif


public class GlobalManager : SingletonMonoBehaviour<GlobalManager>
{
#if UNITY_EDITOR
	private short updateCount = 0;
    private short fixedUpdateCount = 0;
    private short updateUpdateCountPerSecond;
    private short updateFixedUpdateCountPerSecond;
	private GUIStyle fontSize = null;

	private void Awake()
	{
		IEnumerator Loop()
		{
			while (true)
			{
				yield return new WaitForSecondsRealtime(1);
				updateUpdateCountPerSecond = updateCount;
				updateFixedUpdateCountPerSecond = fixedUpdateCount;

				updateCount = 0;
				fixedUpdateCount = 0;
			}
		}
		StartCoroutine(Loop());
	}

	private void FixedUpdate()
	{
		fixedUpdateCount += 1;
	}


	private void OnGUI()
	{
		fontSize = fontSize ?? new GUIStyle( GUI.skin.GetStyle( "label" ) )
		{
			fontSize = 10
		};
		GUI.Label(new Rect(20,  5, 200, 50), $"Update: {updateUpdateCountPerSecond}", fontSize );
        GUI.Label(new Rect(20, 15, 200, 50), $"FixedUpdate: {updateFixedUpdateCountPerSecond}", fontSize );
	}
#endif // UNITY_EDITOR

	private const			string				m_SettingsFilePath	= "Settings";
	private	const			string				m_ConfigsFilePath	= "Configs/All";


	private static			CustomLogHandler	m_LoggerInstance	= null;
	public	static			CustomLogHandler	LoggerInstance		{ get => m_LoggerInstance; }
	public	static			bool				bIsChangingScene	= false;
	public	static			bool				bIsLoadingScene		= false;
	public	static			bool				bCanSave			= true;


	private	static			SectionDB.LocalDB		m_Settings			= null;
	public	static			SectionDB.LocalDB		Settings
	{
		get {
			if ( m_Settings == null )
			{
				m_Settings = new SectionDB.LocalDB(m_SettingsFilePath);
			}
			return m_Settings;
		}
	}


	private	static			SectionDB.LocalDB		m_Configs			= null;
	public	static			SectionDB.LocalDB		Configs
	{
		get {
			if ( m_Configs == null )
			{
				m_Configs = new SectionDB.LocalDB(m_ConfigsFilePath);
			}
			return m_Configs;
		}
	}

	public static InputManager InputMgr = null;


	//////////////////////////////////////////////////////////////////////////
	private static void HandleException( string condition, string stackTrace, LogType type )
    {
		switch ( type )
		{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
			{
				QuitInstanly();
				break;
			}
		}
    }


	//////////////////////////////////////////////////////////////////////////
	protected override void OnInitialize()
	{
		InputMgr = new InputManager();
//		if ( Application.isEditor == false )
		{
			Application.logMessageReceived += HandleException;
		}

	//	if ( Application.isEditor == false )
		{
			m_LoggerInstance = new CustomLogHandler(!Application.isEditor);
		}

		// Whether physics queries should hit back-face triangles.
		Physics.queriesHitBackfaces = false;

		// Priority of background loading thread.
		Application.backgroundLoadingPriority = ThreadPriority.Low;

		// Async texture upload provides timesliced async texture upload on the render thread
		// with tight control over memory and timeslicing. There are no allocations except
		// for the ones which driver has to do. To read data and upload texture data a ringbuffer
		// whose size can be controlled is re-used. Use asyncUploadBufferSize to set the
		// buffer size for asynchronous texture uploads. The size is in megabytes. Minimum
		// value is 2 and maximum is 512. Although the buffer will resize automatically
		// to fit the largest texture currently loading, it is recommended to set the value
		// approximately to the size of biggest texture used in the Scene to avoid re-sizing
		// of the buffer which can incur performance cost.
		QualitySettings.asyncUploadBufferSize = 24; // MB

		// If not already loaded from previous scenes, ensure scene loaded for next sequence
		Scene loadingScene = SceneManager.GetSceneByBuildIndex( (int) ESceneEnumeration.LOADING );
		if (!loadingScene.isLoaded)
		{
			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = ESceneEnumeration.LOADING,
				eMode = LoadSceneMode.Additive
			};
			CustomSceneManager.LoadSceneSync( loadSceneData );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDestroy()
	{
		m_LoggerInstance?.Close();
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		SetCursorVisibility( bool newState )
	{
		Cursor.visible = newState;
		Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		SetTimeScale( float value )
	{
		SoundManager.Pitch = value;

		Time.timeScale = value;
	}


	//	float maximum = 1;
	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
#if UNITY_EDITOR
		updateCount++;
#endif
		/*
		if ( Input.GetKeyDown( KeyCode.V ) )
		{
			System.Diagnostics.Stopwatch m_StopWatch = new System.Diagnostics.Stopwatch();
			m_StopWatch.Start();
			for ( int i = 0; i < maximum; i++ )
			{
				SectionMap sectionMap = new SectionMap();
				sectionMap.LoadFile( configsPath ); 
			}
			m_StopWatch.Stop();

			print( "Performance test: operations done in " + m_StopWatch.Elapsed.Milliseconds + "ms, maximum iterations " + maximum );
			maximum *= 2f;
		}
		*/
		if ( Input.GetKeyDown( KeyCode.U ) )
		{
			SectionDB.LocalDB qwe = new SectionDB.LocalDB("Configs/All");

			SectionDB.GlobalDB.TryLoadFile("Configs/BuildSettings");
		}

		if (Input.GetKeyDown( KeyCode.L ))
		{
		//	string result = SaveSystem.GameObjectToJSON(Player.Instance.gameObject);
		//	GameObject clone = SaveSystem.JSONToGameObject(result);
		//	UnityEditor.EditorApplication.isPaused = true;
			foreach(var spawnPoint in Object.FindObjectsOfType<SpawnPoint>())
			{
				spawnPoint.Spawn();
			}
		}


		if ( Input.GetKeyDown( KeyCode.K ) )
		{
			SoundManager.Pitch = Time.timeScale = 0.02f;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		QuitInstanly()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}


	/*//////////////////////////////////////////////////////////////////////////
	public	static void		Assert( bool condition, string message )
	{
		if ( condition == false )
		{
			throw new System.Exception( message );
		}
	}
	*/

	//////////////////////////////////////////////////////////////////////////
	public	static	void	ForcedQuit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}


	public class CustomLogHandler : ILogHandler
	{
		public static ILogHandler m_DefaultLogHandler { get; private set; } = null;
		private readonly System.IO.FileStream m_FileStream = null;
		private readonly System.IO.StreamWriter m_StreamWriter = null;
		private readonly CultureInfo m_CultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
		private readonly System.Text.RegularExpressions.Regex m_Filter = new System.Text.RegularExpressions.Regex("get_StackTrace|CustomLogHandler|UnityEngine|UnityEditor");

		private bool bCanLog = true;
		private bool bExceptionsAsWarnings = false;

		public void Silence() => bCanLog = false;
		public void Talk() => bCanLog = true;
		public bool CanTalk() => bCanLog;
		public void SetExceptionsAsWarnings(bool value) => bExceptionsAsWarnings = value;

		//////////////////////////////////////////////////////////////////////////
		public CustomLogHandler(bool UseFileSystemWriter)
		{
			m_DefaultLogHandler = Debug.unityLogger.logHandler;
			Debug.unityLogger.logHandler = this;

			if (UseFileSystemWriter)
			{
				string filePath = System.IO.Path.Combine(Application.dataPath, "SessionLog.log");
				m_FileStream = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
				m_StreamWriter = new System.IO.StreamWriter(m_FileStream);
			}
			m_CultureInfo.NumberFormat.NumberDecimalSeparator = ".";
		}

		//////////////////////////////////////////////////////////////////////////
		private string GetContext()
		{
			string contextTitle = "";
			string line = System.Array.Find(System.Environment.StackTrace.Split('\n'), ln => !m_Filter.IsMatch(ln));
			if (!string.IsNullOrEmpty(line))
			{
				int start = line.IndexOf(' ', 2) + 1, end = line.IndexOf(' ', start);
				contextTitle = $"{line.Substring(start, end - start)}: ";
			}
			return contextTitle;
		}

		//////////////////////////////////////////////////////////////////////////
		public void LogFormat(LogType logType, Object context, string format, params object[] args)
		{
			if (bCanLog)
			{
				m_StreamWriter?.WriteLine($"[{Time.time.ToString("0.000", m_CultureInfo)}] {string.Format(format, args)}");
				if (bExceptionsAsWarnings && (logType == LogType.Exception || logType == LogType.Error))
				{
					m_DefaultLogHandler.LogFormat(LogType.Warning, context, $"{GetContext()}{format}", args);
				}
				else
				{
					m_DefaultLogHandler.LogFormat(logType, context, $"{GetContext()}{format}", args);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void LogException(System.Exception exception, Object context)
		{
			if (bCanLog)
			{
				m_StreamWriter?.WriteLine($"[{Time.time.ToString("0.000", m_CultureInfo)}] {exception.Message}");
				m_StreamWriter?.WriteLine(exception.StackTrace);
				m_StreamWriter?.Flush();
				if (bExceptionsAsWarnings)
				{
					m_DefaultLogHandler.LogFormat(LogType.Warning, context, "{0}", $"{exception.Message}");
				}
				else
				{
					m_DefaultLogHandler.LogException(exception, context);
				}
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void Close()
		{
			m_StreamWriter?.Flush();
			m_StreamWriter?.Close();
			m_FileStream?.Close();

			Debug.unityLogger.logHandler = m_DefaultLogHandler;
		}
	}

}
