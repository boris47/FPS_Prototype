using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace Utils {

	public static class MySystem {

		public	static	bool	FileExistsNoExt( string filePath )
		{
			string [] files = System.IO.Directory.GetFiles( System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileNameWithoutExtension( filePath ) + "*");
			return files.Length > 0;
		}


		public static string GetPathFromFilePath( string FilePath, bool bSlash = true )
		{
			return FilePath.Substring( 0, FilePath.LastIndexOf( "\\" ) + ( ( bSlash ) ? 1 : 0 ) );
		}

		public static string CombinePaths(params string[] paths)
		{
			return paths.Aggregate( global::System.IO.Path.Combine );
		}
	}

}
