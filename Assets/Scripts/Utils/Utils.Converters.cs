using UnityEngine;
using System.Collections;


namespace Utils {

	public static  class Converters {


		public	static	bool	StringToColor( string s, ref Color c )
		{
			if ( s.Length == 0 )
				return false;

			s.Trim();
			s.TrimStart();
			s.TrimEnd();

			string[] sArray = s.Split( ',' );
			if ( sArray.Length < 3 )
				return false;

			c.r = float.Parse( sArray[0] );
			c.g = float.Parse( sArray[1] );
			c.b = float.Parse( sArray[2] );
			return true;
		}


		public	static	bool	StringToVector( string s, ref Vector3 v )
		{
			if ( s.Length == 0 )
				return false;

			s.Trim();
			s.TrimStart();
			s.TrimEnd();

			string[] sArray = s.Split( ',' );
			if ( sArray.Length < 3 )
				return false;

			v.x = float.Parse( sArray[0] );
			v.y = float.Parse( sArray[1] );
			v.z = float.Parse( sArray[2] );
			return true;
		}


		public	static	bool	StringToQuaternion( string s, ref Quaternion q )
		{
			if ( s.Length == 0 )
				return false;

			s.Trim();
			s.TrimStart();
			s.TrimEnd();

			string[] sArray = s.Split( ',' );
			if ( sArray.Length < 4 )
				return false;

			q.x = float.Parse( sArray[0] );
			q.y = float.Parse( sArray[1] );
			q.z = float.Parse( sArray[2] );
			q.w = float.Parse( sArray[3] );
			return true;
		}


		public	static	string	Vector3ToString( Vector3 vector )
		{
			return vector.x + ", " + vector.y + ", " + vector.z;
		}


		public	static	string	QauternionToString( Quaternion quaternion )
		{
			return quaternion.x + ", " + quaternion.y + ", " + quaternion.z + ", " + quaternion.w;
		}


	}

}