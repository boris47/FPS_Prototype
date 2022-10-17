
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

namespace Utils // Types
{
	public static class Types
	{
		//////////////////////////////////////////////////////////////////////////
	//public static bool IsNotNull<T, O>(in T InValue, out O OutValue) where T : class where O : class
	//{
	//	OutValue = default;
	//	if (InValue.IsNotNull() && (InValue as O).IsNotNull())
	//	{
	//		OutValue = InValue as O;
	//	}
	//	return !System.Collections.Generic.EqualityComparer<O>.Default.Equals(OutValue, default);
	//}


		//////////////////////////////////////////////////////////////////////////
		public static bool IsNotNull<T, O>(in T InObject, out O OutObject) where T : UnityEngine.Object where O : T
		{
			OutObject = null;
			if (InObject is O converted)
			{
				OutObject = converted;
			}
			return OutObject.IsNotNull();
		}
	}

	[System.Serializable]
	public class TypeIdentifier : System.IEquatable<TypeIdentifier>
	{
		[ReadOnly]
		public string TypeFullName = string.Empty;
		[ReadOnly]
		public string AssemblyName = string.Empty;

		public TypeIdentifier(System.Type InType)
		{
			if (InType.IsNotNull())
			{
				TypeFullName = InType.FullName;
				AssemblyName = InType.Assembly.FullName;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public bool IsValid() => TryGetType(out System.Type _);

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetType(out System.Type OutType) => (OutType = System.Type.GetType($"{TypeFullName}, {AssemblyName}")).IsNotNull();

		//////////////////////////////////////////////////////////////////////////
		public override bool Equals(object obj) => Equals(obj as TypeIdentifier);

		//////////////////////////////////////////////////////////////////////////
		public bool Equals(TypeIdentifier other) => other.IsNotNull() && TypeFullName == other.TypeFullName && AssemblyName == other.AssemblyName;

		//////////////////////////////////////////////////////////////////////////
		public static bool operator ==(TypeIdentifier left, TypeIdentifier right) => System.Collections.Generic.EqualityComparer<TypeIdentifier>.Default.Equals(left, right);

		//////////////////////////////////////////////////////////////////////////
		public static bool operator !=(TypeIdentifier left, TypeIdentifier right) => !(left == right);
	}
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

namespace Utils // CustomAssertions
{
	public static class CustomAssertions
	{
		public struct CustomAssertionMessage
		{
			public readonly string Message;

			private CustomAssertionMessage(in string InMessage)
			{
				Message = InMessage;
			}

			public static implicit operator CustomAssertionMessage(in string InMessage) => new CustomAssertionMessage(InMessage);
			public static implicit operator string(in CustomAssertionMessage cm) => cm.IsNotNull() ? cm.Message : null;
		}

		private struct Messages
		{
			private const string k_ASSERTIONFAILED = "Assertion Failed";
			public static readonly string DefaultValueAssertion = $"{k_ASSERTIONFAILED}(DefaultValue)";
			public static readonly string InvalidCastAssertion = $"{k_ASSERTIONFAILED}(InvalidCast)";
			public static readonly string BooleanAssertion = $"{k_ASSERTIONFAILED}(Boolean)";
			public static readonly string ReferenceAssertion = $"{k_ASSERTIONFAILED}(Reference)";
		}

		//////////////////////////////////////////////////////////////////////////
		private static void LogErrorMessage(string assertionTypeMessage, UnityEngine.Object InUnityObjectContext, in CustomAssertionMessage InCustomMessage)
		{
			string finalMessage = assertionTypeMessage;
			if (!string.IsNullOrEmpty(InCustomMessage))
			{
				finalMessage += $"-> {(string)InCustomMessage}";
			}

			UnityEngine.Debug.LogError(finalMessage, InUnityObjectContext); ;

#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying)
			{
				UnityEditor.EditorApplication.isPaused = true;
			}
#endif
			if (System.Diagnostics.Debugger.IsAttached)
			{
				System.Diagnostics.Debugger.Break();
				System.Diagnostics.Debug.WriteLine(finalMessage);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public static bool IsNotDefault<T>(in T InValue, in CustomAssertionMessage? InCustomMessage = null)
		{
			return IsNotDefault(InValue, null, InCustomMessage);
		}
		//////////////////////////////////////////////////////////////////////////
		public static bool IsNotDefault<T>(in T InValue, UnityEngine.Object InUnityObjectContext, in string InCustomMessage = null)
		{
			bool bIsEqual = System.Collections.Generic.EqualityComparer<T>.Default.Equals(InValue, default(T));
			if (bIsEqual)
			{
				LogErrorMessage(Messages.DefaultValueAssertion, InUnityObjectContext, InCustomMessage);
			}
			return !bIsEqual;
		}

		//////////////////////////////////////////////////////////////////////////
		public static bool IsTrue(in bool bIsTrue, in CustomAssertionMessage? InCustomMessage = null)
		{
			return IsTrue(bIsTrue, null, InCustomMessage);
		}
		//////////////////////////////////////////////////////////////////////////
		public static bool IsTrue(in bool bIsTrue, UnityEngine.Object InUnityObjectContext, in string InCustomMessage = null)
		{
			if (!bIsTrue)
			{
				LogErrorMessage(Messages.BooleanAssertion, InUnityObjectContext, InCustomMessage);
			}
			return bIsTrue;
		}

		//////////////////////////////////////////////////////////////////////////
		public static bool IsNotNull(in System.Object InValue, in CustomAssertionMessage? InCustomMessage = null)
		{
			return IsNotNull(InValue, null, InCustomMessage);
		}
		//////////////////////////////////////////////////////////////////////////
		public static bool IsNotNull(in System.Object InValue, UnityEngine.Object InUnityObjectContext, in string InCustomMessage = null)
		{
			bool bIsNotNull = InValue.IsNotNull();
			if (!bIsNotNull)
			{
				LogErrorMessage(Messages.ReferenceAssertion, InUnityObjectContext, InCustomMessage);
			}
			return bIsNotNull;
		}

		//////////////////////////////////////////////////////////////////////////
		public static bool IsValidUnityObjectCast<T, V>(T InValue, out V OutValue, in CustomAssertionMessage? InCustomMessage = null) where T : UnityEngine.Object where V : T
		{
			return IsValidUnityObjectCast(InValue, out OutValue, null, InCustomMessage);
		}
		//////////////////////////////////////////////////////////////////////////
		public static bool IsValidUnityObjectCast<T, V>(T InValue, out V OutValue, in UnityEngine.Object InUnityObjectContext, in string InCustomMessage = null) where T : UnityEngine.Object where V : T
		{
			OutValue = default(V);
			bool bIsValid = false;
			if (InValue is V converted)
			{
				OutValue = converted;
				bIsValid = true;
			}

			if (!bIsValid)
			{
				LogErrorMessage(Messages.InvalidCastAssertion, InUnityObjectContext, InCustomMessage);
			}
			return bIsValid;
		}
	}

}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

namespace Utils // Generic
{
	public static class Generic
	{
		//////////////////////////////////////////////////////////////////////////
		private static readonly uint[] s_crc32_table = new uint[256]
		{
			0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA,
			0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3,
			0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988,
			0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91,
			0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE,
			0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7,
			0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC,
			0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5,
			0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
			0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B,
			0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940,
			0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59,
			0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116,
			0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F,
			0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924,
			0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D,
			0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A,
			0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
			0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818,
			0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01,
			0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E,
			0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457,
			0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA, 0xFCB9887C,
			0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
			0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2,
			0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB,
			0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
			0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9,
			0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086,
			0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F,
			0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4,
			0x59B33D17, 0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD,
			0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A,
			0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683,
			0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8,
			0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
			0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE,
			0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7,
			0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC,
			0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5,
			0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252,
			0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B,
			0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60,
			0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79,
			0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
			0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F,
			0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04,
			0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D,
			0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A,
			0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713,
			0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38,
			0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21,
			0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E,
			0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
			0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C,
			0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45,
			0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2,
			0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB,
			0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 0x37D83BF0,
			0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
			0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6,
			0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF,
			0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
			0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
		};

		//////////////////////////////////////////////////////////////////////////
		public static uint GetUniqueId(in string InSeed)
		{
			// Ref: CCrc32::Compute from cry engine 5.6 so https://wiki.osdev.org/CRC32
			uint OutResult = 0u;
			for (int i = 0, length = InSeed.Length; i < length; i++)
			{
				OutResult = s_crc32_table[((OutResult) ^ (InSeed[i])) & 0xff] ^ ((OutResult) >> 8);
			}
			return ~OutResult;
		}
	}

}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

namespace Utils // Math
{
	using System.Runtime.CompilerServices;
	using UnityEngine;

	/// <summary> Can be used to access a Vector3 component </summary>
	public enum EVector3Component
	{
		X, Y, Z
	}

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

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

namespace Utils // Paths
{
	public static class Paths
	{
		public static bool IsAssetsPath(in string InPath) => InPath.StartsWith("Assets/");

		public static bool IsAsset(in string InPath) => IsAssetsPath(InPath) && InPath.EndsWith(".asset");

		public static bool IsResourcesPath(in string InPath) => !IsAsset(InPath) && !InPath.Contains("Resources");

		public static bool IsAbsolutePath(in string InPath)
		{
			try
			{
				return !string.IsNullOrWhiteSpace(InPath)
				&& InPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1
				&& System.IO.Path.IsPathRooted(InPath) //  whether the specified path string contains a root.
				&& !System.IO.Path.GetPathRoot(InPath).Equals(System.IO.Path.DirectorySeparatorChar.ToString(), System.StringComparison.Ordinal);
			}
			catch (System.Exception)
			{
				return false;
			}
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> </summary>
		public static bool TryConvertFromAssetPathToResourcePath(in string InAssetPath, out string OutResourcePath)
		{
			const string AssetPathPrefix = "Assets/Resources/";
			const int AssetPathPrefixLength = 17;

			if (!string.IsNullOrEmpty(InAssetPath))
			{
				if (IsResourcesPath(InAssetPath))
				{
					OutResourcePath = InAssetPath;
					return true;
				}

				if (InAssetPath.StartsWith(AssetPathPrefix))
				{
					OutResourcePath =
					// Assets/Resources/PATH_TO_FILE.png
					global::System.IO.Path.ChangeExtension(InAssetPath, null)
					// Assets/Resources/PATH_TO_FILE
					.Remove(0, AssetPathPrefixLength);
					// resourcePath -> // PATH_TO_FILE
					return true;
				}
			}
			OutResourcePath = string.Empty;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		public static bool TryConvertFromResourcePathToAssetPath(in string InResourcePath, out string OutAssetPath)
		{
			const string AssetPathPrefix = "Assets";

			if (IsAssetsPath(InResourcePath))
			{
				OutAssetPath = InResourcePath;
				return true;
			}

			if (!string.IsNullOrEmpty(InResourcePath))
			{
				OutAssetPath = $"{AssetPathPrefix}/Resources/{InResourcePath}.asset";
				return true;
			}

			OutAssetPath = string.Empty;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> </summary>
		public static bool TryConvertFromAbsolutePathToResourcePath(in string InAbsoluteAssetPath, out string OutResourcePath)
		{
			if (!string.IsNullOrEmpty(InAbsoluteAssetPath))
			{
				if (IsAbsolutePath(InAbsoluteAssetPath))
				{
					OutResourcePath = InAbsoluteAssetPath;
					return true;
				}

				int index = InAbsoluteAssetPath.IndexOf("Resources");
				if (index > -1)
				{
					// ABSOLUTE_PATH_TO_RESOURCE_FOLDER/Resources/PATH_TO_RESOURCE.png
					string result = InAbsoluteAssetPath;

					// Remove extension
					if (System.IO.Path.HasExtension(InAbsoluteAssetPath))
					{
						result = System.IO.Path.ChangeExtension(InAbsoluteAssetPath, null);
					}

					// ABSOLUTE_PATH_TO_RESOURCE_FOLDER/Resources/PATH_TO_RESOURCE
					OutResourcePath = result.Remove(0, index + 9 /*'Resource'*/ + 1 /*'/'*/ );
					// PATH_TO_RESOURCE
					return true;
				}
			}
			OutResourcePath = string.Empty;
			return false;
		}
	}

}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

namespace Utils // LayersHelper
{
	using UnityEngine;

	public static class LayersHelper
	{
		////////////////////////////////////////////////
		public static int AllButOne(in string InLayerName)
		{
			int outValue = 0;
			if (string.IsNullOrEmpty(InLayerName))
			{
				int layer = LayerMask.NameToLayer(InLayerName);
				int layerMask = 1 << layer;
				outValue = ~layerMask;
			}
			return outValue;
		}

		////////////////////////////////////////////////
		public static int OneOnly(in string InLayerName)
		{
			return string.IsNullOrEmpty(InLayerName) ? 0 : LayerMask.NameToLayer(InLayerName);
		}

		////////////////////////////////////////////////
		public static LayerMask InclusiveMask(in int[] InLayers)
		{
			int outValue = 0;
			if (InLayers.IsNotNull())
			{
				for (int l = 0, length = InLayers.Length; l < length; l++)
				{
					outValue |= (1 << InLayers[l]);
				}
			}
			return outValue;
		}

		////////////////////////////////////////////////
		public static LayerMask ExclusiveMask(in int[] InLayers)
		{
			int outValue = 0;
			if (InLayers.IsNotNull())
			{
				for (int l = 0, length = InLayers.Length; l < length; l++)
				{
					outValue |= (1 << InLayers[l]);
				}
				outValue = ~outValue;
			}
			return outValue;
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

namespace Utils // Converters
{
	using System.Globalization;
	using UnityEngine;

	public static class Converters
	{
		//////////////////////////////////////////////////////////////////////////
		/// <summary> Accept a string and return the parsed result as enum value </summary>
		public static bool StringToEnum<T>(in string InString, out T OutEnumValue, bool bIgnoreCase = true) where T : struct
		{
			return global::System.Enum.TryParse(InString, bIgnoreCase, out OutEnumValue);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Accept a string and return the parsed result as enum value </summary>
		public static bool StringToEnum(in string InString, System.Type InType, out object OutEnumValue, bool bIgnoreCase = true)
		{
			OutEnumValue = null;
			try
			{
				OutEnumValue = global::System.Enum.Parse(InType, InString, bIgnoreCase);
			}
			catch (System.Exception) { }
			return OutEnumValue.IsNotNull();
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Accept a string a try parse it to a Color </summary>
		public static bool StringToColor(in string InString, out Color OutColor, float InAlpha = 0.0f)
		{
			OutColor = Color.clear;
			if (!string.IsNullOrEmpty(InString))
			{
				string[] sArray = InString.TrimStart().TrimInside().TrimEnd().Split(',');
				if (sArray.Length >= 3)
				{
					if (float.TryParse(sArray[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float r) &&
						float.TryParse(sArray[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float g) &&
						float.TryParse(sArray[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float b))
					{
						OutColor.r = r; OutColor.g = g; OutColor.b = b;
						OutColor.a = InAlpha > 0.0f ? InAlpha : (sArray.Length > 3 && float.TryParse(sArray[3], out float a)) ? a : 1.0f;
						return true;
					}
				}
			}
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Accept a string a try parse it to a Quaternion </summary>
		public static bool StringToQuaternion(in string InString, out Quaternion OutQuaternion)
		{
			OutQuaternion = Quaternion.identity;
			if (!string.IsNullOrEmpty(InString))
			{
				string[] sArray = InString.TrimStart().TrimInside().TrimEnd().Split(',');
				if (sArray.Length >= 4)
				{
					if (float.TryParse(sArray[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
						float.TryParse(sArray[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y) &&
						float.TryParse(sArray[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float z) &&
						float.TryParse(sArray[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float w))
					{
						OutQuaternion.Set(x, y, z, w);
						return true;
					}
				}
			}
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a stringified version of a vector2: Output: "X, Y" </summary>
		public static string Vector2ToString(Vector2 InVector2) => $"{InVector2.x}, {InVector2.y}";

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a stringified version of a vector3: Output: "X, Y, Z" </summary>
		public static string Vector3ToString(Vector3 InVector3) => $"{InVector3.x}, {InVector3.y}, {InVector3.z}";

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a stringified version of a vector4: Output: "X, Y, Z, W" </summary>
		public static string Vector4ToString(Vector4 InVector4) => $"{InVector4.x}, {InVector4.y}, {InVector4.z}, {InVector4.w}";


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  Return a stringified version of a quaternion: Output: "X, Y, Z, W" </summary>
		public static string QuaternionToString(Quaternion InQuaternion) => $"{InQuaternion.x}, {InQuaternion.y}, {InQuaternion.z}, {InQuaternion.w}";
	}
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
namespace Utils.Editor // Editor Utils
{
	using System.Reflection;
	using UnityEngine;
	using UnityEditor;

	// Ref: https://forum.unity.com/threads/no-rename.813678/#post-7985412
	public static class ProjectBrowserResetter
	{
		private static System.Type m_ProjectBrowserType = null;
		private static MethodInfo m_ResetViewsMethod = null;

		/////////////////////////////////////////////////////////////////////////////
		static ProjectBrowserResetter()
		{
			m_ProjectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
			m_ResetViewsMethod = m_ProjectBrowserType.GetMethod("ResetViews", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		}

		/////////////////////////////////////////////////////////////////////////////
		public static void Execute()
		{
			foreach (var window in Resources.FindObjectsOfTypeAll(m_ProjectBrowserType))
			{
				m_ResetViewsMethod.Invoke(window, new object[0]);
			}
		}
	}

	public static class GizmosHelper
	{
		private static readonly Mesh BuiltInCapsuleMesh = null;
		private static readonly Vector3[] _baseVertices = null;
		private static readonly Vector3[] newVertices = null;

		static GizmosHelper()
		{
			var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			var origMesh = go.GetComponent<MeshFilter>().sharedMesh;
			BuiltInCapsuleMesh = new Mesh
			{
				vertices = origMesh.vertices,
				normals = origMesh.normals,
				colors = origMesh.colors,
				triangles = origMesh.triangles
			};
			go.Destroy();

			_baseVertices = BuiltInCapsuleMesh.vertices;
			newVertices = new Vector3[_baseVertices.Length];
		}



		//////////////////////////////////////////////////////////////////////////
		public static void DrawCollider(in Collider InCollider, in Color InColor)
		{
			if (InCollider.IsNotNull())
			{
				Color prevColor = Gizmos.color;
				Matrix4x4 prevMatrix = Gizmos.matrix;
				{
					Gizmos.matrix = Matrix4x4.TRS(InCollider.bounds.center, InCollider.transform.rotation, InCollider.transform.lossyScale);
					Gizmos.color = InColor;

					switch (InCollider)
					{
						case BoxCollider box:
						{
							Gizmos.DrawCube(Vector3.zero, box.size);
							break;
						}
						case SphereCollider sphere:
						{
							Gizmos.DrawSphere(Vector3.zero, sphere.radius);
							break;
						}
						case CapsuleCollider capsule:
						{
							DrawWireCapsule(Vector3.zero, InCollider.transform.rotation, capsule.radius, capsule.height, InColor);
							break;
						}
					}

				}
				Gizmos.color = prevColor;
				Gizmos.matrix = prevMatrix;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public static void DrawWireCapsule(in Vector3 InPosition, in Quaternion InRotation, in float InRadius, in float InHeight, in Color InColor)
		{
			for (int i = 0, length = _baseVertices.Length; i < length; i++)
			{
				Vector3 vertex = _baseVertices[i];
				vertex.x *= InRadius * 2f;
				vertex.y *= InHeight * 0.5f;
				vertex.z *= InRadius * 2f;
				newVertices[i].Set(vertex);
			}
			BuiltInCapsuleMesh.vertices = newVertices;
			Gizmos.DrawMesh(BuiltInCapsuleMesh, -1, InPosition, InRotation);
			/*
			UnityEditor.Handles.color = color;

			Matrix4x4 angleMatrix = Matrix4x4.TRS(pos, rot, UnityEditor.Handles.matrix.lossyScale);
			using (new UnityEditor.Handles.DrawingScope(angleMatrix))
			{
				var pointOffset = (height - (radius * 2)) / 2;

				//draw sideways
				UnityEditor.Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, radius);
				UnityEditor.Handles.DrawLine(new Vector3(0, pointOffset, -radius), new Vector3(0, -pointOffset, -radius));
				UnityEditor.Handles.DrawLine(new Vector3(0, pointOffset, radius), new Vector3(0, -pointOffset, radius));
				UnityEditor.Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, radius);
				//draw frontways
				UnityEditor.Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, radius);
				UnityEditor.Handles.DrawLine(new Vector3(-radius, pointOffset, 0), new Vector3(-radius, -pointOffset, 0));
				UnityEditor.Handles.DrawLine(new Vector3(radius, pointOffset, 0), new Vector3(radius, -pointOffset, 0));
				UnityEditor.Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, radius);
				//draw center
				UnityEditor.Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, radius);
				UnityEditor.Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, radius);
			}
			*/
		}
	}

	public class MarkAsDirty : System.IDisposable
	{
		private bool m_Disposed = false;
		private UnityEngine.Object m_UnityObject;

		public MarkAsDirty(UnityEngine.Object InUnityObject)
		{
			m_Disposed = false;
			if (InUnityObject.IsNotNull())
			{
				EditorUtility.SetDirty(InUnityObject);
				m_UnityObject = InUnityObject;
			}
		}

		public void Dispose()
		{
			if (m_Disposed)
			{
				return;
			}
			m_Disposed = true;

			if (m_UnityObject.IsNotNull())
			{
				AssetDatabase.SaveAssetIfDirty(m_UnityObject);
			}
		}
	}

	public class CustomGUIBackgroundColor : System.IDisposable
	{
		private bool m_Disposed = false;
		private readonly Color m_Color = Color.clear;

		public CustomGUIBackgroundColor()
		{
			m_Disposed = false;
			m_Color = GUI.backgroundColor;
		}

		public void Dispose()
		{
			if (m_Disposed)
			{
				return;
			}
			m_Disposed = true;

			GUI.backgroundColor = m_Color;
		}
	}
}
#endif