
using UnityEngine;

namespace WeatherSystem
{
	public partial interface IWeatherManager_Cycles
	{
		void			SetWeatherByName					(string weatherName);
		void			SetTime								(float DayTime);
		void			SetTime								(float H, float M, float S);
		void			SetTime								(string sTime);
		string			GetTimeAsString						();
		void			SetSkyColor							(Color color);
		Color			GetSkyColor							();
		void			SetSkyExposure						(float newValue);
		float			GetSkyExposure						();

		string			CurrentCycleName					{ get; }
	}

	// CLASS
	public sealed partial class WeatherManager : MonoBehaviour, ISingleton, IWeatherManager_Cycles
	{
		private	static	WeatherManager			m_Instance						= null;
		public	static	WeatherManager			Instance						=> m_Instance;
	#region VARS
		private	static	IWeatherManager_Cycles	m_Instance_Cycles				= null;
		public	static	IWeatherManager_Cycles	Cycles							=> m_Instance_Cycles;


		// CONST
		public	const	float					DAY_LENGTH						= 86400f;
		public	const	string					RESOURCES_WEATHERSCOLLECTION	= "WeatherCollection";
		private	const	string					RESOURCES_SKYMIXER_MAT			= "Materials/SkyMixer";

		// SERIALIZED
		[Header("Main")]

		[SerializeField, ReadOnly]
		private		string						m_CurrentDayTime				= string.Empty;

		[Header("Weather Info")]

		[SerializeField, Range(1f, 500f)]
		private		float						m_TimeFactor					= 1.0f;

		[SerializeField, ReadOnly]
		private		float						m_WeatherChoiceFactor			= 1.0f;

		[Header( "Cycles" )]

		[SerializeField, ReadOnly]
		private		Material					m_SkyMaterial					= null;

		[SerializeField]
		private		Weathers					m_Cycles						= null;

		[SerializeField, ReadOnly]
		private		WeatherCycle				m_CurrentCycle					= null;

		[SerializeField, ReadOnly]
		private		string						m_CurrentCycleName				= string.Empty;

		[SerializeField, ReadOnly]
		private		EnvDescriptor[]				m_Descriptors					= null;

		private		EnvDescriptor				m_EnvDescriptorCurrent			= null;
		private		EnvDescriptor				m_EnvDescriptorNext				= null;
		private		EnvDescriptorMixer			m_EnvDescriptorMixer			= new EnvDescriptorMixer();

		private		float						m_EnvEffectTimer				= 0.0f;
		private 	Quaternion					m_RotationOffset				= Quaternion.AngleAxis( 180f, Vector3.up );
		private		Light						m_Sun							= null;
		private		float						m_DayTimeNow					= -1.0f;
		private		bool						m_ShowDebugInfo					= false;

	#endregion

	#region INTERFACE BASE

		public float			TimeFactor
		{
			get => m_TimeFactor;
			set { m_TimeFactor =  Mathf.Max( value, 0f ); }
		}

		public float			DayTime => m_DayTimeNow;

	#endregion

		#region INTERFACE CYCLES

		/////////////////////////////////////////////////////////////////////////////
		string	IWeatherManager_Cycles.CurrentCycleName => m_CurrentCycleName;


		/////////////////////////////////////////////////////////////////////////////
		void IWeatherManager_Cycles.SetTime(float DayTime)
		{
			m_DayTimeNow = Mathf.Clamp(DayTime, 0f, DAY_LENGTH);
		}


		/////////////////////////////////////////////////////////////////////////////
		void IWeatherManager_Cycles.SetTime(float H, float M, float S)
		{
			m_DayTimeNow = Mathf.Clamp(((H * 3600f) + (M * 60f) + S), 0f, DAY_LENGTH);
		}


		/////////////////////////////////////////////////////////////////////////////
		void IWeatherManager_Cycles.SetTime(string sTime)
		{
			TransformTime(sTime, ref m_DayTimeNow);
		}


		/////////////////////////////////////////////////////////////////////////////
		string IWeatherManager_Cycles.GetTimeAsString()
		{
			string hours   = ((m_DayTimeNow / (60 * 60))).ToString("00");
			string minutes = (m_DayTimeNow / 60 % 60).ToString("00");
			string seconds = (m_DayTimeNow % 60).ToString("00");
			return $"{hours}:{minutes}:{seconds}";
		}


		/////////////////////////////////////////////////////////////////////////////
		void  IWeatherManager_Cycles.SetSkyColor(Color color) => m_SkyMaterial.SetColor("_Tint", color);
		Color IWeatherManager_Cycles.GetSkyColor() => m_SkyMaterial.GetColor("_Tint");

		void  IWeatherManager_Cycles.SetSkyExposure(float newValue) => m_SkyMaterial.SetFloat("_Exposure", newValue);
		float IWeatherManager_Cycles.GetSkyExposure() => m_SkyMaterial.GetFloat("_Exposure");


		/////////////////////////////////////////////////////////////////////////////
		void IWeatherManager_Cycles.SetWeatherByName(string weatherName)
		{
			WeatherCycle newCycle = m_Cycles.LoadedCycles.Find(c => c.name == weatherName);
			if (newCycle.IsNotNull())
			{
				ChangeWeather(newCycle);
			}
		}

		#endregion

		#region STATIC MEMBERS

		/////////////////////////////////////////////////////////////////////////////
		/// <summary> Set the corrisponding float value for given time ( HH:MM[:SS] ), return boolean as result </summary>
		public static bool TransformTime(string sTime, ref float Time)
		{
			int iH = 0, iM = 0, iS = 0;

			string[] parts = sTime.Split(':');
			iH = int.Parse(parts[0]);
			iM = int.Parse(parts[1]);
			iS = parts.Length == 3 ? int.Parse(parts[2]) : 0;

			if (IsValidTime(iH, iM, iS) == false)
			{
				Debug.LogWarning($"Incorrect weather time, {sTime}");
				return false;
			}

			Time = (float)((iH * 3600f) + (iM * 60f) + iS);
			return true;
		}

		/// <summary> Convert the given float value into a formatted string ( HH:MM:SS ), optionally can append seconds </summary>
		public static void TransformTime(float fTime, ref string Time, bool includeSeconds = true)
		{
			string hours = ((fTime / (60 * 60))).ToString("00");
			string minutes = (fTime / 60 % 60).ToString("00");
			string seconds = (fTime % 60).ToString("00");
			Time = $"{hours}:{minutes}{(includeSeconds?$":{seconds}":string.Empty)}";
		}

		/// <summary> Return true if the given value of HH, MM and SS are valid </summary>
		public static bool IsValidTime(float h, float m, float s) => ((h >= 0) && (h < 24) && (m >= 0) && (m < 60) && (s >= 0) && (s < 60));

		#endregion

		#region INITIALIZATION

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void SubsystemRegistration()
		{
			CustomAssertions.IsTrue(MonoBehaviourSingleton<WeatherManager>.TryInitializeSingleton(out m_Instance));
			m_Instance.enabled = true;
		}

		/////////////////////////////////////////////////////////////////////////////
		private void Awake()
		{
			// Singleton
			if (Instance.IsNotNull() && (object)Instance != this)
			{
#if UNITY_EDITOR
				// In EDITOR: If is editor play mode
				if (UnityEditor.EditorApplication.isPlaying)
				{
					Destroy(gameObject);   // Immediate is allowed
				}
				else
				{
					DestroyImmediate(gameObject);
				}
#else
				// In BUILD: Destroy normally ( because of singleton this should never happen )
				Debug.Log("Destroy called in build, this should never happen");
				Destroy( gameObject );	
#endif
				// in any case return
				return;
			}

			if (GlobalManager.Configs.TryGetSection("DebugInfos", out Database.Section debugInfosSection))
			{
				m_ShowDebugInfo = debugInfosSection.AsBool( "WeatherManager");
			}

#if UNITY_EDITOR
			// In EDITOR: If is editor play mode
			if (UnityEditor.EditorApplication.isPlaying)
			{
				DontDestroyOnLoad(this);
			}

			// This callback for the WindowWeatherEditor to update the reference to the current available instance of WeatherManager in scene
			void OnPlayModeStateChanged( UnityEditor.PlayModeStateChange newState )
			{
				WindowWeatherEditor.UpdateEditorInstance(true);
			}

			UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			m_Instance_Editor	= this as IWeatherManager_Editor;
#else
			// In BUILD: DontDestroyOnLoad called normally
			DontDestroyOnLoad( this );
#endif

			m_Instance_Cycles	= this;

		}

		/*
		/////////////////////////////////////////////////////////////////////////////
		private void Start()
		{
			if (CustomAssertions.IsNotNull(GameManager.StreamEvents))
			{
				GameManager.StreamEvents.OnSave += OnSave;
				GameManager.StreamEvents.OnLoad += OnLoad;
			}
		}
		*/

		/////////////////////////////////////////////////////////////////////////////
		private void			OnEnable()
		{
			LoadSkyMixerMaterial();

			if (m_Cycles.IsNotNull() && m_Cycles.LoadedCycles.Count > 0)
			{
				Setup_Cycles();

				// Select descriptors
				StartSelectDescriptors(m_DayTimeNow/* = Random.value * WeatherManager.DAY_LENGTH*/);

				// Make first env lerp
				EnvironmentLerp();
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		private void			OnDisable()
		{
			m_WeatherChoiceFactor	= 1.0f;
		}


		/*
		/////////////////////////////////////////////////////////////////////////////
		private void OnDestroy()
		{
			if (Instance.IsNotNull() && (object)Instance != this)
			{
				return;
			}

			if (GameManager.StreamEvents.IsNotNull())
			{
				GameManager.StreamEvents.OnSave -= OnSave;
				GameManager.StreamEvents.OnLoad -= OnLoad;
			}
		}
		*/


		/////////////////////////////////////////////////////////////////////////////
		private void OnApplicationQuit()
		{
			m_DayTimeNow = 0f;
		}


		/////////////////////////////////////////////////////////////////////////////
		private bool LoadSkyMixerMaterial()
		{
			if (!m_SkyMaterial)
			{
				Material loadedMaterial = Resources.Load<Material>(RESOURCES_SKYMIXER_MAT);
				Debug.Log("WeatherManager::OnEnabled: Loaded Material " + RESOURCES_SKYMIXER_MAT + ": " + ((loadedMaterial) ? "done" : "failed"));

				if (loadedMaterial)
				{
					// Load Sky Material
					m_SkyMaterial = new Material(loadedMaterial);
					return true;
				}
			}
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////
		private bool OnSave(StreamData streamData, ref StreamUnit streamUnit)
		{
			streamUnit = streamData.NewUnit(gameObject);
			{
				streamUnit.SetInternal("DayTimeNow", m_DayTimeNow);
				streamUnit.SetInternal("CycleName", m_CurrentCycleName);
			}
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////
		private bool OnLoad(StreamData streamData, ref StreamUnit streamUnit)
		{
			bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
			if (bResult)
			{
				m_DayTimeNow = streamUnit.GetAsFloat("DayTimeNow");
				string cycleName = streamUnit.GetInternal("CycleName");

				int index = m_Cycles.LoadedCycles.FindIndex(c => c.name == cycleName);
				if (index > -1)
				{
					WeatherCycle cycle = m_Cycles.LoadedCycles[index];
					ChangeWeather(cycle);
				}
			}
			return bResult;
		}


		/////////////////////////////////////////////////////////////////////////////
		private void			SetupSun()
		{
			if (!m_Sun)
			{
				// Create Sun
				Transform child = transform.Find("Sun");
				if (child)
				{
					m_Sun = child.GetOrAddIfNotFound<Light>();
				}
				else
				{
					child = new GameObject("Sun").transform;
					child.SetParent(transform);
					m_Sun = child.gameObject.AddComponent<Light>();
				}
			}
			m_Sun.type = LightType.Directional;
			m_Sun.shadows = LightShadows.Soft;

			if (m_ShowDebugInfo)
			{
				Debug.Log("Sun configured");
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		private void Setup_Cycles()
		{
			SetupSun();

			// Setup for Environment
			RenderSettings.sun		= m_Sun;
			RenderSettings.skybox	= m_SkyMaterial;

			// Defaults
			string startTime = "09:30:00";
			string startWeather = m_Cycles.LoadedCycles[0].name;

			// Get info from settings file
			if (GlobalManager.Configs.TryGetSection("Time", out Database.Section pSection))
			{
				pSection.TryAsString("StartTime",    out startTime,    startTime    );
				pSection.TryAsString("StartWeather", out startWeather, startWeather );
				pSection.TryAsFloat ("TimeFactor",   out m_TimeFactor, m_TimeFactor );
			}

			// Set current time
			if (m_DayTimeNow == -1f)
			{
				TransformTime(startTime, ref m_DayTimeNow);
			}

			startWeather = startWeather.Replace( "\"", "" );
			m_CurrentCycleName = "Invalid";

			if (m_ShowDebugInfo)
			{
				Debug.Log($"Applying time {startWeather}, {startTime}");
			}

			// Set current cycle
			int index = m_Cycles.LoadedCycles.FindIndex(c => c.name == startWeather);
			if (index > -1)
			{
				WeatherCycle cycle = m_Cycles.LoadedCycles[index];
				// set as current
				m_CurrentCycle = cycle;
				// update current descriptors
				m_Descriptors = cycle.LoadedDescriptors;
				// update cycle name
				m_CurrentCycleName = cycle.name;
				// current updated
				m_EnvDescriptorCurrent = m_EnvDescriptorNext;
			}

			m_WeatherChoiceFactor = 1.1f;

			m_EnvEffectTimer = Random.Range(2f, 5f);

			// Select descriptors
			if (m_Cycles.CyclesPaths.Count > 0)
			{
				StartSelectDescriptors(m_DayTimeNow);
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		private float TimeDiff(float Current, float Next)
		{
			if (Current > Next)
			{
				return (DAY_LENGTH - Current + Next);
			}
			else
			{
				return (Next - Current);
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		private	float			TimeInterpolant( float DayTime )
		{
			float interpolant = 0.0f;

			if (m_EnvDescriptorCurrent || m_EnvDescriptorNext)
			{
				float Current = m_EnvDescriptorCurrent.ExecTime;
				float Next = m_EnvDescriptorNext.ExecTime;

				float fLength = TimeDiff(Current, Next);
				if (!Utils.Math.SimilarZero(fLength, Utils.Math.EPS))
				{
					if (Current > Next)
					{
						if ((DayTime >= Current) || (DayTime <= Next))
						{
							interpolant = TimeDiff(Current, DayTime) / fLength;
						}
					}
					else
					{
						if ((DayTime >= Current) && (DayTime <= Next))
						{
							interpolant = TimeDiff(Current, DayTime) / fLength;
						}
					}
					interpolant = Mathf.Clamp01(interpolant + 0.0001f);
				}
			}

			return interpolant;
		}


		////////////////////////////////////////////////////////////////////////////
		private void SetCubemaps()
		{
			m_SkyMaterial.SetTexture("_Skybox1", m_EnvDescriptorCurrent.SkyCubemap);
			m_SkyMaterial.SetTexture("_Skybox2", m_EnvDescriptorNext.SkyCubemap);
		}


		////////////////////////////////////////////////////////////////////////////
		private EnvDescriptor GetPreviousDescriptor(EnvDescriptor current)
		{
			int idx = System.Array.IndexOf(m_Descriptors, current);
			return m_Descriptors[idx == 0 ? m_Descriptors.Length - 1 : (idx - 1)];
		}


		////////////////////////////////////////////////////////////////////////////
		private EnvDescriptor GetNextDescriptor(EnvDescriptor current)
		{
			int idx = System.Array.IndexOf(m_Descriptors, current);
			return m_Descriptors[(idx + 1) == m_Descriptors.Length ? 0 : (idx + 1)];
		}


		////////////////////////////////////////////////////////////////////////////
		private void StartSelectDescriptors(float DayTime, WeatherCycle cycle = null)
		{
			if (cycle.IsNotNull())
			{
				m_Descriptors = cycle.LoadedDescriptors;
			}

			// get the last valid descriptor where its execTime is less than dayTime
			EnvDescriptor descriptor = System.Array.FindLast(m_Descriptors, d => d.ExecTime < DayTime);

			EnvDescriptor first = m_Descriptors[0];
			EnvDescriptor last = m_Descriptors[m_Descriptors.Length - 1];
			if (descriptor == last)
			{
				m_EnvDescriptorCurrent = last;
				m_EnvDescriptorNext = first;
			}
			else
			{
				m_EnvDescriptorCurrent = descriptor;
				m_EnvDescriptorNext = GetNextDescriptor(descriptor);
			}

			if (m_ShowDebugInfo)
			{
				Debug.Log($"WeatherManager: Descriptors selected: {m_EnvDescriptorCurrent.Identifier}, {m_EnvDescriptorNext.Identifier}");
			}

			SetCubemaps();
		}


		/////////////////////////////////////////////////////////////////////////////
		private void ChangeWeather(WeatherCycle newCycle)
		{
			// find the corresponding of the current descriptor in the nex cycle
			int correspondingDescriptorIndex = System.Array.FindIndex(newCycle.LoadedDescriptors, (d => d.Identifier == m_EnvDescriptorNext.Identifier));
			if (correspondingDescriptorIndex == -1)
			{
				return;
			}

			EnvDescriptor correspondingDescriptor = newCycle.LoadedDescriptors[correspondingDescriptorIndex];

			if (m_ShowDebugInfo)
			{
				Debug.Log($"WeatherManager: Changing weather, requested: {newCycle.name}");
			}

			// set as current
			m_CurrentCycle = newCycle;

			// update current descriptors
			m_Descriptors = m_CurrentCycle.LoadedDescriptors;

			// current updated
			m_EnvDescriptorCurrent = m_EnvDescriptorNext;

			m_CurrentCycleName = m_CurrentCycle.name;

			// get descriptor next current from new cycle
			m_EnvDescriptorNext = GetNextDescriptor(correspondingDescriptor);
			Debug.Log($"New cycle: {newCycle.name}");
		}


		/////////////////////////////////////////////////////////////////////////////
		private void RandomWeather()
		{
			// Choose a new cycle
			int newIdx = Random.Range(0, m_Cycles.CyclesPaths.Count - 1);
			WeatherCycle cycle = m_Cycles.LoadedCycles[newIdx];

			if (m_ShowDebugInfo)
			{
				Debug.Log($"WeatherManager: Setting random Weather: {cycle.name}");
			}

			ChangeWeather(cycle);
		}


		/////////////////////////////////////////////////////////////////////////////
		private void SelectDescriptors(float DayTime)
		{
			bool bSelect = false;
			if (m_EnvDescriptorCurrent.ExecTime > m_EnvDescriptorNext.ExecTime)
			{
				bSelect = (DayTime > m_EnvDescriptorNext.ExecTime) && (DayTime < m_EnvDescriptorCurrent.ExecTime);
			}
			else
			{
				bSelect = (DayTime > m_EnvDescriptorNext.ExecTime);
			}
			if (bSelect)
			{
				// Choice for a new cycle
				float randomValue = Random.value;
				if (randomValue > m_WeatherChoiceFactor)
				{
					RandomWeather();
					m_WeatherChoiceFactor += randomValue;
				}
				else
				{
					// Editor stuff
					if (m_WeatherChoiceFactor <= 1.0f)
					{
						m_WeatherChoiceFactor = Mathf.Clamp01(m_WeatherChoiceFactor - 0.2f);
					}

					m_EnvDescriptorCurrent = m_EnvDescriptorNext;
					m_EnvDescriptorNext = GetNextDescriptor(m_EnvDescriptorNext);
				}
				SetCubemaps();
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		private void EnvironmentLerp()
		{
			float interpolant = TimeInterpolant(m_DayTimeNow);
			InterpolateOthers(interpolant);
			m_SkyMaterial.SetFloat("_Interpolant", interpolant);
		}


		/////////////////////////////////////////////////////////////////////////////
		private void InterpolateOthers(float interpolant)
		{
			EnvDescriptor current = m_EnvDescriptorCurrent;
			EnvDescriptor next = m_EnvDescriptorNext;

			m_EnvDescriptorMixer.AmbientColor		= Color.Lerp( current.AmbientColor,		next.AmbientColor,	interpolant );
			m_EnvDescriptorMixer.FogFactor			= Mathf.Lerp( current.FogFactor,		next.FogFactor,		interpolant );
			m_EnvDescriptorMixer.RainIntensity		= Mathf.Lerp( current.RainIntensity,	next.RainIntensity, interpolant );
			m_EnvDescriptorMixer.SkyColor			= Color.Lerp( current.SkyColor,			next.SkyColor,		interpolant );
			m_EnvDescriptorMixer.SunColor			= Color.Lerp( current.SunColor,			next.SunColor,		interpolant );
			m_EnvDescriptorMixer.SunRotation		= Vector3.Lerp( current.SunRotation,	next.SunRotation,	interpolant );

			RenderSettings.ambientSkyColor			= m_EnvDescriptorMixer.SkyColor;
			RenderSettings.ambientLight				= m_EnvDescriptorMixer.AmbientColor;

			RenderSettings.fog						= m_EnvDescriptorMixer.FogFactor > 0.0f;
			RenderSettings.fogDensity				= m_EnvDescriptorMixer.FogFactor;

			if (RainManager.Instance.IsNotNull())
			{
				RainManager.Instance.RainIntensity	= m_EnvDescriptorMixer.RainIntensity;
			}

			m_Sun.color								= m_EnvDescriptorMixer.SunColor;
		}


		/////////////////////////////////////////////////////////////////////////////
		private	void			AmbientEffectUpdate()
		{
			m_EnvEffectTimer -= Time.deltaTime;
			if (m_EnvEffectTimer < 0f)
			{
				AudioCollection effectCollection = m_EnvDescriptorCurrent.AmbientEffects;
				if (effectCollection.IsNotNull())
				{
					AudioClip clip = effectCollection.AudioClips[Random.Range(0, effectCollection.AudioClips.Length)];
					AudioSource.PlayClipAtPoint(clip, Player.Instance.transform.position);
				}

				m_EnvEffectTimer = Random.Range(3f, 7f); // TODO Expose there parameters
			}
		}

		#endregion

		/////////////////////////////////////////////////////////////////////////////
		private void Update()
		{
#if UNITY_EDITOR
			if (runInEditMode) // 
				return;
#endif

			if (m_EnvDescriptorCurrent.IsSet && m_EnvDescriptorNext.IsSet)
			{
					m_DayTimeNow += Time.deltaTime * m_TimeFactor;
					if (m_DayTimeNow > DAY_LENGTH)
					{
						m_DayTimeNow = 0.0f;
					}

					// Only every 10 frames
					if (Time.frameCount % 10 == 0)
					{
						return;
					}

					TransformTime(m_DayTimeNow, ref m_CurrentDayTime);

					SelectDescriptors(m_DayTimeNow);

					EnvironmentLerp();

					AmbientEffectUpdate();

					// Sun rotation by data
					m_Sun.transform.rotation = m_RotationOffset * Quaternion.LookRotation(m_EnvDescriptorMixer.SunRotation);
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		private void			Reset()
		{
			m_Cycles				= null;
			m_CurrentCycle			= null;
			m_CurrentCycleName		= string.Empty;
			m_Descriptors			= null;
			m_EnvDescriptorCurrent	= null;
			m_EnvDescriptorNext		= null;

			m_Instance_Cycles		= this;

			LoadSkyMixerMaterial();
#if UNITY_EDITOR
			m_Instance_Editor		= this;
			runInEditMode			= false;
			UnityEditor.EditorApplication.update -= EditorUpdate;
#endif
		}
	}
}