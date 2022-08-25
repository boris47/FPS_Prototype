
using System.Runtime.CompilerServices;
using UnityEngine;


/// <summary> Can be used to access a Vector3 component </summary>
public enum EVector3Component
{
	X, Y, Z
}

namespace Utils
{
	public static class Types
	{
		//////////////////////////////////////////////////////////////////////////
		public static bool IsNotNull<T, O>(T InValue, out O OutValue) where T : class where O : class
		{
			OutValue = default;
			if (InValue.IsNotNull() && (InValue as O).IsNotNull())
			{
				OutValue = InValue as O;
			}
			return !System.Collections.Generic.EqualityComparer<O>.Default.Equals(OutValue, default);
		}


		//////////////////////////////////////////////////////////////////////////
		public static bool IsNotNull<O>(UnityEngine.Object InObject, out O OutObject) where O : UnityEngine.Object
		{
			OutObject = null;
			if (InObject.IsNotNull() && (InObject as O).IsNotNull())
			{
				OutObject = InObject as O;
			}
			return OutObject.IsNotNull();
		}
	}

	public static class CustomAssertions
	{

		//////////////////////////////////////////////////////////////////////////
		[System.Diagnostics.DebuggerHidden()]
		public static bool IsNotDefault<T>(T value, string message = null, UnityEngine.Object context = null)
		{
			bool bIsEqual = value.Equals(default(T));
			if (bIsEqual)
			{
				UnityEngine.Debug.LogError(message ?? "Utils.CustomAssertions.IsNotDefault: Assertion Failed " + context?.name ?? "", context);
#if UNITY_EDITOR
				System.Diagnostics.Debugger.Break();
#else
				UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.FatalError);
#endif
			}
			return !bIsEqual;
		}


		//////////////////////////////////////////////////////////////////////////
		[System.Diagnostics.DebuggerHidden()]
		public static bool IsTrue(bool bIsTrue, string message = null, UnityEngine.Object context = null)
		{
			if (!bIsTrue)
			{
				message = message ?? "Utils.CustomAssertions.IsTrue: Assertion Failed " + context?.name ?? "";

				if (System.Diagnostics.Debugger.IsAttached)
				{
					System.Diagnostics.Debugger.Break();
					System.Diagnostics.Debug.WriteLine(message);
				}
				UnityEngine.Debug.LogError(message, context);
			}
			return bIsTrue;
		}


		//////////////////////////////////////////////////////////////////////////
		[System.Diagnostics.DebuggerHidden()]
		public static bool IsNotNull(System.Object value, string message = null, UnityEngine.Object context = null)
		{
			bool bIsNull = !value.IsNotNull();
			if (bIsNull)
			{
#if UNITY_EDITOR
				System.Diagnostics.Debugger.Break();

				UnityEditor.EditorApplication.isPlaying = false;
#endif
				string msg = "Utils.CustomAssertions.IsNotNull: Assertion Failed";
				if (!string.IsNullOrEmpty(message))
				{
					msg += $" -> {message}";
				}
				UnityEngine.Debug.LogError(msg, context);
#if !UNITY_EDITOR
				UnityEngine.Debug.LogError(System.Environment.StackTrace);
#endif
			}
			return !bIsNull;
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////

	public static class Math
	{
		public const float EPS = 0.00001f;


		/// <summary> Return true if the value is between min and max values, otherwise return false </summary>
		/// <param name="Value"></param>
		/// <param name="Min"></param>
		/// <param name="Max"></param>
		/// <returns></returns>
		public static bool IsBetweenValues(in float Value, in float Min, in float Max)
		{
			return Value > Min && Value < Max;
		}


		/// <summary> Return true if the value is between or equal min and max values, otherwise return false </summary>
		/// <param name="Value"></param>
		/// <param name="Min"></param>
		/// <param name="Max"></param>
		/// <returns></returns>
		public static bool IsBetweenOrEqualValues(in float Value, in float Min, in float Max)
		{
			return Value >= Min && Value <= Max;
		}


		/// Ref: https://stackoverflow.com/a/28957910
		/// <summary>
		/// Return the scaled value between given limits clamped to range [0, 1]
		/// Ex: CurrentDistance, MAX_DISTANCE, MIN_DISTANCE ( 0 -> 1 [ MinLimit -> CurrentDistance -> MaxLimit ] )
		/// </summary>
		/// <param name="CurrentValue">The actual value to normalize.</param>
		/// <param name="MinValue">The minimum value the actual value can be.</param>
		/// <param name="MaxValue">The maximum value the actual value can be.</param>
		/// <param name="Threshold">The threshold to force to the minimum or maximum value if the normalized value is within the threhold limits.</param>
		/// <returns></returns>
		public static float ScaleBetweenClamped01(in float CurrentValue, in float MinValue, in float MaxValue, in float Threshold = 0f)
		{
			float normalizedMax = MaxValue - MinValue;
			float normalizedValue = normalizedMax - ( MaxValue - CurrentValue );
			float result = normalizedValue * (  normalizedMax != 0f ? 1f / normalizedMax : 1f  );
			result = ( result < Threshold ? 0f : result );
			result = ( result > 1f - Threshold ? 1f : result );
			return Mathf.Clamp( result, 0f, 1f );
		}

		/// Ref: https://en.wikipedia.org/wiki/Feature_scaling
		/// <summary>
		/// Return the value that lies between MinValue and MaxValue scaled in the given limits
		/// Ex: CurrentValue, 0, 5000, 0, 1 ( 0 -> 1 [ MinScale -> CurrentValue -> MaxScale ] )
		/// </summary>
		/// <param name="CurrentValue"></param>
		/// <param name="MinValue"></param>
		/// <param name="MaxValue"></param>
		/// <param name="MinScale"></param>
		/// <param name="MaxScale"></param>
		/// <returns></returns>
		public static float ScaleBetween(in float CurrentValue, in float MinValue, in float MaxValue, in float MinScale, in float MaxScale)
		{
			return MinScale + ( ( CurrentValue - MinValue ) / ( MaxValue - MinValue ) * ( MaxScale - MinScale ) );
		}


		//////////////////////////////////////////////////////////////////////////
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float BoolToMinusOneOsPlusOne(in bool InValue)
		{
			return InValue ? 1 : -1;
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a better performance method to get squared value </summary>
		public static float Sqr(in float value)
		{
			return Mathf.Pow( value, 0.5f );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> With a optional Epsilon, determines if value is similar to Zero </summary>
		public static bool SimilarZero(in float a, float cmp = EPS)
		{
			return Mathf.Abs( a ) < cmp;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a clamped value </summary>
		public static float Clamp(in float Value, in float Min, in float Max)
		{
			return ( Value > Max ) ? Max : ( Value < Min ) ? Min : Value;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Assign a value the clamp itself and return true if value was still in range on [min-max] </summary>
		public static bool ClampResult(ref float value, in float expressionValue, in float min, in float max)
		{
			bool bResult = expressionValue >= min && expressionValue <= max;
			value = (expressionValue > max) ? max : (expressionValue < min) ? min : expressionValue;
			return bResult;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a clamped angle </summary>
		public static float ClampAngle(in float Angle, in float Min = 0f, in float Max = 360f)
		{
			float angle = Angle;
			while (Angle > 360)
			{
				angle = -360;
			}

			angle = Mathf.Max(Mathf.Min(Angle, Max), Min);
			if (angle < 0)
			{
				angle += 360;
			}

			return Angle;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get planar squared distance between two positions, position1 is projected on position2 plane
		/// </summary>
		/// <returns>Planar Squared Distance</returns>
		public static float PlanarSqrDistance(in Vector3 position1, in Vector3 position2, in Vector3 position2PlaneNormal)
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
		public static float PlanarDistance(in Vector3 position1, in Vector3 position2, in Vector3 planeNormal)
		{
			float sqrDistance = PlanarSqrDistance( position1, position2, planeNormal );

			return Mathf.Sqrt( sqrDistance );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get a direction vector from polar coordinates
		/// </summary>>
		public static Vector3 VectorByHP(in float h, in float p)
		{
			float _ch = Mathf.Cos( h * Mathf.Deg2Rad );
			float _cp = Mathf.Cos( p * Mathf.Deg2Rad );
			float _sh = Mathf.Sin( h );
			float _sp = Mathf.Sin( p );
			return new Vector3( _cp * _sh, _sp, _cp * _ch );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Return if a position is inside a mesh
		/// </summary>
		public static bool IsPointInside(in MeshFilter MeshFilter, in Vector3 WorldPosition)
		{
			Mesh aMesh = MeshFilter.sharedMesh;
			Vector3 aLocalPoint = MeshFilter.transform.InverseTransformPoint( WorldPosition );
			Plane plane = new Plane();

			Vector3[] verts = aMesh.vertices;
			int[] tris = aMesh.triangles;
			int triangleCount = tris.Length / 3;
			for ( int i = 0; i < triangleCount; i++ )
			{
				Vector3 V1 = verts[tris[i * 3]];
				Vector3 V2 = verts[tris[( i * 3 ) + 1]];
				Vector3 V3 = verts[tris[( i * 3 ) + 2]];
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
		public static Vector3 SetVectorLength(in Vector3 vector, in float size)
		{
			//normalize the vector
			Vector3 vectorNormalized = Vector3.Normalize( vector );

			//scale the vector
			return vectorNormalized *= size;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This function returns a point which is a projection from a point to a plane.
		/// </summary>
		public static Vector3 ProjectPointOnPlane(in Vector3 planeNormal, in Vector3 planePoint, in Vector3 point)
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


		/// <summary> Get the intersection between a line and a plane.  </summary>
		/// <param name="intersection"></param>
		/// <param name="linePoint"></param>
		/// <param name="lineVec"></param>
		/// <param name="planeNormal"></param>
		/// <param name="planePoint"></param>
		/// <returns>If the line and plane are not parallel, the function outputs true, otherwise false.</returns>
		public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planePoint, Vector3 planeNormal)
		{
			intersection = Vector3.zero;

			//calculate the distance between the linePoint and the line-plane intersection point
			float dotNumerator = Vector3.Dot(planePoint - linePoint, planeNormal);
			float dotDenominator = Vector3.Dot(lineVec, planeNormal);

			//line and plane are not parallel
			if (dotDenominator != 0f)
			{
				float length = dotNumerator / dotDenominator;

				//create a vector from the linePoint to the intersection point
				Vector3 vector = SetVectorLength(lineVec, length);

				//get the coordinates of the line-plane intersection point
				intersection = linePoint + vector;

				return true;
			}
			//output not valid
			return false;
		}

		/// <summary> For valid arguments return the angle between two vectors that lins on the plane defined by given components </summary>
		/// <param name="v1">The first vector</param>
		/// <param name="v2">The second vecor</param>
		/// <param name="Comp1">Primary compoent of the plane</param>
		/// <param name="Comp2">Secondary component of the plane</param>
		/// <returns>The the angle between two vector that defined by the plane defined by given components </returns>
		public static float Angle(Vector3 v1, Vector3 v2, EVector3Component Comp1, EVector3Component Comp2)
		{
			float tanAngleA = 0f, tanAngleB = 0f;
			try
			{
				tanAngleA = Mathf.Atan2(v1[(int)Comp1], v1[(int)Comp2]);
				tanAngleB = Mathf.Atan2(v2[(int)Comp1], v2[(int)Comp2]);
			}
			catch (System.Exception)
			{
				Debug.LogWarning($"Comp1 or Comp2 bad value: AtanY: {(int)Comp1}, AtanX: {(int)Comp2}");
			}

			float angleA = tanAngleA * Mathf.Rad2Deg;
			float angleB = tanAngleB * Mathf.Rad2Deg;
			return Mathf.DeltaAngle(angleA, angleB);
		}

		/// <summary> Calculate the angle between a vector and a plane. The plane is made by a normal vector. Output is in degree. </summary>
		public static float AngleVectorPlane(Vector3 vector, Vector3 normal)
		{
			//calculate the the dot product between the two input vectors. This gives the cosine between the two vectors
			float dot = Vector3.Dot(vector, normal);

			//this is in radians
			float angle = (float)System.Math.Acos(dot);

			return (1.570796326794897f - angle) * Mathf.Rad2Deg; //90 degrees - angle
		}

		//////////////////////////////////////////////////////////////////////////
		public static bool LineSphereIntersection(in Vector3 SphereCenter, in float SphereRadius, in Vector3 LineStart, in Vector3 LineEnd, /*in float LineLength,*/ out Vector3 ClosestPoint)
		{
			ClosestPoint = Vector3.zero;

			Vector3 LineDirectionNormalized = ( LineEnd - LineStart ).normalized;
			Vector3 m = LineStart - SphereCenter;
			float b = Vector3.Dot( m, LineDirectionNormalized );
			float c = Vector3.Dot( m, m ) - ( SphereRadius * SphereRadius );

			// Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
			if ( c > 0.0f && b > 0.0f )
			{
				return false;
			}

			float discriminant = ( b * b ) - c;

			// A negative discriminant corresponds to ray missing sphere 
			if ( discriminant < 0.0f )
			{
				return false;
			}

			// Ray now found to intersect sphere, compute smallest t value of intersection
			float t = -b - Sqr( discriminant );

			// If t is negative, ray started inside sphere so clamp t to zero 
			if ( t < 0.0f )
			{
				t = 0.0f;
			}
			ClosestPoint = LineStart + ( t * LineDirectionNormalized );
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> First-order intercept using absolute target position </summary>
		public static Vector3 CalculateBulletPrediction(in Vector3 shooterPosition, in Vector3 shooterVelocity, in float shotSpeed, in Vector3 targetPosition, in Vector3 targetVelocity)
		{
			Vector3 shooterToTarget = targetPosition - shooterPosition;
			Vector3 velocityDelta = targetVelocity - shooterVelocity;
			float t = FirstOrderInterceptTime
			(
				shotSpeed: shotSpeed,
				shooterToTarget: shooterToTarget,
				velocityDelta: velocityDelta
			);
			return targetPosition + ( t * velocityDelta );
		}

		//first-order intercept using relative target position
		public static float FirstOrderInterceptTime(in float shotSpeed, in Vector3 shooterToTarget, in Vector3 velocityDelta)
		{
			float velocitySquared = velocityDelta.sqrMagnitude;
			if (velocitySquared < 0.001f)
			{
				return 0f;
			}

			float a = velocitySquared - (shotSpeed * shotSpeed);

			//handle similar velocities
			if (Mathf.Abs(a) < 0.001f)
			{
				float t = -shooterToTarget.sqrMagnitude / (2f * Vector3.Dot(velocityDelta, shooterToTarget));
				return Mathf.Max(t, 0f); //don't shoot back in time
			}

			float b = 2f * Vector3.Dot(velocityDelta, shooterToTarget);
			float c = shooterToTarget.sqrMagnitude;
			float determinant = (b * b) - (4f * a * c);

			// First assignment: Determinant == 0; one intercept path, pretty much never happens
			float result = Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time

			if (determinant > 0f)
			{   //	Determinant > 0; two intercept paths (most common)
				float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a);
				float t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
				if (t1 > 0f)
				{
					if (t2 > 0f)
					{
						result = Mathf.Min(t1, t2); //both are positive
					}
					else
					{
						result = t1; //only t1 is positive
					}
				}
				else
				{
					result = Mathf.Max(t2, 0f); //don't shoot back in time
				}
			}

			//determinant < 0; no intercept path
			if (determinant < 0f)
			{
				result = 0f;
			}

			return result;
		}


		// https://unity3d.college/2017/06/30/unity3d-cannon-projectile-ballistics/
		//////////////////////////////////////////////////////////////////////////
		public static Vector3 BallisticVelocity(in Vector3 startPosition, in Vector3 destination, in float angle)
		{
			Vector3 dir = destination - startPosition;              // get Target Direction
			float height = dir.y;                                   // get height difference
			dir.y = 0;                                              // retain only the horizontal difference
			float dist = dir.magnitude;                             // get horizontal direction
			float a = angle * Mathf.Deg2Rad;                        // Convert angle to radians
			dir.y = dist * Mathf.Tan( a );                          // set dir to the elevation angle.
			dist += height / Mathf.Tan( a );                        // Correction for small height differences

			// Calculate the velocity magnitude
			float velocity = Mathf.Sqrt( dist * Physics.gravity.magnitude / Mathf.Sin( 2 * a ) );
			return velocity * dir;                                  // Return a normalized vector.
		}


		//////////////////////////////////////////////////////////////////////////
		public static float CalculateFireAngle(in Vector3 startPosition, in Vector3 endPosition, in float bulletVelocity, in float targetHeight)
		{
			Vector2 a = new Vector2(startPosition.x, startPosition.z);
			Vector2 b = new Vector2(endPosition.x, endPosition.z);
			float dis = Vector2.Distance(a, b);
			float alt = -(startPosition.y - targetHeight);

			float g = Mathf.Abs(Physics.gravity.y);

			float dis2 = dis * dis;
			float vel2 = bulletVelocity * bulletVelocity;
			float vel4 = bulletVelocity * bulletVelocity * bulletVelocity * bulletVelocity;
			float num;
			float sqrt = vel4 - (g * ((g * dis2) + (2f * alt * vel2)));
			if (sqrt >= 0f)
			{
				//Direct Fire
				if (Vector3.Distance(startPosition, endPosition) > bulletVelocity / 2f)
				{
					num = vel2 - Mathf.Sqrt(vel4 - (g * ((g * dis2) + (2f * alt * vel2))));
				}
				else
				{
					num = vel2 + Mathf.Sqrt(vel4 - (g * ((g * dis2) + (2f * alt * vel2))));
				}

				float dom = g * dis;
				float angle = Mathf.Atan(num / dom);
				return angle * Mathf.Rad2Deg;
			}
			return (45f);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		public static float FindClosestPointOfApproach(in Vector3 aPos1, in Vector3 aSpeed1, in Vector3 aPos2, in Vector3 aSpeed2)
		{
			Vector3 PVec = aPos1 - aPos2;
			Vector3 SVec = aSpeed1 - aSpeed2;
			float d = SVec.sqrMagnitude;

			// if d is 0 then the distance between Pos1 and Pos2 is never changing
			// so there is no point of closest approach... return 0
			// 0 means the closest approach is now!
			return ( d >= -0.0001f && d <= 0.0002f ) ? 0.0f : ( -Vector3.Dot( PVec, SVec ) / d );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Returns the quadratic interpolation of given vectors </summary>
		public static Vector3 GetPointLinear(in Vector3 p0, in Vector3 p1, in Vector3 p2, in float t)
		{
			Vector3 v1 = Vector3.Lerp( p0, p1, t );
			Vector3 v2 = Vector3.Lerp( p1, p2, t );
			return Vector3.Lerp( v1, v2, t );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Returns the cubic interpolation of given vectors </summary>>
		public static Vector3 GetPointLinear(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, in float t)
		{
			Vector3 v1 = GetPointLinear( p0, p1, p2, t );
			Vector3 v2 = GetPointLinear( p1, p2, p3, t );
			return Vector3.Lerp( v1, v2, t );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a Five dimensional interpolation of given vectors </summary>
		public static Vector3 GetPointLinear(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, in Vector3 p4, in float t)
		{
			Vector3 v1 = GetPointLinear( p0, p1, p2, p3, t );
			Vector3 v2 = GetPointLinear( p1, p2, p3, p4, t );
			return Vector3.Lerp( v1, v2, t );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> </summary>
		public static Quaternion GetRotation(in Quaternion r0, in Quaternion r1, in Quaternion r2, in float t)
		{
		//	float slerpT = 2.0f * t * ( 1.0f - t );
			Quaternion q1 = Quaternion.Slerp( r0, r1, t );
			Quaternion q2 = Quaternion.Slerp( r1, r2, t );
			return Quaternion.Slerp( q1, q2, t );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a spherical quadrangle interpolation of given quaternions </summary>
		public static Quaternion GetRotation(in Quaternion r0, in Quaternion r1, in Quaternion r2, in Quaternion r3, in float t)
		{
			float slerpT = 2.0f * t * ( 1.0f - t );

			Quaternion q1 = GetRotation( r0, r1, r2, t );
			Quaternion q2 = GetRotation( r1, r2, r3, t );
			return q1.Slerp( q2, t );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a cubic interpolated vector </summary>
		public static Vector3 GetPoint(in Vector3 p0, in Vector3 p1, in Vector3 p2, float t)
		{
			t = Mathf.Clamp01( t );
			float oneMinusT = 1f - t;
			return (oneMinusT * oneMinusT * p0) + (2f * oneMinusT * t * p1) + (t * t * p2);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a quadratic interpolated vector </summary>
		public static Vector3 GetPoint(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, float t)
		{
			t = Mathf.Clamp01( t );
			float OneMinusT = 1f - t;
			return
				( OneMinusT * OneMinusT * OneMinusT * p0 ) +
				( 3f * OneMinusT * OneMinusT * t * p1 ) +
				( 3f * OneMinusT * t * t * p2 ) +
				( t * t * t * 1.0f * p3 );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a Five dimensional interpolated vector </summary>
		public static Vector3 GetPoint(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, in Vector3 p4, float t)
		{
			t = Mathf.Clamp01( t );
			float OneMinusT = 1f - t;
			return
				( OneMinusT * OneMinusT * OneMinusT * OneMinusT * p0 ) +
				( 4f * OneMinusT * OneMinusT * OneMinusT * t * p1 ) +
				( 5f * OneMinusT * OneMinusT * t * t * p2 ) +
				( 4f * OneMinusT * t * t * t * p3 ) +
							( t * t * t * t * p4 );
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a Spline interpolation between given points </summary>
		public static Vector3 GetPoint(in Vector3[] points, float t)
		{
			int length = points.Length;
			if ( points == null || length < 4 )
			{
				UnityEngine.Debug.Log( "GetPoint Called with points invalid array" );
				UnityEngine.Debug.DebugBreak();
			}

			bool bIsReversed = t < 0.0f;
			t = Mathf.Abs( t );

			int numSections = length - 3;
			int currPt = Mathf.Min( Mathf.FloorToInt( t * (float) numSections ), numSections - 1 );
			if ( bIsReversed )
			{
				currPt = length - 1 - currPt;
			}

			float u = ( t * (float) numSections ) - (
				bIsReversed ?
					( (float) length - 1f - (float) currPt )
					:
					(float) currPt
				)
			;
			u = Mathf.Clamp01( u );

			Vector3 a = points[currPt + 0];
			Vector3 b = points[currPt + 1];
			Vector3 c = points[currPt + 2];
			Vector3 d = points[currPt + 3];

			// catmull Rom interpolation
			return .5f *
			(
				( ( -a + ( 3f * b ) - ( 3f * c ) + d ) * ( u * u * u ) ) +
				( ( ( 2f * a ) - ( 5f * b ) + ( 4f * c ) - d ) * ( u * u ) ) +
				( ( -a + c ) * u ) +
				( 2f * b )
			);
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		public static bool GetSegment<T>(in System.Collections.Generic.IList<T> collection, float t, ref T a, ref T b, ref T c, ref T d)
		{
			int length = collection.Count;
			if (collection == null || length < 4)
			{
				UnityEngine.Debug.Log("GetSegment Called with points invalid list");
				UnityEngine.Debug.DebugBreak();
				return false;
			}

			bool bIsReversed = t < 0.0f;
			t = Mathf.Abs(t);

			int numSections = length - 3;
			int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
			if (bIsReversed)
			{
				currPt = length - 1 - currPt;
			}

			a = bIsReversed ? collection[currPt + 0] : collection[currPt + 0];
			b = bIsReversed ? collection[currPt - 1] : collection[currPt + 1];
			c = bIsReversed ? collection[currPt - 2] : collection[currPt + 2];
			d = bIsReversed ? collection[currPt - 2] : collection[currPt + 2];
			return true;
		}
	}

}