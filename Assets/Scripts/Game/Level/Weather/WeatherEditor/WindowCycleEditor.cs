using UnityEngine;
using CFG_Reader;

#if UNITY_EDITOR

using UnityEditor;

namespace WeatherSystem {

	public class WindowCycleEditor : EditorWindow {

		public	static	WindowCycleEditor		m_Window			= null;
	
		private			WeatherCycle			m_CurrentCycle		= null;
		
		private			Color					m_OriginaColor		= Color.clear;

		private			float					m_CurrentTime		= 0.0001f;

		private			float					m_PrevTieme			= 0.0001f;


		/////////////////////////////////////////////////////////////////////////////
		// Init ( EDITING )
		public static void Init( WeatherCycle cycle )
		{
			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<WindowCycleEditor>( true, "Cycle Editor" );
			m_Window.minSize = m_Window.maxSize = new Vector2( 600f, 600f );

			m_Window.m_CurrentCycle = cycle;



			m_Window.m_CurrentTime = ( WeatherManager.Instance.DayTime / WeatherManager.DAY_LENGTH );
			( WeatherManager.Instance as IWeatherManagerInternal ).EditorLinked = true;
			( WeatherManager.Instance as IWeatherManagerInternal ).Start( cycle, 2f );

			Setup();
		}
		

		/////////////////////////////////////////////////////////////////////////////
		// Setup
		public	static	void Setup()
		{
			if ( System.IO.Directory.Exists( WindowWeatherEditor.CYCLESKIES_PATH ) == false )
				return;

			string cycleSkyiesPath = WindowWeatherEditor.CYCLESKIES_PATH + "/" + m_Window.m_CurrentCycle.name;
			if ( System.IO.Directory.Exists( cycleSkyiesPath ) == false )
				return;

			
			string[] files = System.IO.Directory.GetFiles( cycleSkyiesPath, "*.png" );
			if ( files.Length < 24 )
				return;
			
			for ( int i = 0; i < files.Length; i++ )
			{
				string filePath = files[ i ];

				string descriptorName = System.IO.Path.GetFileNameWithoutExtension( filePath );
				string assetPath = cycleSkyiesPath + "/" + descriptorName + ".png";
//				Debug.Log( "FILEPATH:   " + filePath );
//				Debug.Log( "DESCRIPTOR: " + descriptorName );
//				Debug.Log( "ASSET PATH: " + assetPath );
				Cubemap map = AssetDatabase.LoadAssetAtPath<Cubemap>( assetPath );
				m_Window.m_CurrentCycle.Descriptors[ i ].SkyCubemap = map;
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private void OnGUI()
		{
			// LIGHT CONTROL FOR SUN SIMULATION
			string timeAsString = string.Empty;
			WeatherManager.TransformTime( WeatherManager.DAY_LENGTH * m_CurrentTime, ref timeAsString, false );

			GUILayout.Label( timeAsString );
			m_CurrentTime = EditorGUILayout.Slider( m_CurrentTime, 0.0001f, 1.0f );
			if ( m_CurrentTime != m_PrevTieme )
			{
				( WeatherManager.Instance as IWeatherManagerInternal ).StartSelectDescriptors( WeatherManager.DAY_LENGTH * m_CurrentTime, m_CurrentCycle );
			}
			m_PrevTieme = m_CurrentTime;

			( WeatherManager.Instance as IWeatherManagerInternal ).DayTimeNow = WeatherManager.DAY_LENGTH * m_CurrentTime;


			// CONFIG FILE
			if ( GUILayout.Button( "Read Config File" ) )
			{
				string path = EditorUtility.OpenFilePanel( "Pick a config file", "", "ltx" );
				Reader reader = new Reader();
				if ( path.Length == 0 )
					return;

				if ( reader.LoadFile( path ) == false )
				{
					 EditorUtility.DisplayDialog( "Error !", "Selected file cannot be parsed !", "OK" );
				}
				else
				{
					foreach( EnvDescriptor descriptor in m_CurrentCycle.Descriptors )
					{
						Debug.Log( "Parsing data for descripter " + descriptor.Identifier );
						Section section = reader.GetSection( descriptor.Identifier + ":00" );
						if ( section != null )
						{
							if ( section.HasKey( "ambient_color" ) )
								Utils.Converters.StringToColor( section.GetRawValue("ambient_color"),		ref descriptor.AmbientColor	);

							if ( section.HasKey( "sky_color" ) )
								Utils.Converters.StringToColor( section.GetRawValue("sky_color"),			ref descriptor.SkyColor		);

							if ( section.HasKey( "sun_color" ) )
								Utils.Converters.StringToColor(	section.GetRawValue("sun_color"),			ref descriptor.SunColor		);

							if ( section.HasKey( "fog_density" ) )
								descriptor.FogFactor		= section.AsFloat( "fog_density" );

							if ( section.HasKey( "rain_density" ) )
								descriptor.RainIntensity	= section.AsFloat( "rain_density" );
							if ( section.HasKey( "sun_rotation" ) )
								Utils.Converters.StringToVector(section.GetRawValue("sun_rotation"),		ref descriptor.SunRotation	);
							else if ( section.HasKey( "sun_altitude" ) )
							{
								descriptor.SunRotation = Utils.Math.VectorByHP( section.AsFloat( "sun_altitude" ), section.AsFloat( "sun_longitude" ) );
							}

						}
						descriptor.set = true;
						Debug.Log( "Data parsed correctly" );
						section = null;
					}
					reader = null;
				}
			}

			for ( int i = 0; i < m_CurrentCycle.Descriptors.Length; i++ )
			{
				float bo = ( 360f / 24f * (float)i );
				
				EnvDescriptor thisDescriptor = m_CurrentCycle.Descriptors[ i ];

				if ( i > 0 && m_CurrentCycle.Descriptors[ i - 1 ].set == false )
					return;

				// BACKGROUND COLOR ADAPTED
				m_OriginaColor = GUI.backgroundColor;
				Color	toSet = thisDescriptor.set ? Color.green : Color.red;
						toSet = thisDescriptor == ( WeatherManager.Instance as IWeatherManagerInternal ).CurrentDescriptor ? Color.yellow : toSet;
				GUI.backgroundColor = toSet;
				{
					if ( GUI.Button( new Rect( 
							Screen.width /2 + Mathf.Sin( bo * Mathf.Deg2Rad ) * 200f,
							Screen.height/2 - Mathf.Cos( bo * Mathf.Deg2Rad ) * 200f,
							50,
							25 ),
						thisDescriptor.Identifier )
					)
					{
						if ( i > 0 && m_CurrentCycle.Descriptors[ i - 1 ].set == true && thisDescriptor.set == false )
						{
							thisDescriptor.Copy( m_CurrentCycle.Descriptors[ i - 1 ] );
						}

						WindowDescriptorEditor.Init( thisDescriptor );
						EditorUtility.SetDirty( m_CurrentCycle );
					}
				}
				GUI.backgroundColor = m_OriginaColor;
				// BACKGROUND COLOR RESET
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private void OnDestroy()
		{
			( WeatherManager.Instance as IWeatherManagerInternal ).Start( m_CurrentCycle, Random.value );
			( WeatherManager.Instance as IWeatherManagerInternal ).EditorLinked = false;
			EditorUtility.SetDirty( m_CurrentCycle );
			AssetDatabase.SaveAssets();
		}
	}

}

#endif