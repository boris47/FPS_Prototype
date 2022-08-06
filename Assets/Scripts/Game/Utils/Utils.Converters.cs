using System.Globalization;
using UnityEngine;


namespace Utils
{
	public static  class Converters
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
		/// <summary> Return a stringified version of a vector2 </summary>
		public static string Vector2ToString(Vector2 InVector2) => $"{InVector2.x}, {InVector2.y}";


		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a stringified version of a vector3 </summary>
		public static string Vector3ToString(Vector3 InVector3) => $"{InVector3.x}, {InVector3.y}, {InVector3.z}";

		//////////////////////////////////////////////////////////////////////////
		/// <summary> Return a stringified version of a vector4 </summary>
		public static string Vector4ToString(Vector4 InVector4) => $"{InVector4.x}, {InVector4.y}, {InVector4.z}, {InVector4.w}";


		//////////////////////////////////////////////////////////////////////////
		/// <summary>  Return a stringified version of a quaternion ( Used in Save & Load ) </summary>
		public static string QuaternionToString(Quaternion InQuaternion) => $"{InQuaternion.x}, {InQuaternion.y}, {InQuaternion.z}, {InQuaternion.w}";
	}
}
