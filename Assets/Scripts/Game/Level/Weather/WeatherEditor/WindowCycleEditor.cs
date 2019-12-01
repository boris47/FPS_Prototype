using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

namespace WeatherSystem {

	public class WindowCycleEditor : EditorWindow {

		public	static	WindowCycleEditor		m_Window			= null;
	
		private			WeatherCycle			m_CurrentCycle		= null;
		private			Color					m_OriginalColor		= Color.clear;
		private			float					m_CurrentTime		= 0.0001f;
		private			float					m_PrevTime			= 0.0001f;

		/////////////////////////////////////////////////////////////////////////////
		// Init ( EDITING )
		public static void Init( string assetWeatherCyclePath )
		{
			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}

			string weatherCycleName = System.IO.Path.GetFileNameWithoutExtension( assetWeatherCyclePath );

			m_Window = EditorWindow.GetWindow<WindowCycleEditor>( true, "Cycle Editor: " + weatherCycleName );
			m_Window.minSize = m_Window.maxSize = new Vector2( 600f, 600f );

			string path = /*"Assets/Resources/" +*/ assetWeatherCyclePath;
			m_Window.m_CurrentCycle = AssetDatabase.LoadAssetAtPath<WeatherCycle>( path );

			
			float dayTimeNow = WindowWeatherEditor.GetWMGR().INTERNAL_DayTimeNow;

			m_Window.m_CurrentTime = ( dayTimeNow / WeatherManager.DAY_LENGTH );
			
			Setup();
		}
		

		/////////////////////////////////////////////////////////////////////////////
		// Setup
		private	static	void Setup()
		{
			for ( int i = 0; i < m_Window.m_CurrentCycle.DescriptorsPaths.Length; i++ )
			{
				EnvDescriptor loadedDescriptor = m_Window.m_CurrentCycle.LoadedDescriptors[i];
				if ( loadedDescriptor == null )
				{
					string descriptorPath = m_Window.m_CurrentCycle.DescriptorsPaths[i];

					loadedDescriptor = Resources.Load<EnvDescriptor>( descriptorPath );
				}
				m_Window.m_CurrentCycle.LoadedDescriptors[i] = loadedDescriptor;
			}

			WindowWeatherEditor.GetWMGR().INTERNAL_EditorCycleLinked = true;
			WindowWeatherEditor.GetWMGR().INTERNAL_Start( m_Window.m_CurrentCycle, 2f );
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
			if ( m_CurrentTime != m_PrevTime )
			{
				WindowWeatherEditor.GetWMGR().INTERNAL_StartSelectDescriptors( WeatherManager.DAY_LENGTH * m_CurrentTime, m_CurrentCycle );
			}
			m_PrevTime = m_CurrentTime;

			WindowWeatherEditor.GetWMGR().INTERNAL_DayTimeNow = WeatherManager.DAY_LENGTH * m_CurrentTime;


			// CONFIG FILE
			if ( GUILayout.Button( "Read Config File" ) )
			{
				string path = EditorUtility.OpenFilePanel( "Pick a config file", "", "txt" );
				m_CurrentCycle.LoadFromPresetFile( path );
			}

			for ( int i = 0; i < 24; i++ )
			{
				float bo = ( 360f / 12f * (float)i );
				
				EnvDescriptor thisDescriptor = m_CurrentCycle.LoadedDescriptors[ i ];

				if ( i > 0 && m_CurrentCycle.LoadedDescriptors[ i - 1 ].set == false )
					return;

				// BACKGROUND COLOR ADAPTED
				m_OriginalColor = GUI.backgroundColor;
				
				GUI.backgroundColor = GetColor( thisDescriptor );
				{
					float twentyFourVis = 100f * ( ( i < 12 ) ? 1.4f : 2f );
					Rect btnRect = new Rect( Screen.width/2 + Mathf.Sin( bo * Mathf.Deg2Rad ) * twentyFourVis, Screen.height/2 - Mathf.Cos( bo * Mathf.Deg2Rad ) * twentyFourVis, 50f, 25f );
					if ( GUI.Button( btnRect, thisDescriptor.Identifier ) == true )
					{
						if ( i > 0 && m_CurrentCycle.LoadedDescriptors[ i - 1 ].set == true && thisDescriptor.set == false )
						{
							EnvDescriptor loaded = m_CurrentCycle.LoadedDescriptors[ i - 1 ];
							EnvDescriptor.Copy( ref thisDescriptor, loaded );
						}

						WindowDescriptorEditor.Init( thisDescriptor );
						EditorUtility.SetDirty( m_CurrentCycle );
					}
				}
				GUI.backgroundColor = m_OriginalColor;
				// BACKGROUND COLOR RESET
			}
		}

		// 
		private static Color GetColor( EnvDescriptor thisDescriptor )
		{
			EnvDescriptor currDesc = WindowWeatherEditor.GetWMGR().INTERNAL_CurrentDescriptor;
			EnvDescriptor nextDesc = WindowWeatherEditor.GetWMGR().INTERNAL_NextDescriptor;

			Color toSet = ( currDesc != null && thisDescriptor == currDesc ) ? Color.yellow : ( thisDescriptor.set ? Color.green : Color.red );
				  toSet = ( nextDesc != null && thisDescriptor == nextDesc ) ? Color.cyan : toSet;
			return toSet;
		}


		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private void OnDestroy()
		{
			if ( WindowDescriptorEditor.m_Window != null )
				WindowDescriptorEditor.m_Window.Close();

			EditorUtility.SetDirty( m_CurrentCycle );
			AssetDatabase.SaveAssets();

			WeatherManager.Editor.INTERNAL_Start( m_CurrentCycle, Random.value );
			WeatherManager.Editor.INTERNAL_EditorDescriptorLinked = false;
			WeatherManager.Editor.INTERNAL_EditorCycleLinked = false;
		}
	}

}

#endif