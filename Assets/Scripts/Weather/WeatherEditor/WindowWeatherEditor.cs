using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

namespace WeatherSystem {

	public class WindowWeatherEditor : EditorWindow {

		public const string RESOURCES_WM_PREFAB			= "Prefabs/Essentials/WeatherManager";
		public const string ASSETS_CYCLESKIES_PATH		= "Assets/Resources/SkyCubeMaps";
		public const string ASSETS_SCRIPTABLES_PATH		= "Assets/Resources/Scriptables";
		public const string ASSETS_DESCRIPTORS_PATH		= "Assets/Resources/Scriptables/Descriptors";

		public	static	WindowWeatherEditor				m_Window				= null;

		private Vector2									m_ScrollPosition		= Vector2.zero;

		private	static	IWeatherManager_Editor			m_WeatherManager		= null;


		public	static	IWeatherManager_Editor	GetWMGR( bool bForceSearch = false )
		{
			if ( bForceSearch )
			{
				WeatherManager wmgr = Object.FindObjectOfType<WeatherManager>();
				m_WeatherManager = wmgr as IWeatherManager_Editor;
			}

			if ( m_WeatherManager == null || m_WeatherManager.GetType() != typeof(WeatherManager) )
			{
				if ( WeatherManager.Editor == null )
				{
					WeatherManager wmgr = Object.FindObjectOfType<WeatherManager>();
					m_WeatherManager = wmgr as IWeatherManager_Editor;
				}
				else
				{
					m_WeatherManager = WeatherManager.Editor;
				}
			}
			return m_WeatherManager;
		}

		/////////////////////////////////////////////////////////////////////////////
		// Init
		[ MenuItem ( "Window/Weather Manager" ) ]
		public static	void	Init()
		{
			if ( m_Window != null )
			{
				return;
			}

			if ( GetWMGR( bForceSearch: true) == null )
			{
				Debug.Log("Cannot find WeatherManager");
				return;
			}
			
			m_Window = EditorWindow.GetWindow<WindowWeatherEditor>( true, "Weather Manager" );
			m_Window.minSize = new Vector2( 400f, 200f );
			m_Window.maxSize = new Vector2( 400f, 600f );

			Setup();
		}

		/////////////////////////////////////////////////////////////////////////////
		// Setup
		private static	void	Setup()
		{
			// Create directories if not exists
			if ( System.IO.Directory.Exists( ASSETS_SCRIPTABLES_PATH ) == false )
				System.IO.Directory.CreateDirectory( ASSETS_SCRIPTABLES_PATH );

			if ( System.IO.Directory.Exists( ASSETS_CYCLESKIES_PATH ) == false )
				System.IO.Directory.CreateDirectory( ASSETS_CYCLESKIES_PATH );

			if ( System.IO.Directory.Exists( ASSETS_DESCRIPTORS_PATH ) == false )
				System.IO.Directory.CreateDirectory( ASSETS_DESCRIPTORS_PATH );


			// Create or load asset
			Weathers cycles = GetWMGR().EDITOR_Cycles = GetCycles();

			if ( cycles.CyclesPaths.Count > 0 )
			{
//				GetWMGR().INTERNAL_ForceEnable();
				GetWMGR().EDITOR_EditorLinked = true;
			}

		}


		/////////////////////////////////////////////////////////////////////////////
		// GetCycles
		private	static Weathers GetCycles()
		{
			Weathers cycles = null;
			const string assetPath = WindowWeatherEditor.ASSETS_SCRIPTABLES_PATH + "/" + WeatherManager.RESOURCES_WEATHERSCOLLECTION + ".asset";
			if ( System.IO.File.Exists( assetPath ) == false )
			{
				cycles = ScriptableObject.CreateInstance<Weathers>();
				AssetDatabase.CreateAsset( cycles, assetPath );
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				cycles = AssetDatabase.LoadAssetAtPath<Weathers>( assetPath );
			}

			return cycles;
		}


		/////////////////////////////////////////////////////////////////////////////
		// Create Weather Cycle
		private	static string	CreateCycle( string cycleName, string presetFilePath = null )
		{
			// SKIP IF ALREADY EXISTING
			string cycleAssetPath = ASSETS_SCRIPTABLES_PATH + "/" + cycleName + ".asset";
			if ( System.IO.File.Exists( cycleAssetPath ) )
				return null;

			EditorUtility.DisplayProgressBar( "Cycle Creation", "", 0.1f );

			// CREATE SUB-DESCRIPTORS FOLDER
			string descriptorsCyclePath = ASSETS_DESCRIPTORS_PATH + "/" + cycleName + "/";
			System.IO.Directory.CreateDirectory( descriptorsCyclePath );

			// CREATE WEATHER CYCLE
			WeatherCycle weatherCycle = ScriptableObject.CreateInstance<WeatherCycle>();
			{
				weatherCycle.name = cycleName;
				weatherCycle.AssetPath = cycleAssetPath;
			}
			AssetDatabase.CreateAsset( weatherCycle, cycleAssetPath );
			EditorUtility.SetDirty( weatherCycle );

			EditorUtility.DisplayProgressBar( "Cycle Creation", "", 0.2f );

			// ADD DESCRIPTORS
			for ( int i = 0; i < 24; i++ )
			{
				EditorUtility.DisplayProgressBar( "Cycle Creation", "", 0.2f + (i * 0.01f) );
				string assetDescriptorLoadPath = CreateDescriptor( cycleName, (float)i );
				weatherCycle.DescriptorsPaths[i] = assetDescriptorLoadPath;

				EditorUtility.DisplayProgressBar( "Cycle Creation", "", 0.2f + (i * 0.15f) );

				// Load created asset
				EnvDescriptor desc = AssetDatabase.LoadAssetAtPath<EnvDescriptor>( assetDescriptorLoadPath + ".asset" );
				weatherCycle.LoadedDescriptors[i] = desc;
			}

			// Load preset if present
			if ( string.IsNullOrEmpty( presetFilePath ) == false )
			{
				weatherCycle.LoadFromPresetFile( presetFilePath );
			}

			EditorUtility.ClearProgressBar();
			return cycleAssetPath;
		}


		// Create Weather Cycle Descriptor
		private static	string	CreateDescriptor( string cycleName, float Hour )
		{
			string cycleSkyiesPath		= ASSETS_CYCLESKIES_PATH + "/" + cycleName + "/";
			string descriptorsCyclePath	= ASSETS_DESCRIPTORS_PATH + "/" + cycleName + "/";
			float  execTime				= Hour * 3600f;


			// IDENTIFIER
			string identifier = string.Empty;
			WeatherManager.TransformTime( execTime, ref identifier, considerSeconds: false );

			Debug.Log( "Creating descrptor: " + cycleName + ", " + identifier );
			string identifierReplced = identifier.Replace( ':', '-' );

			// CREATE DESCRIPTOR
			string descriptorAssetPath = descriptorsCyclePath + identifierReplced + ".asset";
			EnvDescriptor envDescriptor = ScriptableObject.CreateInstance<EnvDescriptor>();
			{
				envDescriptor.Identifier	= identifier;
				envDescriptor.name			= identifier;
				envDescriptor.AssetPath		= descriptorAssetPath;
				envDescriptor.ExecTime		= execTime;
			}
			AssetDatabase.CreateAsset( envDescriptor, descriptorAssetPath );
			EditorUtility.SetDirty( envDescriptor );


			// LOAD SKY CUBE MAP IF PRESENT
			{
				// Ex: Assets/Resources/SkyCubeMaps/Clear/
				if ( System.IO.Directory.Exists( cycleSkyiesPath ) == true )
				{
					// Ex: Assets/Resources/SkyCubeMaps/Clear/00-00
					string skyCubeMapPath = cycleSkyiesPath + identifierReplced + ".jpg";
					if ( System.IO.File.Exists( skyCubeMapPath ) == true )
					{
						envDescriptor.SkyCubemap = AssetDatabase.LoadAssetAtPath<Cubemap>( skyCubeMapPath );
						Debug.Log( "Cubemap " + ( envDescriptor.SkyCubemap != null ?  "assigned: " : "not loaded: " ) + skyCubeMapPath );
					}
				}
			}

			Debug.Log( "Creation done: " + cycleName + ", " + identifier  );
			string assetDescriptorLoadPath = ASSETS_DESCRIPTORS_PATH + "/" + cycleName + "/" + identifierReplced;

			return assetDescriptorLoadPath;
		}


		/////////////////////////////////////////////////////////////////////////////
		// Delete Weather Cycle
		private	static	void	DeleteCycle( int idx )
		{
			Weathers cycles = GetWMGR().EDITOR_Cycles;
			EditorUtility.SetDirty( cycles );

			if ( cycles.CyclesPaths[ idx ] == null )
			{
				cycles.CyclesPaths.RemoveAt( idx );
			}
			else
			{
				string assetPath = cycles.CyclesPaths[ idx ]; 
				
				string cycleName = assetPath.Substring( assetPath.LastIndexOf('/') + 1 );
				
				// REMOVE FROM CYCLES LIST
				cycles.CyclesPaths.RemoveAt( idx );
				if ( cycles.LoadedCycles.Count > idx )
					cycles.LoadedCycles.RemoveAt( idx );

				// REMOVE ASSET( DELETE .uasset )
				AssetDatabase.DeleteAsset( assetPath + ".asset" );

				// DELETE SUB-DESCRIPTORS FOLDER
				string descriptorsCyclePath = ASSETS_DESCRIPTORS_PATH + "/" + cycleName + "/";
				if ( System.IO.Directory.Exists( descriptorsCyclePath ) == true )
					System.IO.Directory.Delete( descriptorsCyclePath, true );

				// DELETE META FILE
				string descriptorsCyclePathMetaRef = ASSETS_DESCRIPTORS_PATH + "/" + cycleName + ".meta";
				if ( System.IO.File.Exists( descriptorsCyclePathMetaRef ) )
					System.IO.File.Delete( descriptorsCyclePathMetaRef );
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private	void	OnGUI()
		{
			Weathers cycles = GetWMGR().EDITOR_Cycles;

			if ( cycles == null || cycles.CyclesPaths == null )
			{
				return;
			}

			if ( cycles.CyclesPaths.Count == 0 && GetWMGR().EDITOR_EditModeEnabled == true )
			{
				GetWMGR().EDITOR_EditModeEnabled = false;
				return;
			}

			if ( GUILayout.Button( "Attach" ) )
			{
				if ( GetWMGR(true).EDITOR_EditModeEnabled == false )
					GetWMGR().EDITOR_EditModeEnabled = true;
			}

			if ( GUILayout.Button( "Create cycle" ) )
			{
				WindowValueStep.Init<string>(
					callbackOK: delegate
					{
						string cycleName = WindowValueStep.Value.As<string>();
						string presetFilePath = WindowValueStep.Arg.As<string>();
						string assetPath = CreateCycle( cycleName, presetFilePath );
						if ( assetPath != null )
						{
							// SAVE WHEATHER CYCLE
							EditorUtility.SetDirty( cycles );
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();

							this.Repaint();

							cycles.CyclesPaths.Add( ASSETS_SCRIPTABLES_PATH + "/" + cycleName + ".asset" );
							cycles.LoadedCycles.Add( AssetDatabase.LoadAssetAtPath<WeatherCycle>( assetPath ) );

							if ( GetWMGR().EDITOR_EditModeEnabled == false )
								GetWMGR().EDITOR_EditModeEnabled = true;
						}
					},
					callbackCancel:null,
					optionalArg: delegate
					{
						string path = EditorUtility.OpenFilePanel( "Choose a pre-set file", "", "txt" );
						return path;
					}
				);
			}

			if ( cycles.CyclesPaths.Count == 0 )
				return;


			this.m_ScrollPosition = GUILayout.BeginScrollView(this.m_ScrollPosition );
			{
				GUILayout.BeginVertical();
				{
					for ( int i = 0; i < cycles.CyclesPaths.Count; i++ )
					{
						string weatherCyclePath = cycles.CyclesPaths[ i ];
						string weatherCycleName = System.IO.Path.GetFileNameWithoutExtension( weatherCyclePath );

						GUILayout.BeginHorizontal();
						{
							GUILayout.Label( weatherCycleName );
							if ( GUILayout.Button( "Edit" ) )
							{
								WindowCycleEditor.Init( weatherCyclePath );;
							}

							if ( GUILayout.Button( "Delete" ) )
							{
								DeleteCycle( i-- );
								
								// Set runInEditMode false if no cycle can be used
								if ( cycles.CyclesPaths.Count == 0 && GetWMGR().EDITOR_EditModeEnabled == true )
								{
									GetWMGR().EDITOR_EditModeEnabled = false;
								}
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

			Weathers cycles = GetWMGR().EDITOR_Cycles;
			EditorUtility.SetDirty( cycles );
			AssetDatabase.SaveAssets();

			GetWMGR().EDITOR_EditorLinked = false;

			m_Window = null;
		}
	}

}

#endif