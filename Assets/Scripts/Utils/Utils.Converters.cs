using UnityEngine;
using System.Collections;


namespace Utils {

	public static  class Converters {

		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Accept a string and return the parsed result as enum value
		/// </summary>
		public	static	bool	StringToEnum<T>( string s, ref T e, bool ignoreCase = true )
		{
			if ( s.Length == 0 )
				return false;

			if ( typeof( T ).IsEnum == false )
				return false;

			e = ( T ) global::System.Enum.Parse( typeof( T ), s, ignoreCase: true );			
			return true;
		}




		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Accept a ref Color and return read result
		/// </summary>
		public	static	bool	StringToColor( string s, ref Color c, float Alpha = 1.0f )
		{
			if ( s.Length == 0 )
				return false;

			s.TrimStart(); s.Trim(); s.TrimEnd();

			string[] sArray = s.Split( ',' );
			if ( sArray.Length < 3 )
				return false;
			
			c.r = float.Parse( sArray[0] );
			c.g = float.Parse( sArray[1] );
			c.b = float.Parse( sArray[2] );
			c.a = Alpha;
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		///  Accept a ref Vector and return read result
		/// </summary>
		public	static	bool	StringToVector( string s, ref Vector3 v )
		{
			if ( s.Length == 0 )
				return false;

			s.TrimStart(); s.Trim(); s.TrimEnd();

			string[] sArray = s.Split( ',' );
			if ( sArray.Length < 3 )
				return false;

			v.Set( float.Parse( sArray[0] ), float.Parse( sArray[1] ), float.Parse( sArray[2] ) ); 
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		///  Accept a ref Quaternion and return read result
		/// </summary>
		public	static	bool	StringToQuaternion( string s, ref Quaternion q )
		{
			if ( s.Length == 0 )
				return false;

			s.TrimStart(); s.Trim(); s.TrimEnd();

			string[] sArray = s.Split( ',' );
			if ( sArray.Length < 4 )
				return false;

			q.Set( float.Parse( sArray[0] ), float.Parse( sArray[1] ), float.Parse( sArray[2] ), float.Parse( sArray[3] ) );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		///  Return a stringified version of a vector ( Used in Save & Load )
		/// </summary>
		public	static	string	Vector3ToString( Vector3 vector )
		{
			return vector.x + ", " + vector.y + ", " + vector.z;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a stringified version of a quaternion ( Used in Save & Load )
		/// </summary>
		public	static	string	QuaternionToString( Quaternion quaternion )
		{
			return quaternion.x + ", " + quaternion.y + ", " + quaternion.z + ", " + quaternion.w;
		}


	}

}