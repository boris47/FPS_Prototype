
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public enum ESearchContext
{
	/// <summary> Only on This object </summary>
	LOCAL,
	/// <summary> On This object and children </summary>
	LOCAL_AND_CHILDREN,
	/// <summary> On This object and parents </summary>
	LOCAL_AND_PARENTS,
	/// <summary> On all the object hierarchy </summary>
	FROM_ROOT = LOCAL_AND_CHILDREN | LOCAL | LOCAL_AND_PARENTS
}

public static class Extensions_Unity
{
	private static System.Func<Object, bool> s_IsObjectAliveDelegate = delegate (Object _) { return true; };

	static Extensions_Unity()
	{
		// Create FindObjectFromInstanceID delegate from internal UnityEngine.Object.FindObjectFromInstanceID
		System.Reflection.MethodInfo methodInfo = typeof(Object).GetMethod("IsNativeObjectAlive", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		if (methodInfo.IsNotNull())
		{
			s_IsObjectAliveDelegate = (System.Func<Object, bool>)System.Delegate.CreateDelegate(typeof(System.Func<Object, bool>), methodInfo);
		}
	}


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region ANIMATOR

	/////////////////////////////////////////////////////////////////////////////
	public static bool GetClipFromAnimator(this Animator ThisAnimator, string InClipName, out AnimationClip OutResult)
	{
		OutResult = default;
		if (ThisAnimator.runtimeAnimatorController.IsNotNull() && ThisAnimator.runtimeAnimatorController.animationClips.Length > 0)
		{
			OutResult = ThisAnimator.runtimeAnimatorController.animationClips.FirstOrDefault(clip => clip.name == InClipName);
		}
		return OutResult.IsNotNull();
	}

	#endregion // ANIMATOR


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region TRANSFORM

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return unity hierarchy path of This Transform </summary>
	public static string GetFullPath(this Transform ThisTransform)
	{
		string OutResult = ThisTransform.name;
		if (ThisTransform.parent.IsNotNull())
		{
			OutResult = $"{ThisTransform.parent.GetFullPath()}/{ThisTransform.name}";
		}
		return OutResult;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return true if a path to searched child is found, otherwise return false </summary>
	public static bool TryGetChildPath(this Transform ThisTransform, out string OutPathToChild, System.Predicate<Transform> InPredicate)
	{
		static bool SearchForRequestedChild(Transform currentTransform, ref List<string> pathAsList, System.Predicate<Transform> predicate)
		{
			foreach (Transform child in currentTransform)
			{
				int index = pathAsList.Count;
				pathAsList.Add(child.name);

				// Is This the child we are searching for?
				if (predicate(child))
				{
					return true;
				}
				else
				{
					if (SearchForRequestedChild(child, ref pathAsList, predicate))
					{
						return true;
					}
					else
					{
						pathAsList.RemoveAt(index);
					}
				}
			}

			return false;
		}


		List<string> pathAsList = new List<string>();
		if (Utils.CustomAssertions.IsNotNull(InPredicate) && SearchForRequestedChild(ThisTransform, ref pathAsList, InPredicate))
		{
			OutPathToChild = string.Join("/", pathAsList);
		}
		else
		{
			OutPathToChild = null;
		}
		return OutPathToChild.IsNotNull();
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return true if component is found, otherwise return false </summary>
	public static void SetLocalPositionAndRotation(this Transform ThisTransform, in Vector3 InLocalPosition, in Quaternion InLocalRotation)
	{
		ThisTransform.localPosition = InLocalPosition;
		ThisTransform.localRotation = InLocalRotation;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return true if component is found, otherwise return false </summary>
	public static bool HasComponent<T>(this Transform ThisTransform) where T : Component => ThisTransform.gameObject.TryGetComponent(out T _);
	

	/// <summary>
	/// The SetGlobalScale method is used to set a transform scale based on a global scale instead of a local scale.
	/// </summary>
	/// <param name="InWorldScale">A Vector3 of a world scale to apply to the given transform.</param>
	public static void SetWorldScale(this Transform ThisTransform, in Vector3 InWorldScale)
	{
		ThisTransform.localScale = new Vector3(InWorldScale.x / ThisTransform.lossyScale.x, InWorldScale.y / ThisTransform.lossyScale.y, InWorldScale.z / ThisTransform.lossyScale.z);
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Can be used to retrieve a component with more detailed research </summary>
	public static bool TrySearchComponent<T>(this Transform ThisTransform, ESearchContext InContext, out T OutComponent, System.Predicate<T> InFilter = null) where T : Component
	{
		OutComponent = default;
		if (TrySearchComponentsInternal(ThisTransform.gameObject, InContext, out T[] results))
		{
			// Filtered search
			if (InFilter.IsNotNull())
			{
				OutComponent = System.Array.Find(results, InFilter);
			}
			// Normal search
			else
			{
				OutComponent = results[0];
			}
		}
		return OutComponent.IsNotNull();
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Can be used to retrieve a component's array with more detailed research details </summary>
	public static bool TrySearchComponents<T>(this Transform ThisTransform, ESearchContext InContext, out T[] OutComponents, System.Predicate<T> InFilter = null) where T : Component
	{
		if (TrySearchComponentsInternal(ThisTransform.gameObject, InContext, out OutComponents))
		{
			// Filtered search
			if (InFilter.IsNotNull())
			{
				OutComponents = global::System.Array.FindAll(OutComponents, InFilter);
			}
		}
		return OutComponents.IsNotNull() && OutComponents.Length > 0;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Search for a specific component at specific child, if found, return operation result </summary>
	public static bool TrySearchComponentByChildName<T>(this Transform ThisTransform, string InChildName, out T OutComponent, System.Predicate<T> InFilter = null) where T : Component
	{
		OutComponent = default(T);

		InFilter = InFilter.IsNotNull() ? InFilter : delegate (T c) { return true; };

		foreach (Transform child in ThisTransform)
		{
			if (child.name == InChildName && child.TryGetComponent(out T componentFound))
			{
				if (InFilter(componentFound))
				{
					OutComponent = componentFound;
					return true;
				}
			}
		}
		return false;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Search for a specific component at specific child, if found, return operation result </summary>
	public static bool TrySearchComponentByChildIndex<T>(this Transform ThisTransform, int InChildIndex, out T OutComponent, System.Predicate<T> InFilter = null) where T : Component
	{
		OutComponent = default(T);
		if (InChildIndex >= 0 && InChildIndex < ThisTransform.childCount)
		{
			return ThisTransform.GetChild(InChildIndex).TrySearchComponent(ESearchContext.LOCAL, out OutComponent, InFilter);
		}
		return false;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Search for the given component only in children of given transform </summary>
	public static T[] GetComponentsOnlyInChildren<T>(this Transform ThisTransform, bool bDeepSearch = false, System.Predicate<T> InFilter = null) where T : Component
	{
		List<T> compsList = new List<T>();
		foreach (Transform child in ThisTransform)
		{
			if (child.TrySearchComponents(bDeepSearch ? ESearchContext.LOCAL_AND_CHILDREN : ESearchContext.LOCAL, out T[] OutComponents, InFilter))
			{
				compsList.AddRange(OutComponents);
			}
		}
		return compsList.ToArray();
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Look for given component, if not found add it, return component reference  </summary>
	public static T GetOrAddIfNotFound<T>(this Transform ThisTransform) where T : Component
	{
		if (!ThisTransform.TryGetComponent(out T result))
		{
			result = ThisTransform.gameObject.AddComponent<T>();
		}
		return result;
	}

	#endregion // TRANSFORM


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region OBJECT


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Editor Only </summary>
	public static bool TryGetSubObjectsOfType<T>(this Object ThisObject, out T[] OutResult) where T : Object
	{
		return TryGetSubObjectsOfType(ThisObject, typeof(T), out OutResult);
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Editor Only </summary>
	public static bool TryGetSubObjectsOfType<T>(this Object ThisObject, System.Type InBaseType, out T[] OutResult) where T : Object
	{
		OutResult = default;
#if UNITY_EDITOR
		string assetPath = UnityEditor.AssetDatabase.GetAssetPath(ThisObject);
		if (!string.IsNullOrEmpty(assetPath))
		{
			// Ref: https://docs.unity3d.com/ScriptReference/AssetDatabase.LoadAllAssetRepresentationsAtPath.html
			// This function only returns sub Assets that are visible in the Project view.
			// All paths are relative to the project folder, for example: "Assets/MyTextures/hello.png"
			Object[] subAssets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
			OutResult = subAssets.Where(o => ReflectionHelper.IsInerithedFrom(InBaseType, o.GetType())).Cast<T>().ToArray();
		}
#endif
		return OutResult.IsNotNull() && OutResult.Length > 0;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	public static bool IsNotNull(this Object ThisObject) => ((Object)ThisObject) != null && (ThisObject as object).IsNotNull() && s_IsObjectAliveDelegate(ThisObject);


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return true if frameCount frames is repeating, otherwise false </summary>
	public static bool EveryFrames(this Object _, int InFrameCount) => Time.frameCount % InFrameCount != 0;


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Destroys a UnityObject safely. </summary>
	/// <param name="thisObject">Object to be destroyed.</param>
	public static void Destroy(this Object ThisObject)
	{
		if (ThisObject.IsNotNull())
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				Object.Destroy(ThisObject);
			}
			else
			{
				Object.DestroyImmediate(ThisObject);
			}
#else
			Object.Destroy(ThisObject);
#endif
		}
	}


	#endregion // OBJECT


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region RIGIDBODY
	public static void RotateAround(this Rigidbody ThisRigidbody, Vector3 InPoint, Vector3 InAxis, float InAngle)
	{
		Quaternion q = Quaternion.AngleAxis(InAngle, InAxis);
		ThisRigidbody.MovePosition((q * (ThisRigidbody.transform.position - InPoint)) + InPoint);
		ThisRigidbody.MoveRotation(ThisRigidbody.transform.rotation * q);
	}
	public static void Rotate(this Rigidbody ThisRigidbody, Vector3 InAxis, float InAngle)
	{
		Quaternion q = Quaternion.AngleAxis(InAngle, InAxis);
		ThisRigidbody.MoveRotation(ThisRigidbody.transform.rotation * q);
	}
	#endregion


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region COLLIDER

	public static bool Contains(this Collider ThisCollider, Vector3 InPoint)
	{
		return (ThisCollider.ClosestPoint(InPoint) - InPoint).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
	}
	
	#endregion


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region GAMEOBJECT

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return unity hierarchy path of This GameObject </summary>
	public static string GetFullPath(this GameObject ThisGameObject)
	{
		string OutResult = null;
		if (ThisGameObject.transform.parent.IsNotNull())
		{
			OutResult = $"{ThisGameObject.transform.parent.gameObject.GetFullPath()}/{ThisGameObject.name}";
		}
		else
		{
			OutResult = ThisGameObject.name;
		}
		return OutResult;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Search for component of type T on This gameObject if not a value is already assigned </summary>
	public static T GetIfNotAssigned<T>(this GameObject ThisGameObject, ref T OutValue) where T : Component
	{
		if (!OutValue.IsNotNull())
		{
			if (ThisGameObject.TryGetComponent(out T OutValue2)) // Faster
			{
				OutValue = OutValue2;
			}
		}
		return OutValue;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Search for component of type T on This gameObject if not a value is already assigned </summary>
	public static bool TryGetIfNotAssigned<T>(this GameObject ThisGameObject, ref T OutValue) where T : Component
	{
		if (!OutValue.IsNotNull())
		{
			if (ThisGameObject.TryGetComponent(out T OutValue2)) // Faster
			{
				OutValue = OutValue2;
			}
		}
		return OutValue.IsNotNull();
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Look for given component, if not found add it, return component reference </summary>
	public static T GetOrAddIfNotFound<T>(this GameObject ThisGameObject) where T : Component => GetOrAddIfNotFound(ThisGameObject, typeof(T)) as T;


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Look for given component, if not found add it, return component reference </summary>
	public static Component GetOrAddIfNotFound(this GameObject ThisGameObject, System.Type InType)
	{
		Component result = null;
		if (InType.IsSubclassOf(typeof(Component)) && !ThisGameObject.TryGetComponent(InType, out result))
		{
			result = ThisGameObject.AddComponent(InType);
		}
		return result;
	}

	/// <summary> Create a child with desired state (enabled or disabled) with defined name and attached script in desired state (enabled or disabled) </summary>
	public static T AddChildWithComponent<T>(this GameObject ThisGameObject, in string InChildName, in bool bChildEnableState = true, in bool bComponentEnableState = true) where T : Component, new()
	{
		return AddChildWithComponent(ThisGameObject, InChildName, typeof(T), bChildEnableState, bComponentEnableState) as T;
	}

	/// <summary> Create a child with desired state (enabled or disabled) with defined name and attached script in desired state (enabled or disabled) </summary>
	public static Behaviour AddChildWithComponent(this GameObject ThisGameObject, in string InChildName, in System.Type InType, in bool bChildEnableState = true, in bool bComponentEnableState = true)
	{
		GameObject child = new GameObject(InChildName);
		child.SetActive(false);
		child.transform.SetParent(ThisGameObject.transform);

		Behaviour behaviour = child.AddComponent(InType) as Behaviour;
		behaviour.enabled = bComponentEnableState;

		child.SetActive(bChildEnableState);

		return behaviour;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Can be used to retrieve a component with more detailed research </summary>
	public static bool TrySearchComponent<T>(this GameObject ThisGameObject, ESearchContext InContext, out T OutComponent, System.Predicate<T> InFilter = null) where T : Component
	{
		OutComponent = default;
		if (TrySearchComponentsInternal(ThisGameObject, InContext, out T[] results))
		{
			// Filtered search
			if (InFilter.IsNotNull())
			{
				OutComponent = System.Array.Find(results, InFilter);
			}
			// Normal search
			else
			{
				OutComponent = results[0];
			}
		}
		return OutComponent.IsNotNull();
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Can be used to retrieve a component's array with more detailed research details </summary>
	public static bool TrySearchComponents<T>(this GameObject ThisGameObject, ESearchContext InContext, out T[] OutComponents, System.Predicate<T> InFilter = null) where T : Component
	{
		if (TrySearchComponentsInternal(ThisGameObject, InContext, out OutComponents))
		{
			// Filtered search
			if (InFilter.IsNotNull())
			{
				OutComponents = global::System.Array.FindAll(OutComponents, InFilter);
			}
		}
		return OutComponents.IsNotNull() && OutComponents.Length > 0;
	}


	#endregion // GAMEOBJECT


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region VECTOR2

	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Deconstruct(this Vector2 ThisVector, out float OutX, out float OutY)
	{
		OutX = ThisVector.x;
		OutY = ThisVector.y;
	}

	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 ClampComponents(this ref Vector2 ThisVector, in float InMinimum, in float InMaximum)
	{
		ThisVector.x = Mathf.Clamp(ThisVector.x, InMinimum, InMaximum);
		ThisVector.y = Mathf.Clamp(ThisVector.y, InMinimum, InMaximum);
		return ThisVector;
	}


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 ClampComponents(this ref Vector2 ThisVector, in Vector2 InClamping)
	{
		ThisVector.x = Mathf.Clamp(ThisVector.x, -InClamping.x, InClamping.x);
		ThisVector.y = Mathf.Clamp(ThisVector.y, -InClamping.y, InClamping.x);
		return ThisVector;
	}


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Set(this ref Vector2 ThisVector, in Vector2 InOtherVector)
	{
		ThisVector.x = InOtherVector.x;
		ThisVector.y = InOtherVector.y;
		return ThisVector;
	}


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Add(this ref Vector2 ThisVector, in Vector2 InOtherVector)
	{
		ThisVector.x += InOtherVector.x;
		ThisVector.y += InOtherVector.y;
		return ThisVector;
	}


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Sub(this ref Vector2 ThisVector, in Vector2 InOtherVector)
	{
		ThisVector.x -= InOtherVector.x;
		ThisVector.y -= InOtherVector.y;
		return ThisVector;
	}


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LerpTo(this ref Vector2 ThisVector, in Vector2 InDestinationVector, in float InInterpolant)
	{
		ThisVector.x = Mathf.Lerp(ThisVector.x, InDestinationVector.x, InInterpolant);
		ThisVector.y = Mathf.Lerp(ThisVector.x, InDestinationVector.y, InInterpolant);
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Create a vector parsing a string with format [(,] X,Y[,)] </summary>
	public static bool TryFromString(this ref Vector2 ThisVector, in string InString)
	{
		string[] values = InString.Replace("(", string.Empty).Replace(")", string.Empty).Trim().TrimInside().Split(',');
		if (values.Length == 2 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y))
		{
			ThisVector.Set(x, y);
			return true;
		}
		return false;
	}

	#endregion // VECTOR2

	
	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region VECTOR3


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Deconstruct(this Vector3 ThisVector, out float OutX, out float OutY, out float OutZ)
	{
		OutX = ThisVector.x;
		OutY = ThisVector.y;
		OutZ = ThisVector.z;
	}

	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ClampComponents(this ref Vector3 ThisVector, in float InMinimum, in float InMaximum)
	{
		ThisVector.x = Mathf.Clamp(ThisVector.x, InMinimum, InMaximum);
		ThisVector.y = Mathf.Clamp(ThisVector.y, InMinimum, InMaximum);
		ThisVector.z = Mathf.Clamp(ThisVector.z, InMinimum, InMaximum);
		return ThisVector;
	}


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 ClampComponents(this ref Vector3 ThisVector, in Vector3 InClamping)
	{
		ThisVector.x = Mathf.Clamp(ThisVector.x, -InClamping.x, InClamping.x);
		ThisVector.y = Mathf.Clamp(ThisVector.y, -InClamping.y, InClamping.x);
		ThisVector.z = Mathf.Clamp(ThisVector.y, -InClamping.z, InClamping.z);
		return ThisVector;
	}


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Set(this ref Vector3 ThisVector, in Vector3 InOtherVector)
	{
		ThisVector.x = InOtherVector.x;
		ThisVector.y = InOtherVector.y;
		ThisVector.z = InOtherVector.z;
		return ThisVector;
	}


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Add(this ref Vector3 ThisVector, in Vector3 InOtherVector)
	{
		ThisVector.x += InOtherVector.x;
		ThisVector.y += InOtherVector.y;
		ThisVector.z += InOtherVector.z;
		return ThisVector;
	}

	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(this Vector3 ThisVector, in Vector3 InOtherVector) => Vector3.Distance(ThisVector, InOtherVector);


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceXZ(this Vector3 ThisVector, in Vector3 InOtherVector) => Vector2.Distance(new Vector2(ThisVector.x, ThisVector.z), new Vector2(InOtherVector.x, InOtherVector.z));


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSqr(this Vector3 ThisVector, in Vector3 InOtherVector) => Vector3.SqrMagnitude(ThisVector - InOtherVector);
	

	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceXZSqr(this Vector3 ThisVector, in Vector3 InOtherVector) => Vector2.SqrMagnitude(new Vector2(ThisVector.x, ThisVector.z) - new Vector2(InOtherVector.x, InOtherVector.z));

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Gets the magnitude on an axis given a <see cref="Vector3"/>. </summary>
	/// <param name="ThisVector">The vector.</param>
	/// <param name="InAxis">The axis on which to calculate the magnitude.</param>
	/// <returns>The magnitude.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float GetMagnitudeOnAxis(this Vector3 ThisVector, in Vector3 InAxis)
	{
		var vectorMagnitude = ThisVector.magnitude;
		if (vectorMagnitude <= 0)
		{
			return 0;
		}
		var dot = Vector3.Dot(InAxis, ThisVector / vectorMagnitude);
		var val = dot * vectorMagnitude;
		return val;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Get the square magnitude from vectorA to vectorB. </summary>
	/// <returns>The sqr magnitude.</returns>
	/// <param name="ThisVector">First vector.</param>
	/// <param name="InOtherVector">Second vector.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SqrMagnitudeFrom(this Vector3 ThisVector, Vector3 InOtherVector)
	{
		var diff = ThisVector - InOtherVector;
		return diff.sqrMagnitude;
	}

	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 Sub(this ref Vector3 ThisVector, in Vector3 InOtherVector)
	{
		ThisVector.x -= InOtherVector.x;
		ThisVector.y -= InOtherVector.y;
		ThisVector.z -= InOtherVector.z;
		return ThisVector;
	}


	/////////////////////////////////////////////////////////////////////////////
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LerpTo(this ref Vector3 ThisVector, in Vector3 InDestinationVector, in float InInterpolant)
	{
		ThisVector.x = Mathf.Lerp(ThisVector.x, InDestinationVector.x, InInterpolant);
		ThisVector.y = Mathf.Lerp(ThisVector.y, InDestinationVector.y, InInterpolant);
		ThisVector.z = Mathf.Lerp(ThisVector.z, InDestinationVector.z, InInterpolant);
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Create a vector parsing a string with format [(,] X,Y,Z[,)] </summary>
	public static bool TryFromString(this ref Vector3 vector, in string InString)
	{
		string[] values = InString.Replace("(", string.Empty).Replace(")", string.Empty).Trim().TrimInside().Split(',');
		if (values.Length == 3 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
		{
			vector.Set(x, y, z);
			return true;
		}
		return false;
	}

	#endregion // VECTOR3

	
	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region QUATERNION

	/////////////////////////////////////////////////////////////////////////////
	public static void Deconstruct(this Quaternion ThisQuaternion, out float OutX, out float OutY, out float OutZ, out float OutW)
	{
		OutX = ThisQuaternion.x;
		OutY = ThisQuaternion.y;
		OutZ = ThisQuaternion.z;
		OutW = ThisQuaternion.w;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return a vector which rotation is the given quaternion </summary>
	public static Vector3 GetVector(this Quaternion ThisQuaternion, Vector3 InVector)
	{
		// A quaternion doesn't have a direction by itself. It is a rotation.
		// It can be used to rotate any vector by the rotation it represents. Just multiply a Vector3 by the quaternion.
		// Ref: http://answers.unity.com/answers/525956/view.html
		return ThisQuaternion * InVector;

		/*
		// Ref: Unreal math Library
		Vector3 Q = new Vector3( q.x, q.y, q.z );
		Vector3 T = 2.0f * Vector3.Cross( Q, d );
		return d + ( T * q.w ) + Vector3.Cross( Q, T );
		*/
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> We need This because Quaternion.Slerp always uses the shortest arc </summary>
	public static Quaternion Slerp(this Quaternion ThisQuaternion, Quaternion InOtherQuaternion, float InInterpolant)
	{
		float fCos = Quaternion.Dot(ThisQuaternion, InOtherQuaternion);
		fCos = (fCos >= 0.0f) ? fCos : -fCos;

		float fCoeff0 = 0f, fCoeff1 = 0f;

		if (fCos < 0.9999f)
		{
			float omega = Mathf.Acos(fCos);
			float invSin = 1.0f / Mathf.Sin(omega);
			fCoeff0 = Mathf.Sin((1.0f - InInterpolant) * omega) * invSin;
			fCoeff1 = Mathf.Sin(InInterpolant * omega) * invSin;
		}
		else
		{
			// Use linear interpolation
			fCoeff0 = 1.0f - InInterpolant;
			fCoeff1 = InInterpolant;
		}

		fCoeff1 = (fCos >= 0.0f) ? fCoeff1 : -fCoeff1;

		return new Quaternion(
			x: (fCoeff0 * ThisQuaternion.x) + (fCoeff1 * InOtherQuaternion.x),
			y: (fCoeff0 * ThisQuaternion.y) + (fCoeff1 * InOtherQuaternion.y),
			z: (fCoeff0 * ThisQuaternion.z) + (fCoeff1 * InOtherQuaternion.z),
			w: (fCoeff0 * ThisQuaternion.w) + (fCoeff1 * InOtherQuaternion.w)
		);
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return th length of a quaternion </summary>
	public static float GetLength(this Quaternion ThisQuaternion) => Mathf.Sqrt((ThisQuaternion.x * ThisQuaternion.x) + (ThisQuaternion.y * ThisQuaternion.y) + (ThisQuaternion.z * ThisQuaternion.z) + (ThisQuaternion.w * ThisQuaternion.w));
	

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Create a quaternion parsing a string with format [(,] X,Y,Z,W[,)] </summary>
	public static bool TryFromString(this ref Quaternion ThisQuaternion, in string InString)
	{
		string[] values = InString.Replace("(", string.Empty).Replace(")", string.Empty).Trim().TrimInside().Split(',');
		if (values.Length == 4 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z) && float.TryParse(values[3], out float w))
		{
			ThisQuaternion.Set(x, y, z, w);
			return true;
		}
		return false;
	}

	#endregion // QUATERNION

	
	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region CollisionFlags

	public static bool IsOrContains(this CollisionFlags flags, CollisionFlags other)
	{
		return (flags == other) || (flags & other) != 0;
	}

	public static bool Is(this CollisionFlags flags, CollisionFlags other)
	{
		return (flags == other);
	}

	#endregion

	
	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region UnityEditor.SerializedObject

#if UNITY_EDITOR
	public static void DoLayoutWithoutScriptProperty(this UnityEditor.SerializedObject serializedObject)
	{
		using (new UnityEditor.LocalizationGroup(serializedObject))
		{
			UnityEditor.EditorGUI.BeginChangeCheck();
			{
				serializedObject.UpdateIfRequiredOrScript();

				UnityEditor.SerializedProperty iterator = serializedObject.GetIterator();
				bool enterChildren = true;
				while (iterator.NextVisible(enterChildren))
				{
					if (iterator.propertyPath != "m_Script")
					{
						UnityEditor.EditorGUILayout.PropertyField(iterator, true);
					}
					enterChildren = false;
				}
			}
			if (UnityEditor.EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
#endif

	#endregion // UnityEditor.SerializedObject


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region PRIVATE METHODS

	private static bool TrySearchComponentsInternal<T>(GameObject InGameObject, ESearchContext InContext, out T[] OutResults) where T : Component
	{
		OutResults = null;
		switch (InContext)
		{
			case ESearchContext.LOCAL: { OutResults = InGameObject.GetComponents<T>(); break; }
			case ESearchContext.LOCAL_AND_CHILDREN: { OutResults = InGameObject.GetComponentsInChildren<T>(includeInactive: true); break; }
			case ESearchContext.LOCAL_AND_PARENTS: { OutResults = InGameObject.GetComponentsInParent<T>(includeInactive: true); break; }
			case ESearchContext.FROM_ROOT: { OutResults = InGameObject.transform.root.GetComponentsInChildren<T>(includeInactive: true); break; }
		}
		return OutResults.IsNotNull() && OutResults.Length > 0;
	}

	#endregion
}


