using UnityEngine;
using System.Collections;


namespace Utils {

	public static  class Converters {

		public static bool StringToColor( string s, ref Color c )
		{
			if ( s.Length == 0 )
				return false;

			s.Trim();
			s.TrimStart();
			s.TrimEnd();

			string[] sArray = s.Split( ',' );
			if ( sArray.Length < 3 )
				return false;

			Color result = new Color (
				float.Parse(sArray[0]),
				float.Parse(sArray[1]),
				float.Parse(sArray[2])
			);

			c = result;
			return true;
		}

		public static bool StringToVector( string s, ref Vector3 v )
		{
			if ( s.Length == 0 )
				return false;

			s.Trim();
			s.TrimStart();
			s.TrimEnd();

			string[] sArray = s.Split( ',' );
			if ( sArray.Length < 3 )
				return false;

			Vector3 result = new Vector3 (
				float.Parse(sArray[0]),
				float.Parse(sArray[1]),
				float.Parse(sArray[2])
			);

			v = result;
			return true;
		}
	}


}