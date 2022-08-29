using System.Linq;
using System.IO;

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

				if (!lines.Any(l => l.Contains("p4merge.exe")))
				{
					string[] toolsInstructions = new string[]
					{
						"\n",
						"[diff]",
						"	tool = p4diff",
						"[difftool \"p4diff\"]",
						$"	cmd = 'C:/Program Files/Perforce/p4merge.exe' \\\"$LOCAL\\\" \\\"$REMOTE\\\"",

						"[merge]",
						"	tool = p4merge",
						"[difftool \"p4merge\"]",
						"	cmd = 'C:/Program Files/Perforce/p4merge.exe' \\\"$BASE\\\" \\\"$LOCAL\\\" \\\"$REMOTE\\\" \\\"$MERGED\\\"",
						"	trustExitCode = true"
					};
					File.WriteAllLines(gifConfigFilePath, lines.Concat(toolsInstructions));
				}
			}
		}
	}
}