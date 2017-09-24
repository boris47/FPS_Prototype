using System;
using System.Collections .Generic;
using System.Linq;
using System.Text;
using UnityEngine;



namespace Utils {

	public static class System {

		public static bool FileExists( string FilePath ) {
			return global::System.IO.File.Exists( FilePath );
		}

		public static bool DirExists( string DirPath ) {
			return global::System.IO.Directory.Exists( DirPath );
		}

		public static string GetPathFromFilePath( string FilePath, bool bSlash = true ) {
			return FilePath.Substring( 0, FilePath.LastIndexOf( "\\" ) + ( ( bSlash ) ? 1 : 0 ) );
		}

		public static string RemoveExtension( string FilePath ) {
			return FilePath.Substring( 0, FilePath.LastIndexOf( "." ) );
		}

	}

}
