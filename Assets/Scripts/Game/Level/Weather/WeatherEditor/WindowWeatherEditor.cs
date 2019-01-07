using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

namespace WeatherSystem {

	public class WindowWeatherEditor : EditorWindow {

		public const string COLLECTION_FILENAME		= "WeatherCollection";
		public const string CYCLESKIES_PATH			= "Assets/Resources/SkyCubeMaps";
		public const string RESOURCE_PATH			= "Assets/Resources/Scriptables";
		public const string DESCRIPTORS_PATH		= "Assets/Resources/Scriptables/Descriptors";
		public const string RESOURCE_ONLINE_PATH	= "Scriptables/";
		public const string DESCRIPTORS_ONLINE_PATH	= "Scriptables/Descriptors/";

		public	static	WindowWeatherEditor				m_Window				= null;

		private Vector2									m_ScrollPosition		= Vector2.zero;

		private	static	IWeatherManager_Editor			m_WeatherManager		= null;


		public	static	IWeatherManager_Editor	GetWMGR()
		{
			if ( (object)m_WeatherManager == null )
			{
				if ( (object)WeatherManager.Editor == null )
				{
					WeatherManager wmgr = null;
					bool bResult = Utils.Base.GetTemplateSingle( ref wmgr );
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
//				m_Window.Close();
//				m_Window = null;
			}

			Debug.Log("WindowWeatherEditor");

			GetWMGR();
			
			m_Window = EditorWindow.GetWindow<WindowWeatherEditor>( true, "Weather Manager" );
			m_Window.minSize = new Vector2( 400f, 200f );
			m_Window.maxSize = new Vector2( 400f, 600f );

			Setup();
		}


		private	static Weathers GetCycles()
		{
			Weathers cycles = null;
			const string assetPath = RESOURCE_PATH + "/" + COLLECTION_FILENAME + ".asset";
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
		// Setup
		private static	void	Setup()
		{
			// Create directories if not exists
			if ( System.IO.Directory.Exists( RESOURCE_PATH ) == false )
				System.IO.Directory.CreateDirectory( RESOURCE_PATH );

			if ( System.IO.Directory.Exists( CYCLESKIES_PATH ) == false )
				System.IO.Directory.CreateDirectory( CYCLESKIES_PATH );

			if ( System.IO.Directory.Exists( DESCRIPTORS_PATH ) == false )
				System.IO.Directory.CreateDirectory( DESCRIPTORS_PATH );


			// Create or load asset
			Weathers cycles = GetWMGR().INTERNAL_Cycles = GetCycles();

		//	cycles.LoadedCycles = new List<WeatherCycle>();

			if ( cycles.CyclesPaths.Count > 0 )
			{
				GetWMGR().INTERNAL_ForceEnable();
				GetWMGR().INTERNAL_ForceLoadResources();
			}

			GetWMGR().INTERNAL_EditorLinked = true;
		}


		/////////////////////////////////////////////////////////////////////////////
		// Create Weather Cycle
		private	static string	CreateCycle( string cycleName )
		{
			// SKIP IF ALREADY EXISTING
			string assetPath = RESOURCE_PATH + "/" + cycleName + ".asset";
			if ( System.IO.File.Exists( assetPath ) )
				return null;

			// CREATE SUB-DESCRIPTORS FOLDER
			string descriptorsCyclePath = DESCRIPTORS_PATH + "/" + cycleName + "/";
			System.IO.Directory.CreateDirectory( descriptorsCyclePath );

			// CREATE WEATHER CYCLE
			WeatherCycle asset = ScriptableObject.CreateInstance<WeatherCycle>();
			{
				asset.name = cycleName;
				asset.AssetPath = assetPath;
			}
			AssetDatabase.CreateAsset( asset, assetPath );
			EditorUtility.SetDirty( asset );

			// ADD DESCRIPTORS
			for ( int i = 0; i < 24; i++ )
			{
				string onlineLoadPath = CreateDescriptor( cycleName, (float)i );
				asset.DescriptorsPaths[i] = onlineLoadPath;

				EnvDescriptor desc = AssetDatabase.LoadAssetAtPath<EnvDescriptor>( assetPath );
				// Load created asset
				asset.LoadedDescriptors.Add( desc );
			}

			return assetPath;
		}


		// Create Weather Cycle Descriptor
		private static	string	CreateDescriptor( string cycleName, float Hour )
		{
			string cycleSkyiesPath		= CYCLESKIES_PATH + "/" + cycleName + "/";
			string descriptorsCyclePath	= DESCRIPTORS_PATH + "/" + cycleName + "/";

			// IDENTIFIER
			string identifier = "";
			WeatherManager.TransformTime( Hour * 3600f, ref identifier, considerSeconds: false );

			Debug.Log( "Creating descrptor: " + cycleName + ", " + identifier );

			string descriptorAssetPath = descriptorsCyclePath + identifier.Replace( ':', '-' ) + ".asset";

			// CREATE DESCRIPTOR
			EnvDescriptor envDescriptor = ScriptableObject.CreateInstance<EnvDescriptor>();
			{
				envDescriptor.Identifier	= identifier;
				envDescriptor.name			= identifier;
				envDescriptor.AssetPath		= descriptorAssetPath;
				envDescriptor.ExecTime		= Hour * 3600f;
			}
			AssetDatabase.CreateAsset( envDescriptor, descriptorAssetPath );
			EditorUtility.SetDirty( envDescriptor );

			// LOAD SKY CUBE MAP IF PRESENT
			{
				// Ex: Assets/Resources/SkyCubeMaps/Clear/
				if ( System.IO.Directory.Exists( cycleSkyiesPath ) == true )
				{
					// Ex: Assets/Resources/SkyCubeMaps/Clear/00-00.png
					string skyCubeMapPath = cycleSkyiesPath + identifier.Replace( ':', '-' ) + ".png";
					if ( System.IO.File.Exists( skyCubeMapPath ) == true )
					{
						envDescriptor.SkyCubemap = AssetDatabase.LoadAssetAtPath<Cubemap>( skyCubeMapPath );
						Debug.Log( "Cubemap assigned: " + skyCubeMapPath );
					}
				}
			}

			Debug.Log( "Creatione done: " + cycleName + ", " + identifier  );
			string onlineLoadPath = DESCRIPTORS_ONLINE_PATH + cycleName + "/" + identifier.Replace( ':', '-' );

			// RETURN ONLINE LOAD PATH
			return onlineLoadPath;
		}


		/////////////////////////////////////////////////////////////////////////////
		// Delete Weather Cycle
		private	static	void	DeleteCycle( int idx )
		{
			Weathers cycles = GetWMGR().INTERNAL_Cycles;
			EditorUtility.SetDirty( cycles );

			if ( cycles.CyclesPaths[ idx ] == null )
			{
				cycles.CyclesPaths.RemoveAt( idx );
			}
			else
			{
				string assetPath = cycles.CyclesPaths[ idx ];
				
				string cycleName = assetPath.Substring( assetPath.LastIndexOf('/') + 1 );
				
				assetPath = "Assets/Resources/" + assetPath + ".asset";

				// REMOVE FROM CYCLES LIST
				cycles.CyclesPaths.RemoveAt( idx );
				if ( cycles.LoadedCycles.Count > idx )
					cycles.LoadedCycles.RemoveAt( idx );

				// REMOVE ASSET( DELETE .uasset )
				AssetDatabase.DeleteAsset( assetPath );

				// DELETE SUB-DESCRIPTORS FOLDER
				string descriptorsCyclePath = DESCRIPTORS_PATH + "/" + cycleName + "/";
				if ( System.IO.Directory.Exists( descriptorsCyclePath ) == true )
					System.IO.Directory.Delete( descriptorsCyclePath, true );
			}

			if ( cycles.CyclesPaths.Count == 0 && GetWMGR().INTERNAL_EditModeEnabled == true )
				GetWMGR().INTERNAL_EditModeEnabled = false;

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private	void	OnGUI()
		{
			Weathers cycles = GetWMGR().INTERNAL_Cycles;

			if ( cycles == null || cycles.CyclesPaths == null )
			{
				return;
			}

			if ( cycles.CyclesPaths.Count == 0 && GetWMGR().INTERNAL_EditModeEnabled == true )
			{
				GetWMGR().INTERNAL_EditModeEnabled = false;
				return;
			}

			if ( GUILayout.Button( "Create cycle" ) )
			{
				WindowValueStep.Init<string>( delegate
				{
					string cycleName = WindowValueStep.Value.As<string>();
					string assetPath = CreateCycle ( cycleName );
					if ( assetPath != null )
					{
						cycles.CyclesPaths.Add( RESOURCE_ONLINE_PATH + cycleName );

						// SAVE WHEATHER CYCLE
						EditorUtility.SetDirty( cycles );
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();

						this.Repaint();

						if ( cycles.CyclesPaths.Count == 0 && GetWMGR().INTERNAL_EditModeEnabled == false )
							GetWMGR().INTERNAL_EditModeEnabled = true;
					}
				});
			}

			if ( cycles.CyclesPaths.Count == 0 )
				return;

			
			m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition );
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

			Weathers cycles = GetWMGR().INTERNAL_Cycles;

			if ( EditorApplication.isPlaying == false )
			{
				cycles.LoadedCycles.ForEach( delegate( WeatherCycle w )
				{
					w.LoadedDescriptors = new List<EnvDescriptor>(24);
					EditorUtility.SetDirty( w );
				});
				cycles.LoadedCycles = new List<WeatherCycle>();
			}

			EditorUtility.SetDirty( cycles );
			AssetDatabase.SaveAssets();

			GetWMGR().INTERNAL_EditorLinked = false;

			m_Window = null;
		}
	}

}

#endif