
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
		public System.Type Get() => System.Type.GetType($"{TypeFullName}, {AssemblyName}");

		//////////////////////////////////////////////////////////////////////////
		public bool TryGetType(out System.Type OutType) => (OutType = System.Type.GetType($"{TypeFullName}, {AssemblyName}")).IsNotNull();

		//////////////////////////////////////////////////////////////////////////
		public override bool Equals(object obj) => Equals(obj as TypeIdentifier);

		//////////////////////////////////////////////////////////////////////////
		public bool Equals(TypeIdentifier other) => other.IsNotNull() && TypeFullName == other.TypeFullName && AssemblyName == other.AssemblyName;

		//////////////////////////////////////////////////////////////////////////
		public static implicit operator TypeIdentifier(in System.Type InType) => new TypeIdentifier(InType);

		//////////////////////////////////////////////////////////////////////////
		public static implicit operator System.Type(in TypeIdentifier InTypeIdentifier) => InTypeIdentifier.Get();

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
		public readonly struct CustomAssertionMessage
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
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using UnityEditor.XR;
	using UnityEngine;

	/// <summary> Can be used to access a Vector3 component </summary>
	public enum EVector3Component
	{
		X, Y, Z
	}

	public static class Math
	{
		public const float EPS = 0.00001f;

		#region TESTS

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return true if the value is between min and max values, otherwise return false </summary>
		/// <param name="Value"></param>
		/// <param name="Min"></param>
		/// <param name="Max"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBetweenValues(in float Value, in float Min, in float Max)
		{
			return Value > Min && Value < Max;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return true if the value is between or equal min and max values, otherwise return false </summary>
		/// <param name="Value"></param>
		/// <param name="Min"></param>
		/// <param name="Max"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBetweenOrEqualValues(in float Value, in float Min, in float Max)
		{
			return Value >= Min && Value <= Max;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return if a position is inside a mesh </summary>
		public static bool IsPointInsideMesh(in Mesh InMesh, in Component InMeshOwner, in Vector3 WorldPosition)
		{
			Vector3 pointInLocalSpace = InMeshOwner.transform.InverseTransformPoint(WorldPosition);
			Plane plane = new Plane();

			Vector3[] verts = InMesh.vertices;
			int[] tris = InMesh.triangles;
			for (int i = 0, count = tris.Length / 3; i < count; i++)
			{
				Vector3 V1 = verts[tris[i * 3]];
				Vector3 V2 = verts[tris[(i * 3) + 1]];
				Vector3 V3 = verts[tris[(i * 3) + 2]];
				plane.Set3Points(V1, V2, V3);
				if (plane.GetSide(pointInLocalSpace))
				{
					return false;
				}
			}
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Tests if point is inside sphere </summary>
		/// <param name="InPoint"></param>
		/// <param name="InSphereCenter"></param>
		/// <param name="InSphereRadius"></param>
		/// <returns></returns>
		public static bool IsPointInsideSphere(in Vector3 InSphereCenter, in float InSphereRadius, in Vector3 InPoint)
		{
			return (InSphereCenter - InPoint).sqrMagnitude <= InSphereRadius * InSphereRadius;
		}


		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		/// <summary> Check if point is inside a not rotated nor scaled cube </summary>
		/// <param name="InPoint"></param>
		/// <param name="InBoxCenter"></param>
		/// <param name="InBoxSize"></param>
		/// <returns></returns>
		public static bool IsPointInsideBox(in Vector3 InBoxCenter, in Vector3 InBoxSize, in Vector3 InPoint)
		{
			return new Bounds(InBoxCenter, InBoxSize).Contains(InPoint);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Check if point is inside a capsule defined by two points and radius </summary>
		/// <param name="InWorldPoint"></param>
		/// <param name="InWorldCapsuleP1"></param>
		/// <param name="InWorldCapsuleP2"></param>
		/// <param name="InCapsuleRadius"></param>
		/// <returns></returns>
		public static bool IsPointInsideCapsule(in Vector3 InWorldCapsuleP1, in Vector3 InWorldCapsuleP2, in float InCapsuleRadius, in Vector3 InWorldPoint)
		{
			return (InWorldPoint - ClosestPointOnSegment3D(InWorldCapsuleP1, InWorldCapsuleP2, InWorldPoint)).sqrMagnitude < InCapsuleRadius * InCapsuleRadius;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Check if point is inside cube </summary>
		/// <param name="InPoint"></param>
		/// <param name="InCubeWorldCenter"></param>
		/// <param name="InCubeRotation"></param>
		/// <param name="InCubeWorldScale"></param>
		/// <returns></returns>
		public static bool IsPointInsideCube(in Vector3 InCubeWorldCenter, in Quaternion InCubeRotation, in Vector3 InCubeWorldScale, in Vector3 InPoint)
		{
			// Invert the rotation and scale of the cube so that we can easily compare the point with a non-transformed cube
			Matrix4x4 inverseTransform = Matrix4x4.Inverse(Matrix4x4.TRS(InCubeWorldCenter, InCubeRotation, InCubeWorldScale));
			Vector3 localPoint = inverseTransform.MultiplyPoint3x4(InPoint);

			// Check if the point is inside the cube bounds
			bool isInside = Mathf.Abs(localPoint.x) <= 0.5f && Mathf.Abs(localPoint.y) <= 0.5f && Mathf.Abs(localPoint.z) <= 0.5f;
			return isInside;
			/*
			if (m_PointInsideBoxCollider == null)
			{
				GameObject go = new GameObject();
				go.SetActive(false);
				GameObject.DontDestroyOnLoad(go);
				m_PointInsideBoxCollider = go.AddComponent<BoxCollider>();
				go.hideFlags = HideFlags.HideAndDontSave;
			}
			m_PointInsideBoxCollider.transform.SetPositionAndRotation(InBoxWorldCenter, InBoxRotation);
			m_PointInsideBoxCollider.size = InBoxSize;

			Vector3 localPoint = m_PointInsideBoxCollider.transform.InverseTransformPoint(InPoint) - InBoxWorldCenter;
			float l_HalfX = (InBoxSize.x * 0.5f);
			float l_HalfY = (InBoxSize.y * 0.5f);
			float l_HalfZ = (InBoxSize.z * 0.5f);
			return IsBetweenValues(localPoint.x, -l_HalfX, l_HalfX) && IsBetweenValues(localPoint.y, -l_HalfY, l_HalfY) && IsBetweenValues(localPoint.z, -l_HalfZ, l_HalfZ);
			*/
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Try to get the projection of a point on a segment </summary>
		/// <param name="OutProjection"></param>
		/// <param name="OutNormalizedTime"></param>
		/// <param name="InPointToProject"></param>
		/// <param name="InSegmentStart"></param>
		/// <param name="InSegmentEnd"></param>
		/// <returns></returns>
		public static bool HasPointOnSegmentProjection(in Vector3 InSegmentStart, in Vector3 InSegmentEnd, in Vector3 InPointToProject, out Vector3 OutProjection, out float OutNormalizedTime)
		{
			OutProjection = Vector3.zero;
			Vector3 segment = InSegmentEnd - InSegmentStart;
			Vector3 vectToPoint = InPointToProject - InSegmentStart;

			// See if closest point is before StartPoint
			float dot1 = Vector3.Dot(vectToPoint, segment);
			if (dot1 <= 0)
			{
				OutNormalizedTime = 0f;
				return false;
			}

			// See if closest point is beyond EndPoint
			float dot2 = Vector3.Dot(segment, segment);
			if (dot2 <= dot1)
			{
				OutNormalizedTime = 1f;
				return false;
			}

			// Closest Point is within segment
			OutProjection = InSegmentStart + (segment * (OutNormalizedTime = (dot1 / dot2)));
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Get the intersection between a line and a plane. </summary>
		/// <param name="OutIntersection"></param>
		/// <param name="InLinePoint"></param>
		/// <param name="InLineDirection"></param>
		/// <param name="InPlaneNormal"></param>
		/// <param name="InPlanePoint"></param>
		/// <returns>If the line and plane are not parallel, the function outputs true, otherwise false.</returns>
		public static bool HasLinePlaneIntersection(in Vector3 InLinePoint, in Vector3 InLineDirection, in Vector3 InPlanePoint, in Vector3 InPlaneNormal, out Vector3 OutIntersection)
		{
			OutIntersection = Vector3.zero;

			//calculate the distance between the linePoint and the line-plane intersection point
			float dotNumerator = Vector3.Dot(InPlanePoint - InLinePoint, InPlaneNormal);
			float dotDenominator = Vector3.Dot(InLineDirection, InPlaneNormal);

			// Check if the line and plane are not parallel
			if (dotDenominator == 0.0f)
			{
				return false;
			}

			float length = dotNumerator / dotDenominator;

			// Create a vector from the linePoint to the intersection point
			Vector3 vector = InLineDirection.normalized * length;

			// The intersection point is linePoint + vector
			OutIntersection = InLinePoint + vector;

			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		/// <param name="InSegmentStart"></param>
		/// <param name="InSegmentEnd"></param>
		/// <param name="InPlanePoint"></param>
		/// <param name="InPlaneNormal"></param>
		/// <param name="OutIntersection"></param>
		/// <returns></returns>
		public static bool HasSegmentPlaneIntersection(in Vector3 InSegmentStart, in Vector3 InSegmentEnd, in Vector3 InPlanePoint, in Vector3 InPlaneNormal, out Vector3 OutIntersection)
		{
			OutIntersection = Vector3.zero;
			Plane plane = new Plane(InPlaneNormal, InPlanePoint);

			// Compute the direction of the line
			Vector3 segmentDirection = InSegmentEnd - InSegmentStart;

			// Check if the line and plane are parallel (no intersection)
			float dot = Vector3.Dot(plane.normal, segmentDirection);
			if (Mathf.Abs(dot) < float.Epsilon)
			{
				return false;
			}

			// Compute the distance between the line and the plane
			float distance = Vector3.Dot(plane.normal, plane.ClosestPointOnPlane(InSegmentStart) - InSegmentStart) / dot;

			// Compute the intersection point
			OutIntersection = InSegmentStart + (distance * segmentDirection);
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		/// <param name="OutIntersectionPoint"></param>
		/// <param name="InSegmentAStart"></param>
		/// <param name="InSegmentAEnd"></param>
		/// <param name="InSegmentBStart"></param>
		/// <param name="InSegmentBEnd"></param>
		/// <returns></returns>
		public static bool HasSegmentSegmentIntersection(in Vector3 InSegmentAStart, in Vector3 InSegmentAEnd, in Vector3 InSegmentBStart, in Vector3 InSegmentBEnd, out Vector3 OutIntersectionPoint)
		{
			Vector3 segment1Direction = InSegmentAEnd - InSegmentAStart;
			Vector3 segment2Direction = InSegmentBEnd - InSegmentBStart;

			Vector3 crossProduct = Vector3.Cross(segment1Direction, segment2Direction);

			if (crossProduct.sqrMagnitude < 0.00001f)
			{
				OutIntersectionPoint = Vector3.zero;
				return false;
			}

			Vector3 seg2ToSeg1 = InSegmentAStart - InSegmentBStart;

			float a = Vector3.Dot(seg2ToSeg1, crossProduct) / crossProduct.sqrMagnitude;
			OutIntersectionPoint = InSegmentAStart + (a * segment1Direction);
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return the closest point of hit on the sphere with the given segment if any </summary>
		/// <param name="InSphereCenter"></param>
		/// <param name="InSphereRadius"></param>
		/// <param name="InSegmentStart"></param>
		/// <param name="InSegmentEnd"></param>
		/// <param name="OutClosestPoint"></param>
		/// <returns></returns>
		public static bool HasSegmentSphereIntersection(in Vector3 InSegmentStart, in Vector3 InSegmentEnd, in Vector3 InSphereCenter, in float InSphereRadius, out Vector3 OutClosestPoint)
		{
			OutClosestPoint = Vector3.zero;
			Vector3 segmentDirection = InSegmentEnd - InSegmentStart;
			Vector3 sphereToSegmentStart = InSegmentStart - InSphereCenter;

			float a = Vector3.Dot(segmentDirection, segmentDirection);
			float b = 2.0f * Vector3.Dot(sphereToSegmentStart, segmentDirection);
			float c = Vector3.Dot(sphereToSegmentStart, sphereToSegmentStart) - (InSphereRadius * InSphereRadius);

			float discriminant = (b * b) - (4.0f * a * c);
			if (discriminant > 0.0f)
			{
				float discSqrt = Mathf.Sqrt(discriminant);
				float t1 = (-b + discSqrt) / (2.0f * a);
				float t2 = (-b - discSqrt) / (2.0f * a);
				float t = Mathf.Min(t1, t2);

				if (t >= 0.0f && t <= 1.0f)
				{
					OutClosestPoint = InSegmentStart + (t * segmentDirection);
					return true;
				}
			}
			return false;
		}


		// TODO Segment-Cylinder Intersection

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return the closest point of hit on the capsule with the given segment if any </summary>
		/// <param name="InSegmentStart"></param>
		/// <param name="InSegmentEnd"></param>
		/// <param name="InCapsulePoint1"></param>
		/// <param name="InCapsulePoint2"></param>
		/// <param name="InCapsuleRadius"></param>
		/// <param name="OutIntersectionPoint"></param>
		/// <returns></returns>
		public static bool HasSegmentCapsuleIntersection(in Vector3 InSegmentStart, in Vector3 InSegmentEnd, in Vector3 InCapsulePoint1, in Vector3 InCapsulePoint2, in float InCapsuleRadius, out Vector3 OutIntersectionPoint)
		{
			// Check capsule cylinder (Not a real cylinfer-line intersection check)
			{
				Vector3 capsuleCenter = Vector3.Lerp(InCapsulePoint1, InCapsulePoint2, 0.5f);
				Vector3 capsuleDirection = (InCapsulePoint2 - InCapsulePoint1).normalized;
				float capsuleLength = Vector3.Distance(InCapsulePoint1, InCapsulePoint2);

				// Find the closest point on the line segment to the center of the capsule
				Vector3 closestPoint = ClosestPointOnSegment3D(InSegmentStart, InSegmentEnd, capsuleCenter);

				// Check if the closest point is within the cylinder part of the capsule
				float distance = Vector3.Dot(closestPoint - InCapsulePoint1, capsuleDirection);
				if (distance >= 0f && distance <= capsuleLength)
				{
					// Check if the distance between the closest point and the capsule axis is within the capsule radius
					float distanceToAxis = (closestPoint - (InCapsulePoint1 + (capsuleDirection * distance))).magnitude;
					if (distanceToAxis <= InCapsuleRadius)
					{
						closestPoint = ClosestPointOnSegment3D(InCapsulePoint1, InCapsulePoint2, closestPoint);
						return HasSegmentSphereIntersection(InSegmentStart, InSegmentEnd, closestPoint, InCapsuleRadius, out OutIntersectionPoint);
					}
				}
			}

			// Check spheres
			if (
				HasSegmentSphereIntersection(InSegmentStart, InSegmentEnd, InCapsulePoint1, InCapsuleRadius, out OutIntersectionPoint)
				||
				HasSegmentSphereIntersection(InSegmentStart, InSegmentEnd, InCapsulePoint2, InCapsuleRadius, out OutIntersectionPoint))
			{
				return true;
			}
			OutIntersectionPoint = Vector3.zero;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		/// <param name="InCapsule1Point1"></param>
		/// <param name="InCapsule1Point2"></param>
		/// <param name="InCapsule1Radius"></param>
		/// <param name="InCapsule2Point1"></param>
		/// <param name="InCapsule2Point2"></param>
		/// <param name="InCapsule2Radius"></param>
		/// <param name="OutPoint1"></param>
		/// <param name="OutPoint2"></param>
		/// <returns></returns>
		/// Ref: https://arrowinmyknee.com/2021/03/15/some-math-about-capsule-collision/
		/// Also: https://wickedengine.net/2020/04/26/capsule-collision-detection/
		public static bool IsCapsuleCapsuleColliding(in Vector3 InCapsule1Point1, in Vector3 InCapsule1Point2, in float InCapsule1Radius,
			in Vector3 InCapsule2Point1, in Vector3 InCapsule2Point2, in float InCapsule2Radius, out Vector3 OutPoint1, out Vector3 OutPoint2)
		{
			// Compute (squared) distance between the inner structures of the capsules
			float s = 0f, t = 0f;

			//TODO Add AABB check

			float dist2 = ClosestPointsOfTwoSegments(InCapsule1Point1, InCapsule1Point2, InCapsule2Point1, InCapsule2Point2, out s, out t, out OutPoint1, out OutPoint2);
			// If (squared) distance smaller than (squared) sum of radii, they collide
			float radius = InCapsule1Radius + InCapsule2Radius;
			return dist2 <= (radius * radius);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> 
		/// Computes closest points OutP1 and OutP2 of S1(s)=S1P1+s*(S1P2-S1P1) and S2(t)=S2P1+t*(S2P2-S2P1), returning s and t.
		/// Function result is squared distance between between S1(s) and S2(t)
		/// </summary>
		/// <param name="S1P1"> Segment 1 point 1 </param>
		/// <param name="S1P2"> Segment 1 point 2 </param>
		/// <param name="S2P1"> Segment 2 point 1 </param>
		/// <param name="S2P2"> Segment 2 point 2 </param>
		/// <param name="s"></param>
		/// <param name="t"></param>
		/// <param name="OutP1"></param>
		/// <param name="OutP2"></param>
		/// <returns> Squared distance between between S1(s) and S2(t) </returns>
		private static float ClosestPointsOfTwoSegments(in Vector3 S1P1, in Vector3 S1P2, in Vector3 S2P1, in Vector3 S2P2, out float s, out float t, out Vector3 OutP1, out Vector3 OutP2)
		{
			Vector3 d1 = S1P2 - S1P1; // Direction vector of segment S1
			Vector3 d2 = S2P2 - S2P1; // Direction vector of segment S2
			Vector3 r = S1P1 - S2P1;
			float a = Vector3.Dot(d1, d1); // Squared length of segment S1, always nonnegative
			float e = Vector3.Dot(d2, d2); // Squared length of segment S2, always nonnegative
			float f = Vector3.Dot(d2, r);

			// Check if either or both segments degenerate into points
			if (a <= Vector3.kEpsilon)
			{
				if (e <= Vector3.kEpsilon)
				{
					// Both segments degenerate into points
					s = t = 0.0f;
					OutP1 = S1P1;
					OutP2 = S2P1;
					return Vector3.Dot(OutP1 - OutP2, OutP1 - OutP2);
				}
				else
				{
					// First segment degenerates into a point
					s = 0.0f;
					t = f / e; // s = 0 => t = (b*s + f) / e = f / e
					t = Mathf.Clamp01(t);
				}
			}
			else
			{
				float c = Vector3.Dot(d1, r);
				if (e <= Vector3.kEpsilon)
				{
					// Second segment degenerates into a point
					t = 0.0f;
					s = Mathf.Clamp01(-c / a); // t = 0 => s = (b*t - c) / a = -c / a
				}
				else
				{
					// The general nondegenerate case starts here
					float b = Vector3.Dot(d1, d2);
					float denom = (a * e) - (b * b); // Always nonnegative
												 // If segments not parallel, compute closest point on L1 to L2 and
												 // clamp to segment S1. Else pick arbitrary s (here 0)
					if (denom != 0.0f)
					{
						s = Mathf.Clamp01(((b * f) - (c * e)) / denom);
					}
					else s = 0.0f;
					// Compute point on L2 closest to S1(s) using
					// t = Dot((P1 + D1*s) - P2,D2) / Dot(D2,D2) = (b*s + f) / e
					t = ((b * s) + f) / e;
					// If t in [0,1] done. Else clamp t, recompute s for the new value
					// of t using s = Dot((P2 + D2*t) - P1,D1) / Dot(D1,D1)= (t*b - c) / a
					// and clamp s to [0, 1]
					if (t < 0.0f)
					{
						t = 0.0f;
						s = Mathf.Clamp01(-c / a);
					}
					else if (t > 1.0f)
					{
						t = 1.0f;
						s = Mathf.Clamp01((b - c) / a);
					}
				}
			}
			OutP1 = S1P1 + (d1 * s);
			OutP2 = S2P1 + (d2 * t);
			return Vector3.Dot(OutP1 - OutP2, OutP1 - OutP2);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		/// <param name="InSegmentStart"></param>
		/// <param name="InSegmentEnd"></param>
		/// <param name="InCircleCenter"></param>
		/// <param name="InCircleNormal"></param>
		/// <param name="InCircleRadius"></param>
		/// <returns></returns>
		public static bool SegmentTo3DOrientedCircleIntersection(in Vector3 InSegmentStart, in Vector3 InSegmentEnd, in Vector3 InCircleCenter, in Vector3 InCircleNormal, in float InCircleRadius)
		{
			Vector3 segmentDir = (InSegmentEnd - InSegmentStart).normalized;
			if (new Plane(InCircleNormal, InCircleCenter).Raycast(new Ray(InSegmentStart, segmentDir), out float distance) && distance < InCircleRadius)
			{
				Vector3 intersectionPoint = InSegmentStart + (distance * segmentDir);
				if ((intersectionPoint - InCircleCenter).sqrMagnitude < (InCircleRadius * InCircleRadius))
				{
					Vector3 closestPoint = ClosestPointOnSegment3D(InSegmentStart, InSegmentEnd, InCircleCenter);
					return Vector3.Distance(closestPoint, intersectionPoint) <= 0.005f; // Need some tollerance
				}
			}
			return false;
		}

		#endregion // TESTS

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return the closest point (unclamped) on the line of the given point </summary>
		/// <param name="InPoint"></param>
		/// <param name="InfiniteLinePointA"></param>
		/// <param name="InLineDirection"></param>
		/// <returns></returns>
		public static Vector3 PointProjectionOnInfiniteLine2D(in Vector2 InfiniteLinePointA, in Vector2 InLineDirection, in Vector2 InPoint)
		{
			float closestPoint = Vector2.Dot(InPoint - InfiniteLinePointA, InLineDirection) / Vector2.Dot(InLineDirection, InLineDirection);
			return InfiniteLinePointA + (closestPoint * InLineDirection);
		}

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return the closest point (unclamped) on the line of the given point </summary>
		/// <param name="InLineStart"></param>
		/// <param name="InLineDirection"></param>
		/// <param name="InPoint"></param>
		/// <returns></returns>
		public static Vector3 PointProjectionOnInfiniteLine3D(in Vector3 InLineStart, in Vector3 InLineDirection, in Vector3 InPoint)
		{
			float closestPoint = Vector3.Dot(InPoint - InLineStart, InLineDirection) / Vector3.Dot(InLineDirection, InLineDirection);
			return InLineStart + (closestPoint * InLineDirection);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a clamped point on this segment that is the closest one by the given point </summary>
		/// <param name="InSegmentStart"></param>
		/// <param name="InSegmentEnd"></param>
		/// <param name="InPoint"></param>
		/// <returns></returns>
		public static Vector3 ClosestPointOnSegment2D(in Vector2 InSegmentStart, in Vector2 InSegmentEnd, in Vector2 InPoint)
		{
			Vector2 segmentDirection = InSegmentEnd - InSegmentStart;
			float closestPoint = Vector2.Dot(InPoint - InSegmentStart, segmentDirection) / Vector2.Dot(segmentDirection, segmentDirection);
			closestPoint = Mathf.Clamp01(closestPoint);
			return InSegmentStart + (closestPoint * segmentDirection);
		}



		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a clamped point on this segment that is the closest one by the given point </summary>
		/// <param name="InSegmentStart"></param>
		/// <param name="InSegmentEnd"></param>
		/// <param name="InPoint"></param>
		/// <returns></returns>
		public static Vector3 ClosestPointOnSegment3D(in Vector3 InSegmentStart, in Vector3 InSegmentEnd, in Vector3 InPoint)
		{
			Vector3 ap = InPoint - InSegmentStart;
			Vector3 ab = InSegmentEnd - InSegmentStart;
			float magnitudeAB = ab.sqrMagnitude;
			float dotProduct = Vector3.Dot(ap, ab);
			float distance = Mathf.Clamp01(dotProduct / magnitudeAB);
			return InSegmentStart + (distance * ab);
			/*
			Vector3 SegmentDirection = InSegmentEnd - InSegmentStart;
			float closestPoint = Vector3.Dot(SegmentDirection, InPoint - InSegmentStart) / Vector3.Dot(SegmentDirection, SegmentDirection);
			closestPoint = Mathf.Clamp01(closestPoint);
			return InSegmentStart + (closestPoint * SegmentDirection);
			*/
		}
		/*
		public static bool AreThereMirroredProjectionsOf(Vector3 A, Vector3 B, Vector3 C, Vector3 D, out Vector3 OutClosest1, out Vector3 OutClosest2)
		{
			OutClosest1 = OutClosest2 = Vector3.zero;

			if (HasPointOnSegmentProjection(A, B, C, out Vector3 projC, out float _) && HasPointOnSegmentProjection(A, B, D, out Vector3 projD, out float _))
			{
				OutClosest1 = ClosestPointOnSegment3D(C, D, projC);
				OutClosest2 = ClosestPointOnSegment3D(C, D, projD);
				return true;
			}
			
			return false;
		}
		*/


		//////////////////////////////////////////////////////////////////////////
		/// Ref: https://en.wikipedia.org/wiki/Feature_scaling
		/// <summary>
		/// Return the value that lies between MinValue and MaxValue scaled in the given limits <br/>
		/// Ex: 0.2f, 0f, 1f, 1f, 100f = 20f <br/>
		/// Given a current value between minValue and maxValue, reinterpret the normalized value in scale [minOutput - maxOutput]
		/// </summary>
		/// <param name="InCurrentValue"></param>
		/// <param name="InMinValue"></param>
		/// <param name="InMaxValue"></param>
		/// <param name="InMinOutput"></param>
		/// <param name="InMaxOutput"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ScaleBetween(in float InCurrentValue, in float InMinValue, in float InMaxValue, in float InMinOutput, in float InMaxOutput)
		{
			float localCurrentValue = Mathf.Clamp(InCurrentValue, InMinValue, InMaxValue);
			if (localCurrentValue - InMinValue == 0f)
			{
				localCurrentValue = InMinValue + Mathf.Epsilon;
			}

			float normalizedValue = (localCurrentValue - InMinValue) / (InMaxValue - InMinValue);
			float value = InMinOutput + ((InMaxOutput - InMinOutput) * normalizedValue);
			return value;
		}


		//////////////////////////////////////////////////////////////////////////
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float BoolToMinusOneOrPlusOne(in bool InValue)
		{
			return InValue ? 1f : -1f;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> With a optional Epsilon, determines if value is similar to Zero </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SimilarZero(in float a, float cmp = EPS)
		{
			return Mathf.Abs(a) < cmp;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a clamped value </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp(in float Value, in float Min, in float Max)
		{
			return (Value > Max) ? Max : (Value < Min) ? Min : Value;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Assign a value the clamp itself and return true if value was still in range on [min-max] </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ClampResult(ref float value, in float expressionValue, in float min, in float max)
		{
			bool bResult = expressionValue >= min && expressionValue <= max;
			value = (expressionValue > max) ? max : (expressionValue < min) ? min : expressionValue;
			return bResult;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Get planar squared distance between a plane and a point projected on it </summary>
		/// <returns> Planar Squared Distance </returns>
		public static float PlanarSqrDistance(in Vector3 InPoint, in Vector3 InPlanePoint, in Vector3 InPlaneNormal)
		{
			// with given plane normal, project position1 on position2 plane
			Vector3 projectedPoint = ProjectPointOnPlane(InPoint, InPlaneNormal, InPlanePoint);

			return (InPoint - projectedPoint).sqrMagnitude;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Get planar distance between two positions, position1 is projected on position2 plane </summary>
		/// <returns>Planar Distance</returns>
		public static float PlanarDistance(in Vector3 InPoint, in Vector3 InPlanePoint, in Vector3 InPlaneNormal)
		{
			float sqrDistance = PlanarSqrDistance(InPoint, InPlanePoint, InPlaneNormal);

			return Mathf.Sqrt(sqrDistance);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Get then angle between the given vectors projections on the plane </summary>
		/// <returns></returns>
		public static float PlanarAngle(in Vector3 InDirectionA, in Vector3 InDirectionB, in Vector3 InPlaneNormal)
		{
			Vector3 directionA = InDirectionA.normalized;
			Vector3 directionB = InDirectionB.normalized;
			Vector3 planeNormal = InPlaneNormal.normalized;
			Vector3 projected1 = Vector3.ProjectOnPlane(directionA, planeNormal);
			Vector3 projected2 = Vector3.ProjectOnPlane(directionB, planeNormal);
			float sign = Mathf.Sign(Vector3.Dot(planeNormal, Vector3.Cross(directionA, directionB)));
			return Vector3.Angle(projected1, projected2) * sign;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Create a vector of direction "vector" with length "size" </summary>
		public static Vector3 SetVectorLength(in Vector3 InVector, in float InSize)
		{
			//normalize the vector
			Vector3 vectorNormalized = Vector3.Normalize(InVector);

			//scale the vector
			return vectorNormalized *= InSize;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> This function returns a point which is a projection from a point to a plane. </summary>
		public static Vector3 ProjectPointOnPlane(in Vector3 InPoint, in Vector3 InPlaneNormal, in Vector3 InPlanePoint)
		{
			//First calculate the distance from the point to the plane:
			//float distance = Vector3.Dot( planeNormal, ( point - planePoint ) );

			//Reverse the sign of the distance
			//distance *= -1;

			//Get a translation vector
			//Vector3 translationVector = SetVectorLength( planeNormal, distance );

			//Translate the point to form a projection
			//return point - translationVector;

			// Dot product of two normalize vector means the cos of the angle between this two vectors
			// If it's positive means a < 180 angle and negative and angle >= 180
			// Dot product can also be: ( ax × bx ) + ( ay × by ), that's the point
			float pointPlaneDistance = Vector3.Dot(InPlaneNormal, InPoint - InPlanePoint);

			return InPoint - (InPlaneNormal * pointPlaneDistance);
		}


		//////////////////////////////////////////////////////////////////////////
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
		/// <summary> Get a direction vector from polar coordinates </summary>>
		/// <param name="InRadius">The radius, distance from the center</param>
		/// <param name="InDegreePolarAngle">The altitude in degrees (Rotation around the up axis)</param>
		/// <param name="InDegreeAzimuthalAngle">The longitude in degrees (Rotation around the right axis)</param>
		/// <returns>Directional vector</returns>
		/// Ref: https://gamedev.stackexchange.com/a/81715
		public static Vector3 SphericalToCartesian(in float InRadius, in float InDegreePolarAngle, in float InDegreeAzimuthalAngle)
		{
			float a = InRadius * Mathf.Cos(InDegreeAzimuthalAngle * Mathf.Deg2Rad);
			return new Vector3
			(
				x: a * Mathf.Cos(InDegreePolarAngle * Mathf.Deg2Rad),
				y: InRadius * Mathf.Sin(InDegreeAzimuthalAngle * Mathf.Deg2Rad),
				z: a * Mathf.Sin(InDegreePolarAngle * Mathf.Deg2Rad)
			);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Get polar coordinates from a direction vector </summary>
		/// <param name="InCartesianCoords"></param>
		/// <param name="OutRadius">The radius, distance from the center</param>
		/// <param name="OutDegreePolarAngle">The altitude in degrees (Rotation around the up axis)</param>
		/// <param name="OutDegreeAzimuthalAngle">The longitude in degrees (Rotation around the right axis)</param>
		/// Ref: https://gamedev.stackexchange.com/a/81715
		public static void CartesianToSpherical(in Vector3 InCartesianCoords, out float OutRadius, out float OutDegreePolarAngle, out float OutDegreeAzimuthalAngle)
		{
			Vector3 localCartesianCoords = InCartesianCoords;
			if (localCartesianCoords.x == 0f)
			{
				localCartesianCoords.x = Mathf.Epsilon;
			}

			OutRadius = localCartesianCoords.magnitude;
			OutDegreePolarAngle = Mathf.Atan(localCartesianCoords.z / localCartesianCoords.x) * Mathf.Rad2Deg;
			if (localCartesianCoords.x < 0f)
			{
				OutDegreePolarAngle += Mathf.PI;
			}

			OutDegreeAzimuthalAngle = Mathf.Asin(localCartesianCoords.y / OutRadius) * Mathf.Rad2Deg;
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
			return targetPosition + (t * velocityDelta);
		}


		//////////////////////////////////////////////////////////////////////////
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
			dir.y = dist * Mathf.Tan(a);							// set dir to the elevation angle.
			dist += height / Mathf.Tan(a);							// Correction for small height differences

			// Calculate the velocity magnitude
			float velocity = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
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
			return (d >= -0.0001f && d <= 0.0002f) ? 0.0f : (-Vector3.Dot(PVec, SVec) / d);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Returns the quadratic interpolation of given vectors </summary>
		public static Vector3 GetPointLinear(in Vector3 p0, in Vector3 p1, in Vector3 p2, in float t)
		{
			Vector3 v1 = Vector3.Lerp(p0, p1, t);
			Vector3 v2 = Vector3.Lerp(p1, p2, t);
			return Vector3.Lerp(v1, v2, t);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Returns the cubic interpolation of given vectors </summary>>
		public static Vector3 GetPointLinear(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, in float t)
		{
			Vector3 v1 = GetPointLinear(p0, p1, p2, t);
			Vector3 v2 = GetPointLinear(p1, p2, p3, t);
			return Vector3.Lerp(v1, v2, t);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a Five dimensional interpolation of given vectors </summary>
		public static Vector3 GetPointLinear(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, in Vector3 p4, in float t)
		{
			Vector3 v1 = GetPointLinear(p0, p1, p2, p3, t);
			Vector3 v2 = GetPointLinear(p1, p2, p3, p4, t);
			return Vector3.Lerp(v1, v2, t);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> </summary>
		public static Quaternion GetRotation(in Quaternion r0, in Quaternion r1, in Quaternion r2, in float t)
		{
			//	float slerpT = 2.0f * t * ( 1.0f - t );
			Quaternion q1 = Quaternion.Slerp(r0, r1, t);
			Quaternion q2 = Quaternion.Slerp(r1, r2, t);
			return Quaternion.Slerp(q1, q2, t);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a spherical quadrangle interpolation of given quaternions </summary>
		public static Quaternion GetRotation(in Quaternion r0, in Quaternion r1, in Quaternion r2, in Quaternion r3, in float t)
		{
			Quaternion q1 = GetRotation(r0, r1, r2, t);
			Quaternion q2 = GetRotation(r1, r2, r3, t);
			return q1.Slerp(q2, t);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a cubic interpolated vector </summary>
		public static Vector3 GetPoint(in Vector3 p0, in Vector3 p1, in Vector3 p2, float t)
		{
			t = Mathf.Clamp01(t);
			float oneMinusT = 1f - t;
			return (oneMinusT * oneMinusT * p0) + (2f * oneMinusT * t * p1) + (t * t * p2);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a quadratic interpolated vector </summary>
		public static Vector3 GetPoint(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);
			float OneMinusT = 1f - t;
			return
				(OneMinusT * OneMinusT * OneMinusT * p0) +
				(3f * OneMinusT * OneMinusT * t * p1) +
				(3f * OneMinusT * t * t * p2) +
				(t * t * t * 1.0f * p3);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a Five dimensional interpolated vector </summary>
		public static Vector3 GetPoint(in Vector3 p0, in Vector3 p1, in Vector3 p2, in Vector3 p3, in Vector3 p4, float t)
		{
			t = Mathf.Clamp01(t);
			float OneMinusT = 1f - t;
			return
				(OneMinusT * OneMinusT * OneMinusT * OneMinusT * p0) +
				(4f * OneMinusT * OneMinusT * OneMinusT * t * p1) +
				(5f * OneMinusT * OneMinusT * t * t * p2) +
				(4f * OneMinusT * t * t * t * p3) +
							(t * t * t * t * p4);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a Spline interpolation (catmull Rom) between given points </summary>
		public static Vector3 GetPoint(in Vector3[] InPointsCollection, float t)
		{
			if (InPointsCollection == null || InPointsCollection.Length < 4)
			{
				UnityEngine.Debug.Log("GetPoint Called with invalid points array, required at least 4 points");
				UnityEngine.Debug.DebugBreak();
				return Vector3.zero;
			}

			int length = InPointsCollection.Length;
			bool bIsReversed = t < 0.0f;
			t = Mathf.Abs(t);

			int numSections = length - 3;
			int currPt = Mathf.Min(Mathf.FloorToInt(t * numSections), numSections - 1);
			if (bIsReversed)
			{
				currPt = length - 1 - currPt;
			}

			float u = (t * numSections) - (
				bIsReversed ?
					(length - 1f - currPt)
					:
					currPt
				)
			;
			u = Mathf.Clamp01(u);

			Vector3 a = InPointsCollection[currPt + 0];
			Vector3 b = InPointsCollection[currPt + 1];
			Vector3 c = InPointsCollection[currPt + 2];
			Vector3 d = InPointsCollection[currPt + 3];

			// catmull Rom interpolation
			return .5f *
			(
				((-a + (3f * b) - (3f * c) + d) * (u * u * u)) +
				(((2f * a) - (5f * b) + (4f * c) - d) * (u * u)) +
				((-a + c) * u) +
				(2f * b)
			);
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		/// <param name="InInterpolant"></param>
		/// <param name="OutSectionsSubInterpolants"></param>
		public static void FindInterpolatedValues(in float InInterpolant, in float[] OutSectionsSubInterpolants)
		{
			int iSectionCount = OutSectionsSubInterpolants.Length;
			if (OutSectionsSubInterpolants.IsNotNull() && iSectionCount > 0)
			{
				float interpolant = Mathf.Clamp01(InInterpolant);
				float subInterpolantStep = 1f / iSectionCount;
				for (int index = 0; index < iSectionCount; ++index)
				{
					float currentInterpolant = Mathf.Clamp01(interpolant - (subInterpolantStep * index));
					OutSectionsSubInterpolants[index] = Mathf.Clamp01(currentInterpolant / subInterpolantStep);
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		public static bool Get4Elements<T>(in IList<T> InCollection, float t, out T a, out T b, out T c, out T d)
		{
			a = b = c = d = default;
			if (InCollection == null || InCollection.Count < 4)
			{
				UnityEngine.Debug.Log("GetPoint Called with invalid points collection, required at least 4 points");
				UnityEngine.Debug.DebugBreak();
				return false;
			}
			int length = InCollection.Count;

			bool bIsReversed = t < 0.0f;
			t = Mathf.Abs(t);

			int numSections = length - 3;
			int currPt = Mathf.Min(Mathf.FloorToInt(t * numSections), numSections - 1);
			if (bIsReversed)
			{
				currPt = length - 1 - currPt;
			}

			a = bIsReversed ? InCollection[currPt - 0] : InCollection[currPt + 0];
			b = bIsReversed ? InCollection[currPt - 1] : InCollection[currPt + 1];
			c = bIsReversed ? InCollection[currPt - 2] : InCollection[currPt + 2];
			d = bIsReversed ? InCollection[currPt - 2] : InCollection[currPt + 2];
			return true;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Calculate the bounds of a gameobject encapsulating all activbe cooliders bounds </summary>
		/// <param name="InTransform"></param>
		/// <returns></returns>
		public static Bounds GetBoundsOf(in GameObject InSource, in bool InIncludeColliders = false) => GetBoundsOf(InSource.transform, InIncludeColliders);


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Calculate the bounds of a gameobject encapsulating all activbe cooliders bounds </summary>
		/// <param name="InTransform"></param>
		/// <returns></returns>
		public static Bounds GetBoundsOf(in Component InSource, in bool InIncludeColliders = false) => GetBoundsOf(InSource.transform, InIncludeColliders);


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Calculate the bounds of a gameobject encapsulating all activbe cooliders bounds </summary>
		/// <param name="InGameObject"></param>
		/// <returns></returns>
		public static Bounds GetBoundsOf(in Transform InSource, in bool InIncludeColliders = false)
		{
			Bounds outResult = new Bounds(InSource.position, Vector3.zero);
			if (InIncludeColliders)
			{
				foreach (Collider item in InSource.GetComponentsInChildren<Collider>(includeInactive: false))
				{
					if (!item.isTrigger)
					{
						outResult.Encapsulate(item.bounds);
					}
				}
			}

			foreach (Renderer item in InSource.GetComponentsInChildren<Renderer>(includeInactive: false))
			{
				if (item.enabled)
				{
					outResult.Encapsulate(item.bounds);
				}
			}

			return outResult;
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		/// <param name="InVolumePosition"></param>
		/// <param name="InVolumeRotation"></param>
		/// <param name="InVolumeSize"></param>
		/// <param name="InCountForAxis"></param>
		/// <returns></returns>
		public static (Vector3, Vector3)[] IterateBoxVolume(in Vector3 InVolumePosition, in Quaternion InVolumeRotation, in Vector3 InVolumeSize, in Vector3 InCountForAxis)
		{
			List<(Vector3, Vector3)> points = new List<(Vector3, Vector3)>();
			{
				Vector3 voxelSize = new Vector3
				(
					x: InVolumeSize.x / InCountForAxis.x,
					y: InVolumeSize.y / InCountForAxis.y,
					z: InVolumeSize.z / InCountForAxis.z
				);

				Vector3 halfVoxelSize = voxelSize * 0.5f;
				Vector3 halfVolumeSize = InVolumeSize * 0.5f;
				Vector3 currentPosition = -halfVolumeSize + (voxelSize * 0.5f);

				while (true)
				{
					points.Add((InVolumePosition + (InVolumeRotation * currentPosition), voxelSize));

					if ((currentPosition.x += voxelSize.x) > halfVolumeSize.x)
					{
						if ((currentPosition.z += voxelSize.z) > halfVolumeSize.z)
						{
							if ((currentPosition.y += voxelSize.y) > halfVolumeSize.y)
							{
								break;
							}
							currentPosition.z = -halfVolumeSize.z + halfVoxelSize.z;
						}
						currentPosition.x = -halfVolumeSize.x + halfVoxelSize.x;
					}
				}
			}
			return points.ToArray();
		}


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  </summary>
		/// <param name="InSpherePosition"></param>
		/// <param name="InSphereRadius"></param>
		/// <param name="InCountForAxis"></param>
		/// <returns></returns>
		/// // Ref: https://stackoverflow.com/a/47416720
		public static (Vector3, Vector3)[] IterateSphereVolume(in Vector3 InSpherePosition, in float InSphereRadius, float InMaxLats, float InMaxLongs)
		{
			List<(Vector3, Vector3)> points = new List<(Vector3, Vector3)>();
			{
				InMaxLats = Mathf.Max(InMaxLats, 1f);
				InMaxLongs = Mathf.Max(InMaxLongs, 1f);
				
				Vector3 voxelSize = new Vector3
				(
					x: InSphereRadius / InMaxLongs,
					y: InSphereRadius / InMaxLats,
					z: InSphereRadius / InMaxLongs
				);
				
				float azimuthStep = Mathf.RoundToInt(180f / (InMaxLats + 1f));
				for (float currentAzimuthalAngle = 0f; currentAzimuthalAngle <= 180f; currentAzimuthalAngle += azimuthStep)
				{
					float voxelsCount = Mathf.RoundToInt(Mathf.Max((InMaxLongs * Mathf.Sin(currentAzimuthalAngle * Mathf.Deg2Rad)), 1f));
					float polarStep = 360f / voxelsCount;
					for (float currentPolarAngle = 0f; currentPolarAngle < 360f; currentPolarAngle += polarStep)
					{
						Vector3 voxelWorldCenter = SphericalToCartesian(InSphereRadius/*?*/*0.5f, currentPolarAngle, currentAzimuthalAngle - 90f);
						points.Add((InSpherePosition + voxelWorldCenter, voxelSize));
					}
				}
				
			}
			return points.ToArray();
		}


		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		public static class BezierCurve
		{
			//////////////////////////////////////////////////////////////////////////
			/// <summary>  </summary>
			/// <param name="OutPosition"></param>
			/// <param name="InTime"></param>
			/// <param name="InWayPoints"></param>
			/// <returns></returns>
			public static bool Evaluate(out Vector3 OutPosition, in float InTime, in Vector3[] InWayPoints)
			{
				OutPosition = Vector3.zero;
				bool bResult = false;
				if (IsValisCurve(InWayPoints))
				{
					OutPosition = EvaluateNoCheck(InTime, InWayPoints);
					bResult = true;
				}
				return bResult;
			}


			//////////////////////////////////////////////////////////////////////////
			/// <summary>  </summary>
			/// <param name="InWayPoints"></param>
			/// <returns></returns>
			public static bool IsValisCurve(in Vector3[] InWayPoints) => InWayPoints?.Length > 3;


			//////////////////////////////////////////////////////////////////////////
			/// <summary>  </summary>
			/// <param name="aP"></param>
			/// <param name="InWayPoints"></param>
			/// <returns></returns>
			public static float ClosestTimeOnBezier(Vector3 aP, in Vector3[] InWayPoints)
			{
				float t = 0f;
				if (IsValisCurve(InWayPoints))
				{
					t = BestFittingTime(InWayPoints, aP, 0f, 1f, 10f);
					float delta = 1.0f / 10.0f;
					for (int i = 0; i < 4; i++)
					{
						t = BestFittingTime(InWayPoints, aP, t - delta, t + delta, 10f);
						delta /= 9f;
					}
				}
				return t;
			}


			//////////////////////////////////////////////////////////////////////////
			/// <summary>  </summary>
			/// <param name="aP"></param>
			/// <param name="InWayPoints"></param>
			/// <returns></returns>
			public static Vector3 ClosestPointOnBezier(Vector3 aP, in Vector3[] InWayPoints)
			{
				Vector3 outValue = Vector3.zero;
				if (IsValisCurve(InWayPoints))
				{
					outValue = EvaluateNoCheck(ClosestTimeOnBezier(aP, InWayPoints), InWayPoints);
				}
				return outValue;
			}


			//////////////////////////////////////////////////////////////////////////
			public static Vector3[] GetDensePositions(Vector3[] InWayPoints, uint InSteps)
			{
				Vector3[] outValue = new Vector3[InSteps];
				uint currentStep = 0u;
				float step = 1f / (InSteps - 1);
				float t = 0f;
				while (t < 1f)
				{
					outValue[currentStep] = EvaluateNoCheck(t, InWayPoints);
					currentStep++;
					t += step;
				}

				outValue[InSteps - 1] = EvaluateNoCheck(1f, InWayPoints);
				return outValue;
			}


			//////////////////////////////////////////////////////////////////////////
			private static Vector3 EvaluateNoCheck(in float InTime, in Vector3[] InWayPoints)
			{
				int length = InWayPoints.Length;
				bool bIsReversed = InTime < 0.0f;
				float t = Mathf.Abs(InTime);

				int numSections = length - 3;
				int currPt = Mathf.Min(Mathf.FloorToInt(t * numSections), numSections - 1);
				if (bIsReversed)
				{
					currPt = length - 1 - currPt;
				}

				float u = (t * numSections) - (
					bIsReversed ?
						(length - 1f - currPt)
						:
						currPt
					)
				;
				u = Mathf.Clamp01(u);

				Vector3 a = InWayPoints[currPt + 0];
				Vector3 b = InWayPoints[currPt + 1];
				Vector3 c = InWayPoints[currPt + 2];
				Vector3 d = InWayPoints[currPt + 3];
				return 0.5f *
				(
					((-a + (3f * b) - (3f * c) + d) * (u * u * u)) +
					(((2f * a) - (5f * b) + (4f * c) - d) * (u * u)) +
					((-a + c) * u) +
					(2f * b)
				);
			}


			//////////////////////////////////////////////////////////////////////////
			private static float BestFittingTime(in Vector3[] points, Vector3 aP, float aStart, float aEnd, float aSteps)
			{
				float Res = 0f;
				if (points?.Length > 3)
				{
					aStart = Mathf.Clamp01(aStart);
					aEnd = Mathf.Clamp01(aEnd);
					float step = (aEnd - aStart) / aSteps;
					float Ref = float.MaxValue;
					for (float i = 0f; i < aSteps; i++)
					{
						float t = aStart + (step * i);
						float L = (EvaluateNoCheck(t, points) - aP).sqrMagnitude;
						if (L < Ref)
						{
							Ref = L;
							Res = t;
						}
					}
				}
				return Res;
			}
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

namespace Utils // UI
{
	using UnityEngine;

	public static class UI
	{
		public static Quaternion RotationToFaceTarget2D(in Transform InSource, in Transform InTarget)
		{
			Vector3 vectorToTarget = InTarget.position - InSource.position;
			float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
			return Quaternion.AngleAxis(angle, Vector3.forward);
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
#if UNITY_EDITOR
namespace Utils.Editor // Editor Utils
{
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;

	// Ref: https://forum.unity.com/threads/no-rename.813678/#post-7985412
	public static class ProjectBrowserResetter
	{
		private readonly static System.Type m_ProjectBrowserType = null;
		private readonly static MethodInfo m_ResetViewsMethod = null;

		/////////////////////////////////////////////////////////////////////////////
		static ProjectBrowserResetter()
		{
			m_ProjectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
			m_ResetViewsMethod = m_ProjectBrowserType.GetMethod("ResetViews", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		}

		/////////////////////////////////////////////////////////////////////////////
		public static void Execute()
		{
			foreach (Object window in Resources.FindObjectsOfTypeAll(m_ProjectBrowserType))
			{
				m_ResetViewsMethod.Invoke(window, new object[0]);
			}
		}
	}

	public static class Helpers
	{
		public static void ScheduleEditorAction(System.Action InAction)
		{
			void DoAction()
			{
				InAction();
				EditorApplication.update -= DoAction;
			}
			EditorApplication.update += DoAction;
		}
	}

	public static class GizmosHelper
	{
		private static Mesh s_BuiltInCapsuleMesh = null;
		private static Vector3[] m_CapsuleVertices => BuiltInCapsuleMesh.vertices;
		private static Vector3[] m_CapsuleNewVertices = null;
		private static Mesh BuiltInCapsuleMesh
		{
			get
			{
				if (s_BuiltInCapsuleMesh.IsNull())
				{
					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
					Mesh origMesh = go.GetComponent<MeshFilter>().sharedMesh;
					s_BuiltInCapsuleMesh = new Mesh
					{
						vertices = origMesh.vertices,
						normals = origMesh.normals,
						colors = origMesh.colors,
						triangles = origMesh.triangles
					};
					go.Destroy();
					m_CapsuleNewVertices = new Vector3[m_CapsuleVertices.Length];
				}
				return s_BuiltInCapsuleMesh;
			}
		}



		//////////////////////////////////////////////////////////////////////////
		public static void DrawCollider(in Collider InCollider, in Color InColor)
		{
			if (InCollider.IsNotNull())
			{
				//Matrix4x4 prevMatrix = Gizmos.matrix;
				using (new UseGizmoColor(InColor))
				{
					//Gizmos.matrix = Matrix4x4.TRS(InCollider.bounds.center, InCollider.transform.rotation, InCollider.transform.lossyScale);

					switch (InCollider)
					{
						case BoxCollider box:
						{
							using (new UseGizmoMatrix(Matrix4x4.TRS(InCollider.bounds.center, InCollider.transform.rotation, InCollider.transform.lossyScale)))
							{
								Gizmos.DrawCube(Vector3.zero, box.size);
							}
							break;
						}
						case SphereCollider sphere:
						{
							using (new UseGizmoMatrix(Matrix4x4.TRS(InCollider.bounds.center, InCollider.transform.rotation, InCollider.transform.lossyScale)))
							{
								Gizmos.DrawSphere(Vector3.zero, sphere.radius);
							}
							break;
						}
						case CapsuleCollider capsule:
						{
							DrawWireCapsule(InCollider.bounds.center, InCollider.transform.rotation, capsule.radius, capsule.height, InCollider.transform.lossyScale);
							break;
						}
					}

				}
				//Gizmos.matrix = prevMatrix;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public static void DrawWireCapsule(in Vector3 InPosition, in Quaternion InRotation, in float InRadius, in float InHeight, in Vector3 InScale)
		{
			for (int i = 0, length = m_CapsuleVertices.Length; i < length; i++)
			{
				Vector3 vertex = m_CapsuleVertices[i];
				vertex.x *= InRadius * 2f;
				vertex.y *= InHeight * 0.5f;
				vertex.z *= InRadius * 2f;
				m_CapsuleNewVertices[i].Set(vertex);
			}
			BuiltInCapsuleMesh.vertices = m_CapsuleNewVertices;
			Gizmos.DrawMesh(BuiltInCapsuleMesh, -1, InPosition, InRotation, InScale);

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

		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		public class UseGizmoColor : System.IDisposable
		{
			private bool m_Disposed = false;
			private Color m_PreviousData = Color.clear;

			public UseGizmoColor(in Color InNewData)
			{
				m_Disposed = false;
				m_PreviousData = Gizmos.color;
				Gizmos.color = InNewData;
			}

			public void Dispose()
			{
				if (m_Disposed)
				{
					return;
				}
				m_Disposed = true;

				Gizmos.color = m_PreviousData;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////
		public class UseGizmoMatrix : System.IDisposable
		{
			private bool m_Disposed = false;
			private Matrix4x4 m_PreviousData = Matrix4x4.identity;

			public UseGizmoMatrix(in Matrix4x4 InNewData)
			{
				m_Disposed = false;
				m_PreviousData = Gizmos.matrix;
				Gizmos.matrix = InNewData;
			}

			public void Dispose()
			{
				if (m_Disposed)
				{
					return;
				}
				m_Disposed = true;

				Gizmos.matrix = m_PreviousData;
			}
		}
	}

	public class MarkAsDirty : System.IDisposable
	{
		private readonly Object m_UnityObject = null;
		private bool m_Disposed = false;

		public MarkAsDirty(Object InUnityObject)
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