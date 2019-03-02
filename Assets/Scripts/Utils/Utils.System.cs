using System;
using System.Collections .Generic;
using System.Linq;
using System.Text;
using UnityEngine;



namespace Utils {

	public static class System {

		public static string GetPathFromFilePath( string FilePath, bool bSlash = true )
		{
			return FilePath.Substring( 0, FilePath.LastIndexOf( "\\" ) + ( ( bSlash ) ? 1 : 0 ) );
		}
	}

}
