
using UnityEngine;



namespace Utils {

	public static class Math {

		public	const	float	EPS		= 0.00001f;


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Equal to Mathf.Sign but this works
		/// </summary>
		public	static		float		Sign( float value )
		{
			return ( value > 0f ) ? 1f : ( value < 0f ) ? -1f : 0f;
		}


		/// <summary>
		/// Return true if the value lays betweeen bound values, otherwise return false
		/// </summary>
		/// <param name="Value"></param>
		/// <param name="Val1"></param>
		/// <param name="Val2"></param>
		/// <returns></returns>
		public	static		bool		BetweenValues( float Value, float Val1, float Val2 )
		{
			float minBound = Mathf.Min( Val1, Val2 );
			float maxBound = Mathf.Max( Val1, Val2 );

			return Value > minBound && Val1 < maxBound;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a better performance method to get squared value
		public	static		float		Sqr( float value )
		{
			return Mathf.Pow( value, 0.5f );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// With a facoltative Epsilon, determines if value is similar to Zero
		/// </summary>
		public	static		bool		SimilarZero( float a, float cmp = EPS )
		{
			return Mathf.Abs( a ) < cmp;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a clamped value
		/// </summary>
		public	static		float		Clamp( float Value, float Min, float Max )
		{
			return ( Value > Max ) ? Max : ( Value < Min ) ? Min : Value;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a clamped angle
		/// </summary>
		public	static		float		ClampAngle( float Angle, float Min, float Max )
		{
			while ( Angle > 360 )
				Angle =-360;

			Angle = Mathf.Max ( Mathf.Min ( Angle, Max ), Min );
			if ( Angle < 0 )
				Angle += 360;

			return Angle;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get planar squared distance between two positions, position1 is projected on position2 plane
		/// </summary>
		/// <returns>Planar Squared Distance</returns>
		public	static		float		PlanarSqrDistance( Vector3 position1, Vector3 position2, Vector3 position2PlaneNormal )
		{
			// with given plane normal, project position1 on position2 plane
			Vector3 projectedPoint = ProjectPointOnPlane( position2PlaneNormal, position1, position2 );

			return ( position2 - projectedPoint ).sqrMagnitude;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get planar distance between two positions, position1 is projected on position2 plane
		/// </summary>
		/// <returns>Planar Distance</returns>
		public	static		float		PlanarDistance( Vector3 position1, Vector3 position2, Vector3 planeNormal )
		{
			float sqrDistance = PlanarSqrDistance( position1, position2, planeNormal );
			
			return Mathf.Sqrt( sqrDistance );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get a direction vector from polar coordinates
		/// </summary>>
		public	static		Vector3		VectorByHP( float h, float p )
		{
			h *= Mathf.Deg2Rad;
			p *= Mathf.Deg2Rad;
			float _ch = Mathf.Cos( h );
			float _cp = Mathf.Cos( p );
			float _sh = Mathf.Sin( h );
			float _sp = Mathf.Sin( p );
			return new Vector3 ( _cp * _sh, _sp, _cp * _ch );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return if a position is inside a mesh
		/// </summary>
		public	static		bool		IsPointInside( MeshFilter MeshFilter, Vector3 WorldPosition )
		{
			Mesh aMesh = MeshFilter.sharedMesh;
			Vector3 aLocalPoint = MeshFilter.transform.InverseTransformPoint(WorldPosition);
			Plane plane = new Plane();
			
			var verts = aMesh.vertices;
			var tris = aMesh.triangles;
			int triangleCount = tris.Length / 3;
			for ( int i = 0; i < triangleCount; i++ )
			{
				var V1 = verts[ tris[ i * 3 ] ];
				var V2 = verts[ tris[ i * 3 + 1 ] ];
				var V3 = verts[ tris[ i * 3 + 2 ] ];
				plane.Set3Points( V1, V2, V3 );
				if ( plane.GetSide( aLocalPoint ) )
					return false;
			}
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Create a vector of direction "vector" with length "size"
		/// </summary>
		public	static		Vector3		SetVectorLength( Vector3 vector, float size )
		{
			//normalize the vector
			Vector3 vectorNormalized = Vector3.Normalize(vector);

			//scale the vector
			return vectorNormalized *= size;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function returns a point which is a projection from a point to a plane.
		/// </summary>
		public	static		Vector3		ProjectPointOnPlane( Vector3 planeNormal, Vector3 planePoint, Vector3 point )
		{
			//First calculate the distance from the point to the plane:
//			float distance = Vector3.Dot( planeNormal, ( point - planePoint ) );

			//Reverse the sign of the distance
//			distance *= -1;

			//Get a translation vector
//			Vector3 translationVector = SetVectorLength( planeNormal, distance );

			//Translate the point to form a projection
//			return point - translationVector;
			
			// Dot product of two normalize vector means the cos of the angle between this two vectors
			// If it's positive means a < 180 angle and negative and angle >= 180
			// Dot product can also be: ( ax × bx ) + ( ay × by ), that's the point
			float pointPlaneDistance = Vector3.Dot( planeNormal, point - planePoint );

			return point - ( planeNormal * pointPlaneDistance );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// First-order intercept using absolute target position
		/// </summary>
		public	static		Vector3		CalculateBulletPrediction( Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity )
		{
			Vector3 targetRelativePosition = targetPosition - shooterPosition;
			Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
			float t = FirstOrderInterceptTime
			(
				shotSpeed:				shotSpeed,
				targetRelativePosition:	targetRelativePosition,
				targetRelativeVelocity:	targetRelativeVelocity
			);
			return targetPosition + t * ( targetRelativeVelocity );
		}

		//first-order intercept using relative target position
		public	static		float		FirstOrderInterceptTime( float shotSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity )
		{
			float velocitySquared = targetRelativeVelocity.sqrMagnitude;
			if ( velocitySquared < 0.001f )
				return 0f;

			float a = velocitySquared - shotSpeed * shotSpeed;

			//handle similar velocities
			if ( Mathf.Abs( a ) < 0.001f )
			{
				float t = -targetRelativePosition.sqrMagnitude / ( 2f * Vector3.Dot( targetRelativeVelocity, targetRelativePosition ) );
				return Mathf.Max( t, 0f ); //don't shoot back in time
			}

			float b = 2f * Vector3.Dot( targetRelativeVelocity, targetRelativePosition );
			float c = targetRelativePosition.sqrMagnitude;
			float determinant = b * b - 4f * a * c;

			// First assignment: Determinant == 0; one intercept path, pretty much never happens
			float result = Mathf.Max( -b / ( 2f * a ), 0f ); //don't shoot back in time

			if ( determinant > 0f )
			{	//	Determinant > 0; two intercept paths (most common)
				float t1 = ( -b + Mathf.Sqrt( determinant ) ) / ( 2f * a );
				float t2 = ( -b - Mathf.Sqrt( determinant ) ) / ( 2f * a );
				if ( t1 > 0f )
				{
					if ( t2 > 0f )
						result = Mathf.Min( t1, t2 ); //both are positive
					else
						result = t1; //only t1 is positive
				}
				else
				{
					result = Mathf.Max( t2, 0f ); //don't shoot back in time
				}
			}
			
			//determinant < 0; no intercept path
			if ( determinant < 0f ) 
			{
				result = 0f;
			}

			return result;
		}
		
		
		// https://unity3d.college/2017/06/30/unity3d-cannon-projectile-ballistics/
		//////////////////////////////////////////////////////////////////////////
		public	static		Vector3		BallisticVelocity( Vector3 startPosition, Vector3 destination, float angle )
		{
			Vector3 dir = destination - startPosition;				// get Target Direction
			float height = dir.y;									// get height difference
			dir.y = 0;												// retain only the horizontal difference
			float dist = dir.magnitude;								// get horizontal direction
			float a = angle * Mathf.Deg2Rad;						// Convert angle to radians
			dir.y = dist * Mathf.Tan(a);							// set dir to the elevation angle.
			dist += height / Mathf.Tan(a);							// Correction for small height differences

			// Calculate the velocity magnitude
			float velocity = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
			return velocity * dir;						// Return a normalized vector.
		}


		//////////////////////////////////////////////////////////////////////////
		public	static		float		CalculateFireAngle( float alt, Vector3 startPosition, Vector3 endPosition, float bulletVelocity, float targetHeight )
		{ 
			Vector2 a = new Vector2( startPosition.x, startPosition.z );
			Vector2 b = new Vector2( endPosition.x, endPosition.z );
			float dis = Vector2.Distance( a, b );
			alt = -( startPosition.y - targetHeight );
		
			float g = Mathf.Abs( Physics.gravity.y );
				
			float dis2 = dis * dis;
			float vel2 = bulletVelocity * bulletVelocity;
			float vel4 = bulletVelocity * bulletVelocity * bulletVelocity * bulletVelocity;
			float num;
			float sqrt = vel4 - g * ( ( g * dis2 ) + ( 2f * alt * vel2 ) );
			if ( sqrt < 0 )
				return(45f);

			//Direct Fire
			if ( Vector3.Distance( startPosition, endPosition ) > bulletVelocity / 2f )
				num = vel2 - Mathf.Sqrt( vel4 - g * ( ( g * dis2 ) + ( 2f * alt * vel2 ) ) );
			else
				num = vel2 + Mathf.Sqrt( vel4 - g * ( ( g * dis2 ) + ( 2f * alt * vel2 ) ) );
		
			float dom = g * dis;
			float angle = Mathf.Atan( num / dom );

			return angle * Mathf.Rad2Deg;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 
		/// </summary>
		public	static		float		FindClosestPointOfApproach( Vector3 aPos1, Vector3 aSpeed1, Vector3 aPos2, Vector3 aSpeed2 )
		{
			Vector3 PVec = aPos1 - aPos2;
			Vector3 SVec = aSpeed1 - aSpeed2;
			float d = SVec.sqrMagnitude;

			// if d is 0 then the distance between Pos1 and Pos2 is never changing
			// so there is no point of closest approach... return 0
			// 0 means the closest approach is now!
			return ( d >= -0.0001f && d <= 0.0002f ) ? 0.0f : ( -Vector3.Dot( PVec, SVec ) / d );
		}


		/// <summary>
		/// Returns the quadratic interpolation of given vectors
		/// </summary>
		public	static		Vector3		GetPointLinear( Vector3 p0, Vector3 p1, Vector3 p2, float t )
		{
			Vector3 v1 = Vector3.Lerp( p0, p1, t );
			Vector3 v2 = Vector3.Lerp( p1, p2, t );
			return Vector3.Lerp( v1, v2, t );
		}


		/// <summary>
		/// Returns the cubic interpolation of given vectors
		/// </summary>>
		public	static		Vector3		GetPointLinear( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t )
		{
			Vector3 v1 = GetPointLinear( p0, p1, p2, t );
			Vector3 v2 = GetPointLinear( p1, p2, p3, t );
			return Vector3.Lerp( v1, v2, t );
		}


		/// <summary>
		/// Return a Five dimensional interpolation of given vectors
		/// </summary>
		public	static		Vector3		GetPointLinear( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t )
		{
			Vector3 v1 = GetPointLinear( p0, p1, p2, p3, t );
			Vector3 v2 = GetPointLinear( p1, p2, p3, p4, t );
			return Vector3.Lerp( v1, v2, t );
		}

		/// <summary>
		/// 
		/// </summary>
		public	static		Quaternion	GetRotation( Quaternion r0, Quaternion r1, Quaternion r2, float t )
		{
			float slerpT = 2.0f * t * (1.0f - t);
			Quaternion q1 = Quaternion.Slerp( r0, r1, t );
			Quaternion q2 = Quaternion.Slerp( r1, r2, t );
			return Quaternion.Slerp( q1, q2, t );
		}


		/// <summary>
		/// Return a spherical quadrangle interpolation of given quaterniions
		/// </summary>
		public	static		Quaternion	GetRotation( Quaternion r0, Quaternion r1, Quaternion r2, Quaternion r3, float t )
		{
			float slerpT = 2.0f * t * (1.0f - t);

			Quaternion q1 = GetRotation( r0, r1, r2, t );
			Quaternion q2 = GetRotation( r1, r2, r3, t );
			return q1.Slerp( q2, t );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a cubic interpolated vector 
		/// </summary>
		public	static		Vector3		GetPoint( Vector3 p0, Vector3 p1, Vector3 p2, float t )
		{
			t = Mathf.Clamp01( t );
			float oneMinusT = 1f - t;
			return
						oneMinusT	*	oneMinusT	*	p0	+
				2f	*	oneMinusT	*		t		*	p1 +
							t		*		t		*	p2;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a quadratic interpolated vector
		/// </summary>
		public	static		Vector3		GetPoint( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t )
		{
			t = Mathf.Clamp01( t );
			float OneMinusT = 1f - t;
			return
						OneMinusT	*	OneMinusT	*	OneMinusT	*	p0 +
				3f	*	OneMinusT	*	OneMinusT	*		t		*	p1 +
				3f	*	OneMinusT	*		t		*		t		*	p2 +
				t	*		t		*		t		*		1.0f	*	p3;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a Five dimensional interpolated vector
		/// </summary>
		public	static		Vector3		GetPoint( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t )
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


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a Spline interpolation between given points
		/// </summary>
		public	static		Vector3		GetPoint( Vector3[] points, float t )
		{
			if ( points == null || points.Length < 4 )
			{
				UnityEngine.Debug.Log( "GetPoint Called with points invalid array" );
				UnityEngine.Debug.DebugBreak();
			}

			int numSections = points.Length - 3;
			int currPt = Mathf.Min( Mathf.FloorToInt( t * ( float ) numSections ), numSections - 1 );
			float u = t * ( float ) numSections - ( float ) currPt;

			Vector3 a = points[ currPt + 0 ];
			Vector3 b = points[ currPt + 1 ];
			Vector3 c = points[ currPt + 2 ];
			Vector3 d = points[ currPt + 3 ];

			// catmull Rom interpolation
			return .5f * 
			(
				( -a + 3f * b - 3f * c + d )		* ( u * u * u ) +
				( 2f * a - 5f * b + 4f * c - d )	* ( u * u ) +
				( -a + c )							* u +
				2f * b
			);
		}
		

		/// <summary>
		/// Compute spline's waypoints interpolation, assigning position and rotation, return true if everything fine otherwise false
		/// </summary>
		public	static		bool		GetInterpolatedWaypoint( PathWayPointOnline[] ws, float t, ref Vector3 position, ref Quaternion rotation )
		{
			if ( ws == null || ws.Length < 4 )
			{
				UnityEngine.Debug.Log( "GetPoint Called with points invalid array" );
				UnityEngine.Debug.DebugBreak();
				return false;
			}

			int numSections = ws.Length - 3;
			int currPt = Mathf.Min( Mathf.FloorToInt( t * ( float ) numSections ), numSections - 1 );
			float u = t * ( float ) numSections - ( float ) currPt;

//			float rotationInterpolant = 0.0f;

			#region Position
			{
				Vector3 p_a = ws[ currPt + 0 ];
				Vector3 p_b = ws[ currPt + 1 ];
				Vector3 p_c = ws[ currPt + 2 ];
				Vector3 p_d = ws[ currPt + 3 ];

//				rotationInterpolant = ( p_b - position ).magnitude / ( p_c - p_b ).magnitude;

				position = .5f * 
				(
					( -p_a + 3f * p_b - 3f * p_c + p_d )		* ( u * u * u ) +
					( 2f * p_a - 5f * p_b + 4f * p_c - p_d )	* ( u * u ) +
					( -p_a + p_c )								* u +
					2f * p_b
				);
				
			}
			#endregion

			#region Rotation
			{
				Quaternion r_a = ws[ currPt + 0 ];
				Quaternion r_b = ws[ currPt + 1 ];
				Quaternion r_c = ws[ currPt + 2 ];
				Quaternion r_d = ws[ currPt + 3 ];

				rotation = Quaternion.Slerp( r_b, r_c, u ) ;

//				rotation = Utils.Math.GetRotation( r_a, r_b, r_c, r_d, u );
			}
			#endregion

			return true;
		}


		// 
		public    static        bool        GetSegment<T>( global::System.Collections.Generic.IList<T> collection, float t, ref T a, ref T b, ref T c, ref T d )
		{
			if ( collection == null || collection.Count < 4 )
			{
				UnityEngine.Debug.Log( "GetSegment Called with points invalid list" );
				UnityEngine.Debug.DebugBreak();
				return false;
			}
			
			int numSections = collection.Count - 3;
			int currPt = Mathf.Min( Mathf.FloorToInt( t * ( float ) numSections ), numSections - 1 );

			a = collection[ currPt + 0 ];
			b = collection[ currPt + 1 ];
			c = collection[ currPt + 2 ];
			d = collection[ currPt + 2 ];

			return true;
		}

		#region Too complex to explain
		/*
		public static float GetQuatLength(Quaternion q)
		{
			return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
		}

		public static Quaternion GetQuatConjugate(Quaternion q)
		{
			return new Quaternion(-q.x, -q.y, -q.z, q.w);
		}

		/// <summary>
		/// Logarithm of a unit quaternion. The result is not necessary a unit quaternion.
		/// </summary>
		public static Quaternion GetQuatLog(Quaternion q)
		{
			Quaternion res = q;
			res.w = 0;

			if (Mathf.Abs(q.w) < 1.0f)
			{
				float theta = Mathf.Acos(q.w);
				float sin_theta = Mathf.Sin(theta);

				if (Mathf.Abs(sin_theta) > 0.0001)
				{
					float coef = theta / sin_theta;
					res.x = q.x * coef;
					res.y = q.y * coef;
					res.z = q.z * coef;
				}
			}

			return res;
		}

		public static Quaternion GetQuatExp(Quaternion q)
		{
			Quaternion res = q;

			float fAngle = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);
			float fSin = Mathf.Sin(fAngle);

			res.w = Mathf.Cos(fAngle);

			if (Mathf.Abs(fSin) > 0.0001)
			{
				float coef = fSin / fAngle;
				res.x = coef * q.x;
				res.y = coef * q.y;
				res.z = coef * q.z;
			}

			return res;
		}

		/// <summary>
		/// SQUAD Spherical Quadrangle interpolation [Shoe87]
		/// </summary>
		public static Quaternion GetQuatSquad(float t, Quaternion q0, Quaternion q1, Quaternion a0, Quaternion a1)
		{
			float slerpT = 2.0f * t * (1.0f - t);

			Quaternion slerpP = Slerp(q0, q1, t);
			Quaternion slerpQ = Slerp(a0, a1, t);

			return Slerp(slerpP, slerpQ, slerpT);
		}

		public static Quaternion GetSquadIntermediate(Quaternion q0, Quaternion q1, Quaternion q2)
		{
			Quaternion q1Inv = GetQuatConjugate(q1);
			Quaternion p0 = GetQuatLog(q1Inv * q0);
			Quaternion p2 = GetQuatLog(q1Inv * q2);
			Quaternion sum = new Quaternion(-0.25f * (p0.x + p2.x), -0.25f * (p0.y + p2.y), -0.25f * (p0.z + p2.z), -0.25f * (p0.w + p2.w));

			return q1 * GetQuatExp(sum);
		}
		

		/// <summary>
		/// We need this because Quaternion.Slerp always uses the shortest arc.
		/// </summary>
		public static Quaternion Slerp(Quaternion p, Quaternion q, float t)
		{
			Quaternion ret;

			float fCos = Quaternion.Dot(p, q);

			fCos = ( fCos >= 0.0f ) ? fCos : -fCos;

			float fCoeff0, fCoeff1;

			if ( fCos < 0.9999f )
			{
				float omega = Mathf.Acos(fCos);
				float invSin = 1.0f / Mathf.Sin(omega);
				fCoeff0 = Mathf.Sin((1.0f - t) * omega) * invSin;
				fCoeff1 = Mathf.Sin(t * omega) * invSin;
			}
			else
			{
				// Use linear interpolation
				fCoeff0 = 1.0f - t;
				fCoeff1 = t;
			}

			fCoeff1 = ( fCos >= 0.0f ) ? fCoeff1 : -fCoeff1;

			ret.x = fCoeff0 * p.x + fCoeff1 * q.x;
			ret.y = fCoeff0 * p.y + fCoeff1 * q.y;
			ret.z = fCoeff0 * p.z + fCoeff1 * q.z;
			ret.w = fCoeff0 * p.w + fCoeff1 * q.w;
			
			return ret;
		}

		public static float Ease(float t, float k1, float k2)
		{
			float f; float s;

			f = k1 * 2 / Mathf.PI + k2 - k1 + (1.0f - k2) * 2 / Mathf.PI;

			if (t < k1)
			{
				s = k1 * (2 / Mathf.PI) * (Mathf.Sin((t / k1) * Mathf.PI / 2 - Mathf.PI / 2) + 1);
			}
			else
				if (t < k2)
				{
					s = (2 * k1 / Mathf.PI + t - k1);
				}
				else
				{
					s = 2 * k1 / Mathf.PI + k2 - k1 + ((1 - k2) * (2 / Mathf.PI)) * Mathf.Sin(((t - k2) / (1.0f - k2)) * Mathf.PI / 2);
				}

			return (s / f);
		}
		*/
		#endregion
	}

}