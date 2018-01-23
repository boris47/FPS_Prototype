using UnityEngine;
using System.Collections.Generic;

using UnityEditor;

namespace WeatherSystem {

	public class WindowWeatherEditor : EditorWindow {
	
		public const string RESOURCE_PATH			= "Assets/Resources/Weather/Descriptors";
		public const string COLLECTION_FILENAME		= "WeatherCollection";


		public	static	WindowWeatherEditor				m_Window				= null;

		public	Weathers								m_WeathersCycles		= null;

		private Vector2									m_ScrollPosition		= Vector2.zero;


		private static GUIStyle textAreaWrapTextStyle = null;


		/////////////////////////////////////////////////////////////////////////////
		/// Init
		[ MenuItem ( "Window/Weather Manager" ) ]
		public static void Init()
		{
			if ( m_Window != null )
			{
				m_Window.Close();
				m_Window = null;
			}
			m_Window = EditorWindow.GetWindow<WindowWeatherEditor>( true, "Weather Manager" );
			m_Window.minSize = new Vector2( 800, 600 );

			if ( textAreaWrapTextStyle == null )
			{
				textAreaWrapTextStyle = new GUIStyle( EditorStyles.textArea );
				textAreaWrapTextStyle.wordWrap = true;
			}

			Setup();
		}

		private static	void	Setup()
		{
			// Create directory if not exists
			if ( System.IO.Directory.Exists( RESOURCE_PATH ) == false )
				System.IO.Directory.CreateDirectory( RESOURCE_PATH );


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
		/// DeleteWeather
		private void DeleteCycle( Weathers cyclesCollection, int idx )
		{
			EditorUtility.SetDirty( cyclesCollection );

			if ( cyclesCollection.Cycles[ idx ] == null )
			{
				cyclesCollection.Cycles.RemoveAt( idx );
			}
			else
			{
				for ( int i = 0; i < cyclesCollection.Cycles[ idx ].Descriptors.Count; i++ )
				{
					DeleteDescriptor( cyclesCollection.Cycles[ idx ], i );
				}

				string assetPath = cyclesCollection.Cycles[ idx ].AssetPath;
				cyclesCollection.Cycles.RemoveAt( idx );
				System.IO.Directory.Delete( System.IO.Path.GetDirectoryName( assetPath ), true );
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		/////////////////////////////////////////////////////////////////////////////
		/// DeleteDescriptor
		private void DeleteDescriptor( WeatherCycle collection, int idx )
		{
			EditorUtility.SetDirty( collection );
			if ( collection.Descriptors[ idx ] == null )
			{
				collection.Descriptors.RemoveAt( idx );
			}
			else
			{
				string assetPath = collection.Descriptors[ idx ].AssetPath;
				collection.Descriptors.RemoveAt( idx );
				AssetDatabase.DeleteAsset( assetPath );
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		/////////////////////////////////////////////////////////////////////////////
		/// UNITY
		private void OnGUI()
		{
			if ( m_WeathersCycles == null || m_WeathersCycles.Cycles == null )
			{
				Setup();
				return;
			}

			if ( GUILayout.Button( "Create cycle" ) )
			{
				// TODO: NEW DEDICATED WINDOW
				WindowValueStep.Init<string>( () => WindowCycleEditor.Init( WindowValueStep.Value ) );
			
			}

			if ( m_WeathersCycles.Cycles.Count == 0 )
				return;

			
			m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition );
			{
				GUILayout.BeginHorizontal();
				{
					for ( int i = 0; i < m_WeathersCycles.Cycles.Count; i++ )
					{
						WeatherCycle weatherCycle = m_WeathersCycles.Cycles[ i ];
						if ( weatherCycle == null )
						{
							Debug.Log( "Cycle null at index " + i );
							DeleteCycle( m_WeathersCycles, i-- );
							continue;
						}

						GUILayout.BeginVertical();
						{
							GUILayout.Label( "Cycle" );
							GUILayout.BeginHorizontal();
							{

								GUILayout.Label( weatherCycle.name );
								if ( GUILayout.Button( "New Descriptor" ) )
								{
									if ( weatherCycle.Descriptors == null )
										weatherCycle.Descriptors = new List<EnvDescriptor>();
									WindowValueStep.Init<string>( () => WindowDescriptorEditor.Init( weatherCycle, WindowValueStep.Value ) );
								}

								if ( GUILayout.Button( "Delete" ) )
								{
									DeleteCycle( m_WeathersCycles, i-- );
									continue;
								}
							}
							GUILayout.EndHorizontal();


							if ( weatherCycle.Descriptors == null || weatherCycle.Descriptors.Count == 0 )
								continue;

							GUILayout.Label( "" ); // space
							GUILayout.Label( "DESCRIPTORS" );
							GUILayout.BeginVertical();
							for ( int j = 0; j < weatherCycle.Descriptors.Count; j++ )
							{
								EnvDescriptor envDescriptor = weatherCycle.Descriptors[ j ];
								if ( envDescriptor == null )
								{
									Debug.Log( "Descript null at index " + i + " of cycle " + weatherCycle.name );
									DeleteDescriptor( weatherCycle, j-- );
									continue;
								}

								GUILayout.BeginHorizontal();
								{
									GUILayout.Label( envDescriptor.name );

									if ( GUILayout.Button( "Edit" ) )
									{
										// TODO EDIT WINDOW FOR ENVDESCRIPTOR
										WindowDescriptorEditor.Init( envDescriptor );
									}
									if ( GUILayout.Button( "Del" ) )
									{
										DeleteDescriptor( weatherCycle, j-- );
										continue;
									}
								}
								GUILayout.EndHorizontal();
							}
							GUILayout.EndVertical();
						}
						GUILayout.EndVertical();
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
		}



		private void OnDestroy()
		{
			if ( WindowDescriptorEditor.m_Window != null )
				WindowDescriptorEditor.m_Window.Close();

			if ( WindowCycleEditor.m_Window != null )
				WindowCycleEditor.m_Window.Close();

			if ( WindowValueStep.m_Window != null )
				WindowValueStep.m_Window.Close();

			m_Window = null;
		}
	}

}