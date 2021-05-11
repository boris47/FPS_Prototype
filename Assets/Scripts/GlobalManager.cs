using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameEvent : UnityEngine.Events.UnityEvent { }
[System.Serializable]
public class GameEventArg1 : UnityEngine.Events.UnityEvent<UnityEngine.GameObject> { }
[System.Serializable]
public class GameEventArg2 : UnityEngine.Events.UnityEvent<UnityEngine.GameObject, UnityEngine.GameObject> { }
[System.Serializable]
public class GameEventArg3 : UnityEngine.Events.UnityEvent<UnityEngine.GameObject, UnityEngine.GameObject, UnityEngine.GameObject> { }
[System.Serializable]
public class GameEventArg4 : UnityEngine.Events.UnityEvent<UnityEngine.GameObject, UnityEngine.GameObject, UnityEngine.GameObject, UnityEngine.GameObject> { }

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
class EditorInitializer
{
	static EditorInitializer ()
	{
		// Assets/Resources/Scriptables/WeatherCollection.asset
		/*	string assetPath = System.IO.Path.Combine(WeatherSystem.WindowWeatherEditor.ASSETS_SCRIPTABLES_PATH, $"{WeatherSystem.WeatherManager.RESOURCES_WEATHERSCOLLECTION}.asset");
			if (System.IO.File.Exists(assetPath))
			{
				WeatherSystem.Weathers weathers = UnityEditor.AssetDatabase.LoadAssetAtPath<WeatherSystem.Weathers>(assetPath);
				CustomAssertions.IsNotNull(weathers, "Cannot preload weather cycles");
				Debug.Log("Weathers cycles preloaded!");
			}
	*/
		static void EnableCursorInEditMode(UnityEditor.PlayModeStateChange currentState)
		{
			if (currentState == UnityEditor.PlayModeStateChange.EnteredEditMode)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
		}
		UnityEditor.EditorApplication.playModeStateChanged -= EnableCursorInEditMode;
		UnityEditor.EditorApplication.playModeStateChanged += EnableCursorInEditMode;
	}
}
#endif

public class GlobalManager : MonoBehaviourSingleton<GlobalManager>
{
	public partial class CustomLogger { } // Definition located at the bottom of this class

	public static bool IsQuittings { get; private set; } = false;

	private static			CustomLogger	m_LoggerInstance	= null;
	public	static			CustomLogger	LoggerInstance		=> m_LoggerInstance;
	static GlobalManager()
	{
		//m_LoggerInstance = new CustomLogger(!Application.isEditor);
	}

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
				yield return new WaitForSecondsRealtime(1f);
				updateUpdateCountPerSecond = updateCount;
				updateFixedUpdateCountPerSecond = fixedUpdateCount;

				updateCount = 0;
				fixedUpdateCount = 0;
			}
		}
		StartCoroutine(Loop());

		IsQuittings = false;
	}

	private void FixedUpdate()
	{
		fixedUpdateCount += 1;
	}


	private void OnGUI()
	{
		fontSize = fontSize ?? new GUIStyle(GUI.skin.label)
		{
			fontSize = 10
		};
		GUI.Label(new Rect(20,  5, 200, 50), $"Update: {updateUpdateCountPerSecond}", fontSize );
        GUI.Label(new Rect(20, 15, 200, 50), $"FixedUpdate: {updateFixedUpdateCountPerSecond}", fontSize );
	}
#endif // UNITY_EDITOR

	private const			string				m_SettingsFilePath	= "Settings";
	private	const			string				m_ConfigsFilePath	= "Configs/All";

	public	static			bool				bIsChangingScene	= false;
	public	static			bool				bIsLoadingScene		= false;
	public	static			bool				bCanSave			= true;


	private	static			SectionDB.LocalDB		m_Settings			= null;
	public	static			SectionDB.LocalDB		Settings
	{
		get
		{
			if (m_Settings == null)
			{
				m_Settings = new SectionDB.LocalDB(m_SettingsFilePath);
			}
			return m_Settings;
		}
	}


	private	static			SectionDB.LocalDB		m_Configs			= null;
	public	static			SectionDB.LocalDB		Configs
	{
		get
		{
			if (m_Configs == null)
			{
				m_Configs = new SectionDB.LocalDB(m_ConfigsFilePath);
			}
			return m_Configs;
		}
	}

	public static InputManager InputMgr = null;

	private					bool			m_SkipOneFrame			= true;


	//////////////////////////////////////////////////////////////////////////
	private static void HandleException(string condition, string stackTrace, LogType type)
	{
		switch (type)
		{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
			{
				Debug.LogError($"Error({type}): {condition}\n{stackTrace}");
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

	}


	//////////////////////////////////////////////////////////////////////////
	protected /*override*/ void OnDestroy()
	{
		m_LoggerInstance?.Close();

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}


	//////////////////////////////////////////////////////////////////////////
	public static void SetCursorVisibility(bool bVisible)
	{
		//if (Cursor.visible != bVisible)
		{
			//Cursor.visible = bVisible;
			//Cursor.lockState = bVisible ? CursorLockMode.None : CursorLockMode.Locked;


			IEnumerator SetCursorVisibility()
			{
				Cursor.visible = bVisible;
				Cursor.lockState = bVisible ? CursorLockMode.None : CursorLockMode.Locked;

				yield return null;
				Cursor.visible = bVisible;
				Cursor.lockState = bVisible ? CursorLockMode.None : CursorLockMode.Locked;

				yield return null;
				Cursor.visible = bVisible;
				Cursor.lockState = bVisible ? CursorLockMode.None : CursorLockMode.Locked;
			}
			m_Instance.StartCoroutine(SetCursorVisibility());
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public static void SetTimeScale(float value)
	{
		SoundManager.Pitch = value;

		Time.timeScale = value;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> This prevent the ui interaction can trigger actions in-game </summary>
	public void RequireFrameSkip()
	{
		m_SkipOneFrame = true;
	}


	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
#if UNITY_EDITOR
		updateCount++;
#endif
		// This prevent the ui interaction can trigger actions in-game
		if (m_SkipOneFrame)
		{
			m_SkipOneFrame = false;
			return;
		}

		GlobalManager.InputMgr.Update();
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		QuitInstanly()
	{
		IsQuittings = true;

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public partial class CustomLogger : ILogHandler
	{
		public static ILogHandler m_DefaultLogHandler { get; private set; } = null;
		private readonly System.IO.FileStream m_FileStream = null;
		private readonly System.IO.StreamWriter m_StreamWriter = null;
		private readonly CultureInfo m_CultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();
		private readonly System.Text.RegularExpressions.Regex m_Filter = new System.Text.RegularExpressions.Regex("get_StackTrace|CustomLogger|UnityEngine|UnityEditor");

		private bool bCanLog = true;
		private bool bExceptionsAsWarnings = false;

		public void Silence() => bCanLog = false;
		public void Talk() => bCanLog = true;
		public bool CanTalk() => bCanLog;
		public void SetExceptionsAsWarnings(bool value) => bExceptionsAsWarnings = value;

		//////////////////////////////////////////////////////////////////////////
		public CustomLogger(bool UseFileSystemWriter)
		{
			Debug.Log($"Using custom logger with {nameof(UseFileSystemWriter)} set as {UseFileSystemWriter}");
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
