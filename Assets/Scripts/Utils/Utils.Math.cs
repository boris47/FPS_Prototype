using System;
using System.Collections .Generic;
using System.Linq;
using System.Text;
using UnityEngine;



namespace Utils {

	public static class Math {

		public const float EPS = 0.0000100f;

		public	static	bool	SimilarZero( float a, float cmp )
		{
			return Mathf.Abs( a ) < cmp;
		}

		public	static	float Clamp( float Value, float Min, float Max ) {

			if ( Value > Max ) Value = Max;
			if ( Value < Min ) Value = Min;
			return Value;

		}

		public	static	float ClampAngle( float Angle, float Min, float Max ) {

			while ( Angle > 360 )
				Angle =-360;

			Angle = Mathf.Max ( Mathf.Min ( Angle, Max ), Min );

			if ( Angle < 0 )
				Angle += 360;

			return Angle;

		}
		/*
			sun_dir.setHP(
			deg2rad(config.r_float(m_identifier.c_str(), "sun_altitude")),
			deg2rad(config.r_float(m_identifier.c_str(), "sun_longitude"))
			);
		*/

		public static Vector3 VectorByHP( float h, float p )
		{
			h *= Mathf.Deg2Rad;
			p *= Mathf.Deg2Rad;
			float _ch = Mathf.Cos( h );
			float _cp = Mathf.Cos( p );
			float _sh = Mathf.Sin( h );
			float _sp = Mathf.Sin( p );
			return new Vector3 ( _cp*_sh, _sp, _cp*_ch );
		}
	}
		

}