// C# example.
using UnityEditor;

public class Build_Batch 
{

	// EXECUTABLE FILE NAME
	private const string FolderName = "Build";

	// BUILD FOLDER NAME
	private const string ExeName = "TestBuild";



	[MenuItem("MyTools/Custom Build")]
	public static void BuildGame ()
	{
		// Get project folder
		string projectFolder = GetProjectFolder();
		UnityEngine.Debug.Log( "Project Folder: " + projectFolder );

		// build folder relative path
		string relativeBuildFolder = projectFolder + "/" + FolderName + "/";
		System.IO.Directory.CreateDirectory( relativeBuildFolder );
		UnityEngine.Debug.Log( "Build Folder: " + relativeBuildFolder );

		// built executable relative path
		string executableRelativePath = projectFolder + "/" + FolderName + "/" + ExeName + ".exe";
		UnityEngine.Debug.Log( "Built Executable: " + executableRelativePath );

		// Build Options
		const BuildOptions			buildOptions	= BuildOptions.Il2CPP | BuildOptions.ForceEnableAssertions | BuildOptions.StrictMode;
		const BuildTarget			buildTarget		= BuildTarget.StandaloneWindows64;
		const BuildTargetGroup		targetGroup		= BuildTargetGroup.Standalone;

		// Scene path array to include and use inside the build
		// IMPORTANT: MIND THE ORDER
		string[] levels =
		{
			"Assets/Scenes/Intro.unity",
			"Assets/Scenes/MainMenu.unity",
			"Assets/Scenes/OpenWorld.unity",
			"Assets/Scenes/OpenWorld2.unity",
			"Assets/Scenes/OpenWorld3.unity",
		};

		BuildPlayerOptions options = new BuildPlayerOptions()
		{
			locationPathName	= executableRelativePath,			// The path where the application will be built.
			options				= buildOptions,						// Additional BuildOptions, like whether to run the built player.
			scenes				= levels,							// The scenes to be included in the build. If empty, the currently open scene will be built. Paths are relative to the project folder "Assets/Scenes/Intro.unity".
			target				= buildTarget,						// The BuildTarget to build.
			targetGroup			= targetGroup						// The BuildTargetGroup to build.
		};
		
		// Build player.
		BuildPipeline.BuildPlayer( options );
	}


	//////////////////////////////////////////////////////////
	private	static	string	GetProjectFolder()
	{
		string assetFolderRelativePath = UnityEngine.Application.dataPath;
		int slashIdx = assetFolderRelativePath.LastIndexOf( '/' );
		string projectFolder = assetFolderRelativePath.Remove( slashIdx );
		return projectFolder;
	}
}