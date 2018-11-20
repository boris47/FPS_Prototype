
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
			float distance = Vector3.Dot( planeNormal, ( point - planePoint ) );

			//Reverse the sign of the distance
			distance *= -1;

			//Get a translation vector
			Vector3 translationVector = SetVectorLength( planeNormal, distance );

			//Translate the point to form a projection
			return point + translationVector;
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
			float result = result = Mathf.Max( -b / ( 2f * a ), 0f ); //don't shoot back in time

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
		public	static		float	CalculateFireAngle( float alt, Vector3 startPosition, Vector3 endPosition, float bulletVelocity, float targetHeight )
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
			if ( d >= -0.0001f && d <= 0.0002f )
				return 0.0f;
			return ( -Vector3.Dot( PVec, SVec ) / d );
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
		public	static		Vector3		GetPoint( Vector3[] points, float t, out Vector3 pos1, out Vector3 pos2 )
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

			pos1 = b;
			pos2 = c;

			return .5f * 
			(
				( -a + 3f * b - 3f * c + d )		* ( u * u * u ) +
				( 2f * a - 5f * b + 4f * c - d )	* ( u * u ) +
				( -a + c )							* u +
				2f * b
			);
		}

		/*

				//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return a Spline interpolation between given points
		/// </summary>
		public	static		Vector3		GetPoint( IList<Vector3> points, float t, out PathWayPointOnline w1, out PathWayPointOnline w2 )
		{
			if ( points == null || points.Count < 4 )
			{
				Debug.Log( "GetPoint Called with points invalid array" );
				Debug.DebugBreak();
			}

			int numSections = points.Count - 3;
			int currPt = Mathf.Min( Mathf.FloorToInt( t * ( float ) numSections ), numSections - 1 );
			float u = t * ( float ) numSections - ( float ) currPt;

			w1 = points[ currPt + 1 ];
			w2 = points[ currPt + 2 ];

			Vector3 a = points[ currPt + 0 ];
			Vector3 b = points[ currPt + 1 ];
			Vector3 c = points[ currPt + 1 ];
			Vector3 d = points[ currPt + 3 ];
		
			return .5f * 
			(
				( -a + 3f * b - 3f * c + d )		* ( u * u * u ) +
				( 2f * a - 5f * b + 4f * c - d )	* ( u * u ) +
				( -a + c )							* u +
				2f * b
			);
		}
		*/
	}

}