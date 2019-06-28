// C# example.
using UnityEditor;

public class Build_Batch 
{
	// BUILD SETTINGS CONFIG FILE PATH
	private	const	string	BuildSettingsConfigFile = "Configs\\BuildSettings";

	private	const	string	MenuLabel = "Build";



	///////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////
	[MenuItem( MenuLabel + "/Development", priority = 1 )]
	public		static		void		Build_Development()
	{
		// BUILD SETTINGS CONFIG SECTION NAME
		const	string	buildSettingsSectionName = "Development";

		// EXECUTABLE FILE NAME
		string folderName = "Development";

		// BUILD FOLDER NAME
		string executableFilename = buildSettingsSectionName;

		string[] scenesToBuild = null;

		// Search for build settings in corresponding section
		if ( GetBuildInfo( buildSettingsSectionName, ref folderName, ref executableFilename, ref scenesToBuild ) == false )
			return;

		// Build Options
		const BuildOptions			buildOptions	= BuildOptions.ForceEnableAssertions;
		
		ExecuteBuild( folderName, buildSettingsSectionName, executableFilename, buildOptions, scenesToBuild );
	}


	///////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////
	[MenuItem( MenuLabel + "/Release", priority = 2 )]
	public		static		void		Build_Release()
	{
		// BUILD SETTINGS CONFIG SECTION NAME
		const	string	buildSettingsSectionName = "Release";

		// EXECUTABLE FILE NAME
		string folderName = "Release";

		// BUILD FOLDER NAME
		string executableFilename = buildSettingsSectionName;

		string[] scenesToBuild = null;

		// Search for build settings in corresponding section
		if ( GetBuildInfo( buildSettingsSectionName, ref folderName, ref executableFilename, ref scenesToBuild ) == false )
			return;

		// Build Options
		const BuildOptions			buildOptions	= BuildOptions.Il2CPP | BuildOptions.StrictMode;
		

		ExecuteBuild( buildSettingsSectionName, folderName, executableFilename, buildOptions, scenesToBuild );
	}


	///////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////
//	[MenuItem( MenuLabel + "/CreateBundle", priority = 3 )]
	public		static		void		CreateBundleExample()
	{
		//Create an array for 2 different prefabs.
		AssetBundleBuild[] buildMap = new AssetBundleBuild[2];
		{
			//Make a buildMap for the first prefab.
			const string bundle1Name = "Bullets";
			AssetBundleBuild buildInfo1 = new AssetBundleBuild()
			{
				assetBundleName = bundle1Name+".unity3d",
				assetNames = new string[] {
					"Assets/Resources/Prefabs/Bullets.prefab"
				}
			};
			buildMap[0] = buildInfo1;

			//Make a buildMap for the second prefab.
			const string bundle2Name = "GameManager";
			AssetBundleBuild buildInfo2 = new AssetBundleBuild()
			{
				assetBundleName = bundle2Name+".unity3d",
				assetNames = new string[] {
					"Assets/Resources/Prefabs/Essentials/InGame/GameManager.prefab"
				}
			};
			buildMap[1] = buildInfo2;
		}

		const string bundleRelativePath = "Assets/Bundles/FirstBandle";

		EnsureCreatedAndEmptyFolder( bundleRelativePath );

		//Save the prefabs as bundles to the "bundleFolder" path.
		UnityEngine.AssetBundleManifest a = BuildPipeline.BuildAssetBundles( bundleRelativePath, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64 );

		foreach( var b in a.GetAllAssetBundles() )
		{
			UnityEngine.Debug.Log( b );
		}
	}


	//////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////
	private		static		void		ExecuteBuild( string buildSettingsSectionName, string folderName, string executableFilename, BuildOptions buildOptions, string[] scenesToBuild )
	{
		// Get project folder
		string projectFolder = GetProjectFolder();

		string relativeBuildFolder		= projectFolder + "/Builds/" + folderName + "/";

		string executableRelativePath	= relativeBuildFolder + "Build_" + executableFilename + ".exe";

		const BuildTarget			buildTarget				= BuildTarget.StandaloneWindows64;
		const BuildTargetGroup		targetGroup				= BuildTargetGroup.Standalone;

		// Project Folder absolute path
		UnityEngine.Debug.Log( "Project Folder: " + projectFolder );

		// build folder relative path
		UnityEngine.Debug.Log( "Build Folder: " + relativeBuildFolder );

		// built executable relative path
		UnityEngine.Debug.Log( "Built Executable: " + executableRelativePath );

		EnsureCreatedAndEmptyFolder( relativeBuildFolder );

		BuildPlayerOptions options = new BuildPlayerOptions()
		{
			locationPathName	= executableRelativePath,			// The path where the application will be built.
			options				= buildOptions,						// Additional BuildOptions, like whether to run the built player.
			scenes				= scenesToBuild,					// The scenes to be included in the build. If empty, the currently open scene will be built. Paths are relative to the project folder "Assets/Scenes/Intro.unity".
			target				= buildTarget,						// The BuildTarget to build.
			targetGroup			= targetGroup						// The BuildTargetGroup to build.
		};
		
		// Execute build
		string errorMessage = BuildPipeline.BuildPlayer( options );

		if ( errorMessage.Length > 0 )
		{
			System.IO.File.WriteAllText( relativeBuildFolder + "BuildError.log", errorMessage );
		}
	}


	///////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////
	private		static		void		EnsureCreatedAndEmptyFolder( string relativePath )
	{
		if ( relativePath.EndsWith( "/" ) == false )
			relativePath += '/';

		// Ensure for empty folder
		if ( System.IO.Directory.Exists( relativePath ) )
		{
			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo( relativePath );
			foreach ( System.IO.FileInfo file in di.GetFiles() )
			{
				file.Delete(); 
			}
			foreach ( System.IO.DirectoryInfo dir in di.GetDirectories() )
			{
				dir.Delete( true );
			}
		}
		else
		{
			System.IO.Directory.CreateDirectory( relativePath );
		}
	}


	///////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////
	private		static		string		GetProjectFolder()
	{
		string assetFolderRelativePath = UnityEngine.Application.dataPath;
		int slashIdx = assetFolderRelativePath.LastIndexOf( '/' );
		string projectFolder = assetFolderRelativePath.Remove( slashIdx );
		return projectFolder;
	}


	///////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////////////////////////////////////////////////
	private		static		bool		GetBuildInfo( string buildSettingsSectionName, ref string folderName, ref string executableFilename, ref string[] scenesToBuild )
	{
		// Search for build settings in corresponding section
		SectionMap buildSettings = new SectionMap();
		if ( buildSettings.LoadFile( BuildSettingsConfigFile ) == true )
		{
			Database.Section buildSettingsSection = null;
			if ( buildSettings.bGetSection( buildSettingsSectionName, ref buildSettingsSection ) == false )
			{
				UnityEngine.Debug.LogError( "Cannot load build settings section for " + buildSettingsSectionName );
				buildSettings = null;
				return false;
			}

			folderName			= buildSettingsSection.AsString( "FolderName", folderName );
			executableFilename	= buildSettingsSection.AsString( "ExecutableFilename", executableFilename );

			return buildSettingsSection.bGetMultiAsArray( "ScenesToBuild", ref scenesToBuild );
		}
		return false;
	}
}