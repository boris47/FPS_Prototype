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

	}
		

}