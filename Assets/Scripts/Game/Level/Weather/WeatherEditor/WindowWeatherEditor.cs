using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

namespace WeatherSystem {

	public class WindowWeatherEditor : EditorWindow {

		public const string CYCLESKIES_PATH			= "Assets/Resources/Weather/SkyMaterials";
		public const string RESOURCE_PATH			= "Assets/Resources/Weather/Descriptors";
		public const string COLLECTION_FILENAME		= "WeatherCollection";

		public	static	WindowWeatherEditor				m_Window				= null;

		public	Weathers								m_WeathersCycles		= null;

		private Vector2									m_ScrollPosition		= Vector2.zero;


//		private static GUIStyle textAreaWrapTextStyle = null;


		/////////////////////////////////////////////////////////////////////////////
		// Init
		[ MenuItem ( "Window/Weather Manager" ) ]
		public static	void	Init()
		{
			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<WindowWeatherEditor>( true, "Weather Manager" );
			m_Window.minSize = new Vector2( 400f, 400f );
			m_Window.maxSize = new Vector2( 400f, 800f );
			/*
			if ( textAreaWrapTextStyle == null )
			{
				textAreaWrapTextStyle = new GUIStyle( EditorStyles.textArea );
				textAreaWrapTextStyle.wordWrap = true;
			}
			*/
			Setup();
		}


		/////////////////////////////////////////////////////////////////////////////
		// Setup
		private static	void	Setup()
		{
			// Create directory if not exists
			if ( System.IO.Directory.Exists( RESOURCE_PATH ) == false )
				System.IO.Directory.CreateDirectory( RESOURCE_PATH );

			if ( System.IO.Directory.Exists( CYCLESKIES_PATH ) == false )
				System.IO.Directory.CreateDirectory( CYCLESKIES_PATH );

			// Create or load asset
			string assetPath = RESOURCE_PATH + "/" + COLLECTION_FILENAME + ".asset";
			if ( System.IO.File.Exists( assetPath ) == false )
			{
				m_Window.m_WeathersCycles = ScriptableObject.CreateInstance<Weathers>();
				AssetDatabase.CreateAsset( m_Window.m_WeathersCycles, assetPath );
				AssetDatabase.SaveAssets();
			}
			else
			{
				m_Window.m_WeathersCycles = AssetDatabase.LoadAssetAtPath<Weathers>( assetPath );
			}

			if ( m_Window.m_WeathersCycles.Cycles == null )
			{
				m_Window.m_WeathersCycles.Cycles = new List<WeatherCycle>();
			}

			EditorUtility.SetDirty( m_Window.m_WeathersCycles );
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		/////////////////////////////////////////////////////////////////////////////
		// DeleteWeather
		private	void	CreateCycle( string name )
		{
			string assetPath = RESOURCE_PATH + "/" + name + ".asset";
			if ( System.IO.File.Exists( assetPath ) )
				return;

			WeatherCycle asset = ScriptableObject.CreateInstance<WeatherCycle>();
			asset.name = name;
			asset.AssetPath = assetPath;

			AssetDatabase.CreateAsset( asset, assetPath );

			for ( int i = 0; i < 24; i++ )
			{
				EnvDescriptor envDescriptor = new EnvDescriptor();
				string identifier = "";
				WeatherManager.TransformTime( (float)i * 3600f, ref identifier, false );
				envDescriptor.Identifier = identifier;
				envDescriptor.ExecTime = (float)i * 3600f;
				asset.Descriptors[i] = envDescriptor;
			}

			EditorUtility.SetDirty( asset );
			m_WeathersCycles.Cycles.Add( asset );
			EditorUtility.SetDirty( m_WeathersCycles );
			AssetDatabase.SaveAssets();

			string cycleSkyiesPath = CYCLESKIES_PATH + "/" + name;
			if ( System.IO.Directory.Exists( cycleSkyiesPath ) == true )
				System.IO.Directory.Delete( cycleSkyiesPath, true );

			System.IO.Directory.CreateDirectory( cycleSkyiesPath );
			AssetDatabase.Refresh();
		}


		/////////////////////////////////////////////////////////////////////////////
		// DeleteWeather
		private	void	DeleteCycle( int idx )
		{
			EditorUtility.SetDirty( m_WeathersCycles );

			if ( m_WeathersCycles.Cycles[ idx ] == null )
			{
				m_WeathersCycles.Cycles.RemoveAt( idx );
			}
			else
			{
				string assetPath = m_WeathersCycles.Cycles[ idx ].AssetPath;
				string cycleName = m_WeathersCycles.Cycles[ idx ].name;
				m_WeathersCycles.Cycles.RemoveAt( idx );
				AssetDatabase.DeleteAsset( assetPath );

				string cycleSkyiesPath = WindowWeatherEditor.CYCLESKIES_PATH + "/" + cycleName;
				if ( System.IO.Directory.Exists( cycleSkyiesPath ) == true )
				System.IO.Directory.Delete( cycleSkyiesPath, true );
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private	void	OnGUI()
		{
			if ( m_WeathersCycles == null || m_WeathersCycles.Cycles == null )
			{
				Setup();
				return;
			}

			if ( GUILayout.Button( "Create cycle" ) )
			{
				WindowValueStep.Init<string>( () => CreateCycle ( WindowValueStep.Value ) );
			}

			if ( m_WeathersCycles.Cycles.Count == 0 )
				return;

			
			m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition );
			{
				GUILayout.BeginVertical();
				{
					for ( int i = 0; i < m_WeathersCycles.Cycles.Count; i++ )
					{
						WeatherCycle weatherCycle = m_WeathersCycles.Cycles[ i ];
						if ( weatherCycle == null )
						{
							Debug.Log( "Cycle null at index " + i );
							DeleteCycle( i-- );
							continue;
						}

						GUILayout.BeginHorizontal();
						{
							GUILayout.Label( weatherCycle.name );
							if ( GUILayout.Button( "Edit" ) )
							{
								WindowCycleEditor.Init( weatherCycle );;
							}
								if ( GUILayout.Button( "Delete" ) )
							{
								DeleteCycle( i-- );
								continue;
							}
						}
						GUILayout.EndHorizontal();
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndScrollView();
		}


		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private	void	OnDestroy()
		{
			if ( WindowDescriptorEditor.m_Window != null )
				WindowDescriptorEditor.m_Window.Close();

			if ( WindowCycleEditor.m_Window != null )
				WindowCycleEditor.m_Window.Close();

			if ( WindowValueStep.m_Window != null )
				WindowValueStep.m_Window.Close();

			EditorUtility.SetDirty( m_WeathersCycles );
			AssetDatabase.SaveAssets();

			m_Window = null;
		}
	}

}