using System;
using System.Globalization;
using UnityEngine;


namespace Utils {

	public static  class Converters {

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Accept a string and return the parsed result as enum value </summary>
		public	static	bool	StringToEnum<T>( string s, out T enumValue, bool ignoreCase = true ) where T : struct
		{
			return global::System.Enum.TryParse( s, true, out enumValue);			
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Accept a string and return the parsed result as enum value </summary>
		public	static	bool	StringToEnum( string s, System.Type t, out object e, bool ignoreCase = true )
		{
			e = null;
			if (s.Length > 0 && t.IsEnum)
			{
				e = global::System.Enum.Parse(t, s, ignoreCase: true);
				return true;
			}
			return false;
		}




		//////////////////////////////////////////////////////////////////////////
		/// <summary> Accept a ref Color and return read result </summary>
		public	static	bool	StringToColor( string s, out Color c, float Alpha = 0.0f )
		{
			c = Color.clear;
			if ( s.Length > 0 )
			{
				string[] sArray = s.TrimStart().TrimInside().TrimEnd().Split(',');
				if (sArray.Length >= 3)
				{
					if (float.TryParse(sArray[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float r) &&
						float.TryParse(sArray[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float g) &&
						float.TryParse(sArray[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float b))
					{
						c.r = r; c.g = g; c.b = b;
						c.a = Alpha > 0.0f ? Alpha : (sArray.Length > 3 && float.TryParse(sArray[3], out float a)) ? a : 1.0f;
						return true;
					}
				}
			}
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		///  Accept a ref Vector and return read result
		/// </summary>
		public	static	bool	StringToVector3( in string s, out Vector3 v )
		{
			v = Vector3.zero;
			if ( s.Length > 0 )
			{
				string[] sArray = s.TrimStart().TrimInside().TrimEnd().Split(',');
				if (sArray.Length >= 3)
				{
					if (float.TryParse(sArray[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
						float.TryParse(sArray[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y) &&
						float.TryParse(sArray[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float z))
					{
						v.Set(x, y, z);
						return true;
					}
				}
			}
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		///  Accept a ref Quaternion and return read result
		/// </summary>
		public	static	bool	StringToQuaternion( string s, out Quaternion q )
		{
			q = Quaternion.identity;
			if ( s.Length > 0 )
			{
				string[] sArray = s.TrimStart().TrimInside().TrimEnd().Split(',');
				if (sArray.Length >= 4)
				{
					if (float.TryParse(sArray[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
						float.TryParse(sArray[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y) &&
						float.TryParse(sArray[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float z) &&
						float.TryParse(sArray[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float w))
					{
						q.Set(x, y, z, w);
						return true;
					}
				}
			}
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a stringified version of a vector ( Used in Save & Load ) </summary>
		public static string Vector2ToString(Vector2 vector)
		{
			return $"{vector.x}, {vector.y}";
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a stringified version of a vector ( Used in Save & Load ) </summary>
		public	static	string	Vector3ToString(Vector3 vector)
		{
			return $"{vector.x}, {vector.y}, {vector.z}";
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  Return a stringified version of a quaternion ( Used in Save & Load ) </summary>
		public	static	string	QuaternionToString( Quaternion quaternion )
		{
			return $"{quaternion.x}, {quaternion.y}, {quaternion.z}, {quaternion.w}";
		}
	}
}