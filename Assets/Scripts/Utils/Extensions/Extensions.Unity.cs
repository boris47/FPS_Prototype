
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class Extensions_Unity
{
	private static System.Type UnityEngineObjectType = typeof(UnityEngine.Object);
	private static System.Func<int, UnityEngine.Object> m_FindObjectFromInstanceID = null;

	static Extensions_Unity()
	{
		// Create FindObjectFromInstanceID delegate from internal UnityEngine.Object.FindObjectFromInstanceID
		System.Reflection.MethodInfo methodInfo = UnityEngineObjectType.GetMethod("FindObjectFromInstanceID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		if (methodInfo.IsNotNull())
		{
			m_FindObjectFromInstanceID = (System.Func<int, UnityEngine.Object>)System.Delegate.CreateDelegate(typeof(System.Func<int, UnityEngine.Object>), methodInfo);
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region ANIMATOR

	/////////////////////////////////////////////////////////////////////////////
	public static	bool			GetClipFromAnimator( this Animator animator, string name, ref AnimationClip result )
	{
		if (animator.runtimeAnimatorController == null || animator.runtimeAnimatorController.animationClips.Length == 0)
		{
			return false;
		}

		AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
		int arraySize = animationClips.Length;
		bool bIsClipFound = false;
		for (int i = 0; i < arraySize && bIsClipFound == false; i++)
		{
			AnimationClip clip = animationClips[i];

			if (clip.name == name)
			{
				bIsClipFound = true;
				result = clip;
			}
		}

		return bIsClipFound;
	}

	#endregion // ANIMATOR


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region TRANSFORM

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return true if a path to searched child is found, otherwise return false </summary>
	public static bool				TryGetChildPath(this Transform transform, out string pathToChild, System.Predicate<Transform> predicate)
	{
		CustomAssertions.IsNotNull(predicate);
		pathToChild = default;
		bool bFound = false;

		List<string> pathAsList = new List<string>();
		bool SearchForrequestedChild(Transform currentTransform)
		{
			foreach (Transform child in currentTransform)
			{
				int index = pathAsList.Count;
				pathAsList.Add(child.name);

				// Is this the child we are searching for?
				if (bFound = predicate(child))
				{
					break;  // has been found
				}
				else
				{
					if (SearchForrequestedChild(child))
					{
						break; // has been found
					}
					else
					{
						pathAsList.RemoveAt(index);
					}
				}
			}

			return bFound;
		}

		SearchForrequestedChild(transform);
		if (pathAsList.Count > 0)
		{
			pathToChild = string.Join("/", pathAsList);
		}
		return pathAsList.Count > 0;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return true if component is found, otherwise return false </summary>
	public static	void			SetLocalPositionAndRotation( this Transform transform, in Vector3 localPosition, in Quaternion localRotation )
	{
		transform.localPosition = localPosition;
		transform.localRotation = localRotation;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return true if component is found, otherwise return false </summary>
	public static	bool			HasComponent<T>( this Transform transform ) where T : Component
	{
		return transform.TryGetComponent( out T comp );
	}

	/// <summary>
	/// The SetGlobalScale method is used to set a transform scale based on a global scale instead of a local scale.
	/// </summary>
	/// <param name="worldScale">A Vector3 of a world scale to apply to the given transform.</param>
	public static	void			SetWorldScale(this Transform transform, Vector3 worldScale)
	{
		transform.localScale = new Vector3(worldScale.x / transform.lossyScale.x, worldScale.y / transform.lossyScale.y, worldScale.z / transform.lossyScale.z);
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Can be used to retrieve a component with more detailed research </summary>
	public static	bool			TrySearchComponent<T>( this Transform transform, ESearchContext Context, out T Component, System.Predicate<T> Filter = null ) where T : Component
	{
		return Utils.Base.TrySearchComponent( transform.gameObject, Context, out Component, Filter );
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Can be used to retrieve a component's array with more detailed research details </summary>
	public	static	bool			TrySearchComponents<T>( this Transform transform, ESearchContext Context, out T[] Components, System.Predicate<T> Filter = null ) where T : Component
	{
		return Utils.Base.TrySearchComponents( transform.gameObject, Context, out Components, Filter );
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Search for a specific component at specific child, if found, return operation result </summary>
	public	static	bool			TrySearchComponentByChildName<T>( this Transform transform, string childName, out T component, System.Predicate<T> filter = null) where T : Component
	{
		component = default(T);

		filter = filter.IsNotNull() ? filter : delegate (T c) { return true; };

		foreach (Transform child in transform)
		{
			if (child.name == childName && child.TryGetComponent<T>(out T componentFound))
			{
				if (filter(componentFound))
				{
					component = componentFound;
					return true;
				}
			}
		}
		return false;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Search for a specific component at specific child, if found, return operation result </summary>
	public	static	bool			TrySearchComponentByChildIndex<T>(this Transform t, int childIndex, out T component, System.Predicate<T> filter = null) where T : Component
	{
		component = default(T);
		if (childIndex >= 0 && childIndex < t.childCount)
		{
			return t.GetChild(childIndex).TrySearchComponent(ESearchContext.LOCAL, out component, filter);
		}
		return false;
	} 


	/////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Create and fills up given array with components found paired in childrens to the given enum type
	/// Requirements: children must have same name (case sensitive) of enum members
	/// </summary>
	public	static	bool			MapComponentsInChildrenToArray<T0, T1>( this Transform t, out T0[] array ) where T0 : Component where T1 : System.Enum
	{
		array = null;
		if (typeof(T1).IsEnum)
		{
			string[] names = System.Enum.GetNames( typeof(T1) );
			int namesCount = names.Length;
			array = new T0[namesCount];
			for (int i = 0; i < namesCount; i++)
			{
				string name = names[i];
				array[i] = null;
				t.TrySearchComponentByChildName( name, out array[i] );
			}
			return true;
		}
		return false;
	}
		

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Search for the given component only in children of given transform </summary>
	public	static	T[]				GetComponentsOnlyInChildren<T>(this Transform transform, bool deepSearch = false, bool includeInactive = false, System.Predicate<T> filter = null) where T : Component
	{
		T[] results = default;

		filter = filter.IsNotNull() ? filter : delegate (T c) { return true; };

		if (deepSearch)
		{
			results = transform.GetComponentsInChildren<T>(includeInactive: includeInactive).Where(c => filter(c)).ToArray();
		}
		else
		{
			List<T> compsList = new List<T>();
			foreach(Transform child in transform)
			{
				if (child.TrySearchComponents(ESearchContext.LOCAL, out T[] comps, filter))
				{
					compsList.AddRange(comps);
				}
			}
			results = compsList.ToArray();
		}

		return results;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Look for given component, if not found add it, return component reference  </summary>
	public	static	T				GetOrAddIfNotFound<T>(this Transform t) where T : Component
	{
		if (!t.TryGetComponent<T>(out T result))
		{
			result = t.gameObject.AddComponent<T>();
		}
		return result;
	}

	#endregion // TRANSFORM


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region OBJECT

	/// <summary> Return true if frameCount frames is repeating, otherwise false </summary>
	/// <param name="frameCount"></param>
	public static bool					EveryFrames(this Object t, int frameCount)
	{
		return Time.frameCount % frameCount != 0;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return the object with instance id requested or null </summary>
	public static UnityEngine.Object	FindByInstanceID(this UnityEngine.Object obj, int InstanceId)
	{
		return m_FindObjectFromInstanceID?.Invoke(InstanceId);
	}

	#endregion // OBJECT


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region GAMEOBJECT


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Look for given component, if not found add it, return component reference </summary>
	public static	T				GetOrAddIfNotFound<T>( this UnityEngine.GameObject go ) where T : UnityEngine.Component
	{
		return GetOrAddIfNotFound(go, typeof(T)) as T;
	}

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Look for given component, if not found add it, return component reference </summary>
	public static Component			GetOrAddIfNotFound(this UnityEngine.GameObject go, System.Type type)
	{
		Component result = null;
		if (type.IsSubclassOf(typeof(UnityEngine.Component)) && !go.TryGetComponent(type, out result))
		{
			result = go.AddComponent(type);
		}
		return result;
	}
	

	#endregion // GAMEOBJECT


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region VECTOR2

		/////////////////////////////////////////////////////////////////////////////
	public static	Vector2			ClampComponents( this ref Vector2 v, in float min, in float max )
	{
		v.x = Mathf.Clamp( v.x, min, max );
		v.y = Mathf.Clamp( v.y, min, max );
		return v;
	}


	/////////////////////////////////////////////////////////////////////////////
	public	static	Vector2			ClampComponents( this ref Vector2 v, in Vector2 clamping )
	{
		v.x = Mathf.Clamp( v.x, -clamping.x, clamping.x );
		v.y = Mathf.Clamp( v.y, -clamping.y, clamping.x );
		return v;
	}


	/////////////////////////////////////////////////////////////////////////////
	public	static	Vector3			Add( this ref Vector2 v, in Vector2 v2 )
	{
		v.x += v2.x; v.y += v2.y; return v;
	}


	/////////////////////////////////////////////////////////////////////////////
	public	static	Vector3			Sub( this ref Vector2 v, in Vector2 v2 )
	{
		v.x -= v2.x; v.y -= v2.y; return v;
	}


	/////////////////////////////////////////////////////////////////////////////
	public	static	void			LerpTo( this ref Vector2 v, in Vector2 dest, in float interpolant )
	{
		v.x = Mathf.Lerp( v.x, dest.x, interpolant );
		v.y = Mathf.Lerp( v.x, dest.y, interpolant );
	}

	/////////////////////////////////////////////////////////////////////////////
	public static	Vector2			FromString(this ref Vector2 v, in string input)
	{
		Vector2 result = Vector2.zero;
		{
			string[] values = input.Replace("(", string.Empty).Replace(")", string.Empty).Trim().TrimInside().Split(',');
			if (values.Length == 2 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y))
			{
				result.Set(x, y);
			}
		}
		return result;
	}

	#endregion // VECTOR2


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region VECTOR3

	/////////////////////////////////////////////////////////////////////////////
	public static	Vector3			ClampComponents( this ref Vector3 v, in float min, in float max )
	{
		v.x = Mathf.Clamp( v.x, min, max );
		v.y = Mathf.Clamp( v.y, min, max );
		v.z = Mathf.Clamp( v.z, min, max );
		return v;
	}


	/////////////////////////////////////////////////////////////////////////////
	public	static	Vector3			ClampComponents( this ref Vector3 v, in Vector3 clamping )
	{
		v.x = Mathf.Clamp( v.x, -clamping.x, clamping.x );
		v.y = Mathf.Clamp( v.y, -clamping.y, clamping.x );
		v.z = Mathf.Clamp( v.y, -clamping.z, clamping.z );
		return v;
	}


	/////////////////////////////////////////////////////////////////////////////
	public	static	Vector3			Add( this ref Vector3 v, in Vector3 v2 )
	{
		v.x += v2.x;
		v.y += v2.y;
		v.z += v2.z;
		return v;
	}


	/////////////////////////////////////////////////////////////////////////////
	public	static	Vector3			Sub( this ref Vector3 v, in Vector3 v2 )
	{
		v.x -= v2.x;
		v.y -= v2.y;
		v.z -= v2.z;
		return v;
	}


	/////////////////////////////////////////////////////////////////////////////
	public	static	void			LerpTo( this ref Vector3 v, in Vector3 dest, in float interpolant )
	{
		v.x = Mathf.Lerp(v.x, dest.x, interpolant);
		v.y = Mathf.Lerp(v.y, dest.y, interpolant);
		v.z = Mathf.Lerp(v.z, dest.z, interpolant);
	}
	/*
	public static float SignedAngleFromPosition(this Vector3 referencepoint, Vector3 frompoint, Vector3 topoint, Vector3 referenceup)
	{
		// calculates the the angle between two position vectors regarding to a reference point (plane), with a referenceup a sign in which direction it points can be calculated (clockwise is positive and counter clockwise is negative)
		Vector3 fromdir = frompoint - referencepoint;                       // calculate the directionvector pointing from refpoint towards frompoint
		Vector3 todir = topoint - referencepoint;                           // calculate the directionvector pointing from refpoint towards topoint
		Vector3 planenormal = Vector3.Cross(fromdir, todir);               // calculate the planenormal (perpendicular vector)
		float angle = Vector3.Angle(fromdir, todir);                       // calculate the angle between the 2 direction vectors (note: its always the smaller one smaller than 180°)
		float orientationdot = Vector3.Dot(planenormal, referenceup);   // calculate wether the normal and the referenceup point in the same direction (>0) or not (<0), http://docs.unity3d.com/Documentation/Manual/ComputingNormalPerpendicularVector.html

		return angle * Mathf.Sign(orientationdot);
	//	if (orientationdot > 0.0f)                                           // the angle is positive (clockwise orientation seen from referenceup)
	//		return angle;
	//	return -angle;   // the angle is negative (counter-clockwise orientation seen from referenceup)
	}

	public static float SignedAngleFromDirection(this Vector3 fromdir, Vector3 todir, Vector3 referenceup)
	{
		// calculates the the angle between two direction vectors, with a referenceup a sign in which direction it points can be calculated (clockwise is positive and counter clockwise is negative)
		Vector3 planenormal = Vector3.Cross(fromdir, todir);             // calculate the planenormal (perpendicular vector)
		float angle = Vector3.Angle(fromdir, todir);                     // calculate the angle between the 2 direction vectors (note: its always the smaller one smaller than 180°)
		float orientationdot = Vector3.Dot(planenormal, referenceup);    // calculate wether the normal and the referenceup point in the same direction (>0) or not (<0), http://docs.unity3d.com/Documentation/Manual/ComputingNormalPerpendicularVector.html

		return angle * Mathf.Sign(orientationdot);
//		if (orientationdot > 0.0f)                                         // the angle is positive (clockwise orientation seen from referenceup)
//			return angle;
//		return -angle;  // the angle is negative (counter-clockwise orientation seen from referenceup)
	}
	*/
	/////////////////////////////////////////////////////////////////////////////
	public static Vector3			FromString(this ref Vector3 v, in string input)
	{
		Vector3 result = Vector3.zero;
		{
			string[] values = input.Replace("(", string.Empty).Replace(")", string.Empty).Trim().TrimInside().Split(',');
			if (values.Length == 3 && float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
			{
				result.Set(x, y, z);
			}
		}
		return result;
	}

	#endregion // VECTOR3


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	#region QUATERNION

	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Returna vector which rotation is the given quaternion </summary>
	public static	Vector3			GetVector( this ref Quaternion q, Vector3 d )
		{
			// A quaternion doesn't have a direction by itself. It is a rotation.
			// It can be used to rotate any vector by the rotation it represents. Just multiply a Vector3 by the quaternion.
			// Ref: http://answers.unity.com/answers/525956/view.html
			return q * d;

	/*		// Ref: Unreal math Library
			Vector3 Q = new Vector3( q.x, q.y, q.z );
			Vector3 T = 2.0f * Vector3.Cross( Q, d );
			return d + ( T * q.w ) + Vector3.Cross( Q, T );
	*/
		}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> We need this because Quaternion.Slerp always uses the shortest arc </summary>
	public	static	Quaternion		Slerp( this ref Quaternion p, Quaternion q, float t)
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

		ret.x = (fCoeff0 * p.x) + (fCoeff1 * q.x);
		ret.y = (fCoeff0 * p.y) + (fCoeff1 * q.y);
		ret.z = (fCoeff0 * p.z) + (fCoeff1 * q.z);
		ret.w = (fCoeff0 * p.w) + (fCoeff1 * q.w);
			
		return ret;
	}


	/////////////////////////////////////////////////////////////////////////////
	/// <summary> Return th lenght of a quaternion </summary>
	public	static	float			GetLength( this ref Quaternion q )
	{
		return Mathf.Sqrt((q.x * q.x) + (q.y * q.y) + (q.z * q.z) + (q.w * q.w));
	}

	#endregion // QUATERNION
}


