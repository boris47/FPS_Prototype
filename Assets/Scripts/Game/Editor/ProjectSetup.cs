using System.Linq;
using System.IO;

#if UNITY_EDITOR_WIN
using Microsoft.Win32;
#endif
using UnityEngine;
using UnityEditor;

namespace ProjectSetup
{
	[InitializeOnLoad]
	public class OnEditorStartup
	{
		static OnEditorStartup()
		{
			string editorProjectFolderPath = Path.GetDirectoryName(Application.dataPath);
			string gitFolderPath = Path.Combine(editorProjectFolderPath, ".git");
			string gifConfigFilePath = Path.Combine(gitFolderPath, "config");

			if (File.Exists(gifConfigFilePath))
			{
				// Read a text file line by line.  
				string[] lines = File.ReadAllLines(gifConfigFilePath);

				if (!lines.Any(l => l.Contains("p4merge.exe")) && TryGetPerforceFolderPath(out string OutPerforceFolder))
				{
					string[] toolsInstructions = new string[]
					{
						"\n",
						"[diff]",
						"	tool = p4diff",
						"[difftool \"p4diff\"]",
					   $"	cmd = '{OutPerforceFolder}p4merge.exe' \\\"$LOCAL\\\" \\\"$REMOTE\\\"",

						"[merge]",
						"	tool = p4merge",
						"[difftool \"p4merge\"]",
					   $"	cmd = '{OutPerforceFolder}p4merge.exe' \\\"$BASE\\\" \\\"$LOCAL\\\" \\\"$REMOTE\\\" \\\"$MERGED\\\"",
						"	trustExitCode = true"
					};
					File.WriteAllLines(gifConfigFilePath, lines.Concat(toolsInstructions));
				}
			}
		}


		//////////////////////////////////////////////////////////////////
		public static bool TryGetPerforceFolderPath(out string OutPath)
		{
			OutPath = null;
			{
#if UNITY_EDITOR_WIN
				// Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{2B1F805D-E677-4239-90E3-C47A5D1F0E67}
				try
				{
				// Not consistent
				//	using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{2B1F805D-E677-4239-90E3-C47A5D1F0E67}"))
				//	{
				//		if (key.IsNotNull())
				//		{
				//			System.Object o = key.GetValue("InstallLocation");
				//			if (o.IsNotNull())
				//			{
				//				OutPath = o as System.String;
				//			}
				//		}
				//	}

					if (OutPath == null)
					{
						using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Perforce\\Environment"))
						{
							if (key.IsNotNull())
							{
								System.Object o = key.GetValue("P4INSTROOT");
								if (o.IsNotNull())
								{
									OutPath = o as System.String;
								}
							}
						}
					}
				}
				catch (System.Exception) {}
#endif
			}
			return OutPath.IsNotNull();
		}
	}

	class INTERNAL
	{
		[MenuItem("APIExamples/GetDependencies")]
		static void GetAllDependenciesForScenes()
		{
			var allScenes = AssetDatabase.FindAssets("t:Scene");
			string[] allPaths = new string[allScenes.Length];
			int curSceneIndex = 0;

			foreach (var guid in allScenes)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				allPaths[curSceneIndex] = path;
				++curSceneIndex;
			}

			var dependencies = AssetDatabase.GetDependencies(allPaths);

			System.Text.StringBuilder dependenciesString = new();
			dependenciesString.AppendLine();

			foreach (var curDependency in dependencies)
			{
				dependenciesString.Append(curDependency);
				dependenciesString.AppendLine();
			}

			Debug.Log("All dependencies for Scenes in Project: " + dependenciesString);
		}
	}
}