
using UnityEngine;



namespace Utils {

	public static class Math {

		public const float EPS = 0.00001f;

		public	static	float		Sign( float value )
		{
			return ( value > 0f ) ? 1f : ( value < 0f ) ? -1f : 0f;
		}


		public	static	float		Sqr( float value )
		{
			return Mathf.Pow( value, 0.5f );
		}


		public	static	bool		SimilarZero( float a, float cmp = EPS )
		{
			return Mathf.Abs( a ) < cmp;
		}


		public	static	float		Clamp( float Value, float Min, float Max )
		{
			return ( Value > Max ) ? Max : ( Value < Min ) ? Min : Value;
		}


		public	static	float		ClampAngle( float Angle, float Min, float Max )
		{
			while ( Angle > 360 )
				Angle =-360;

			Angle = Mathf.Max ( Mathf.Min ( Angle, Max ), Min );
			if ( Angle < 0 )
				Angle += 360;

			return Angle;
		}

		/// <summary>
		/// Get planar squared distance between two positions, position1 is projected on position2 plane
		/// </summary>
		/// <returns>Planar Squared Distance</returns>
		public	static	float		PlanarSqrDistance( Vector3 position1, Vector3 position2, Vector3 position2PlaneNormal )
		{
			// with given plane normal, project position1 on position2 plane
			Vector3 projectedPoint = ProjectPointOnPlane( position2PlaneNormal, position1, position2 );

			return ( position2 - projectedPoint ).sqrMagnitude;
		}

		/// <summary>
		/// Get planar distance between two positions, position1 is projected on position2 plane
		/// </summary>
		/// <returns>Planar Distance</returns>
		public	static	float		PlanarDistance( Vector3 position1, Vector3 position2, Vector3 planeNormal )
		{
			float sqrDistance = PlanarSqrDistance( position1, position2, planeNormal );
			
			return Mathf.Sqrt( sqrDistance );
		}

		public	static	Vector3		VectorByHP( float h, float p )
		{
			h *= Mathf.Deg2Rad;
			p *= Mathf.Deg2Rad;
			float _ch = Mathf.Cos( h );
			float _cp = Mathf.Cos( p );
			float _sh = Mathf.Sin( h );
			float _sp = Mathf.Sin( p );
			return new Vector3 ( _cp * _sh, _sp, _cp * _ch );
		}


		//create a vector of direction "vector" with length "size"
		public	static	Vector3		SetVectorLength( Vector3 vector, float size )
		{
			//normalize the vector
			Vector3 vectorNormalized = Vector3.Normalize(vector);

			//scale the vector
			return vectorNormalized *= size;
		}


		//This function returns a point which is a projection from a point to a plane.
		public	static	Vector3		ProjectPointOnPlane( Vector3 planeNormal, Vector3 planePoint, Vector3 point )
		{
			//First calculate the distance from the point to the plane:
			float distance = Vector3.Dot( planeNormal, ( point - planePoint ) );

			//Reverse the sign of the distance
			distance *= -1;

			//Get a translation vector
			Vector3 translationVector = SetVectorLength( planeNormal, distance );

			//Translate the point to form a projection
			return point + translationVector;
		}


		public	static	Vector3		GetPoint( Vector3 p0, Vector3 p1, Vector3 p2, float t )
		{
			t = Mathf.Clamp01( t );
			float oneMinusT = 1f - t;
			return
						oneMinusT	*	oneMinusT	*	p0	+
				2f	*	oneMinusT	*		t		*	p1 +
							t		*		t		*	p2;
		}


		public	static	Vector3		GetPoint( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t )
		{
			t = Mathf.Clamp01( t );
			float OneMinusT = 1f - t;
			return
						OneMinusT	*	OneMinusT	*	OneMinusT	*	p0 +
				3f	*	OneMinusT	*	OneMinusT	*		t		*	p1 +
				3f	*	OneMinusT	*		t		*		t		*	p2 +
				t	*		t		*		t		*		p3;
		}


		public	static	Vector3		GetPoint( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t )
		{
			t = Mathf.Clamp01( t );
			float OneMinusT = 1f - t;
			return
						OneMinusT	*	OneMinusT	*	OneMinusT	*	OneMinusT	*	p0 +
				4f *	OneMinusT	*	OneMinusT	*	OneMinusT	*		t		*	p1 +
				5f *	OneMinusT	*	OneMinusT	*		t		*		t		*	p2 +
				4f *	OneMinusT	*		t		*		t		*		t		*	p3 +
							t		*		t		*		t		*		t		*	p4;
		}


		public	static	Vector3		GetPoint( Vector3[] points, float t )
		{
			int numSections = points.Length - 3;
			int currPt = Mathf.Min( Mathf.FloorToInt( t * ( float ) numSections ), numSections - 1 );
			float u = t * ( float ) numSections - ( float ) currPt;
		
			Vector3 a = points[ currPt + 0 ];
			Vector3 b = points[ currPt + 1 ];
			Vector3 c = points[ currPt + 2 ];
			Vector3 d = points[ currPt + 3 ];
		
			return .5f * 
			(
				( -a + 3f * b - 3f * c + d )		* ( u * u * u ) +
				( 2f * a - 5f * b + 4f * c - d )	* ( u * u ) +
				( -a + c )							* u +
				2f * b
			);
		}

	}
		

}