using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace Utils {

	public static class System {

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
