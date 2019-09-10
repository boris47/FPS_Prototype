﻿
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

public static class Extensions {

	/////////////////////////////////////////////////////////////////////////////
	///////////////	C# ///////////////


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		OBJECT
	public	static	bool	IsNotNull( this System.Object obj )
	{
		bool bIsNotNull = ( obj ) != null;
		return bIsNotNull;
	}


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		STRING

	/// <summary> This method also trim inside the string </summary>
	public	static	string			TrimInside( this string str, params char[] trimChars )
	{
		List<char> charsToSearch = new List<char>(1);
		if ( trimChars != null && trimChars.Length > 0 )
		{
			charsToSearch.AddRange( trimChars );
		}
		else
		{
			charsToSearch.Add( ' ' );
		}

		for ( int i = str.Length - 1; i >= 0; i-- )
		{
			if ( charsToSearch.IndexOf( str[ i ] ) != -1 )
			{
				str = str.Remove( i, 1 );
			}
		}
		return str;
	}
	
	
	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		ARRAY

	/// <summary> Allow to easly get a value from an array checking given index, default value is supported </summary>
	public	static	T				GetByIndex<T>( this global::System.Array a, int idx, T Default = default(T) )
	{
		return ( idx > -1 && idx < a.Length ) ? (T)a.GetValue(idx) : Default;
	}


	/// <summary> Search along a bidimensional array for item using predicate </summary>
	public	static	bool			FindByPredicate<T>( this T[,] array, ref T value, ref Vector2 location, System.Predicate<T> predicate )
	{
		// Gets the rank (number of dimensions) of the Array
		int dimensions = array.Rank;
		bool bIsFound = false;
		location.Set( 0, 0 );
		for ( int dimension = 0; dimension < dimensions && bIsFound == false; dimension++ )
		{
			int upper = array.GetUpperBound(dimension);
			int lower = array.GetLowerBound(dimension);

			for ( int index = lower; index <= upper && bIsFound == false; index++ )
			{
				T currentValue = array[ dimension, index ];
				if ( predicate( currentValue ) )
				{
					value = currentValue;
					location.Set( dimension, index );
					bIsFound = true;
				}
			}
		}

		return bIsFound;
	}


	public	static	bool		FindByPredicate<T>( this global::System.Array array, System.Predicate<T> predicate, ref T value, ref int[] location, int locationLevel )
	{
		bool bIsFound = false;

		int dimensions = array.Rank;

		// Mono-dimensional array
		if ( dimensions == 1 )
		{
			int upper = array.GetUpperBound(1);
			int lower = array.GetLowerBound(1);

			for ( int index = lower; index <= upper && bIsFound == false; index++ )
			{
				T currentValue = (T)array.GetValue( index );
				if ( predicate( currentValue ) )
				{
					value = currentValue;
					location = new int[2] { dimensions, index };
					bIsFound = true;
				}
			}
			return bIsFound;
		}

		// Multi dimensional array
		for ( int dimension = 1; dimension < dimensions && bIsFound == false; dimension++ )
		{
			int upper = array.GetUpperBound(dimension);
			int lower = array.GetLowerBound(dimension);

			for ( int index = lower; index <= upper && bIsFound == false; index++ )
			{

			}
		}

		return bIsFound;
	}
	

	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		LIST

	public	static	bool		AddUnique<T>( this List<T> list, T element, System.Predicate<T> predicate = null )
	{
		System.Predicate<T> finalPredicate = predicate ?? delegate( T e ) { return e.Equals( element ); };
		bool bAlreadyExists = list.FindIndex( finalPredicate ) > -1;
		if ( bAlreadyExists == false )
		{
			list.Add( element );
		}
		return bAlreadyExists;
	}



	/////////////////////////////////////////////////////////////////////////////
	///////////////	UNITY ///////////////

	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		ANIMATOR

	public	static	bool			GetClipFromAnimator( this Animator animator, string name, ref AnimationClip result )
	{
		if ( animator.runtimeAnimatorController == null || animator.runtimeAnimatorController.animationClips.Length == 0 )
		{
			return false;
		}

		AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
		int arraySize = animationClips.Length;
		bool bIsClipFound = false;
		for ( int i = 0; i < arraySize && bIsClipFound == false; i++ )
		{
			AnimationClip clip = animationClips[i];

			if ( clip.name == name )
			{
				bIsClipFound = true;
				result = clip;
			}
		}

		return bIsClipFound;
	}


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		TRANSFORM

	/// <summary> Return true if component is found, otherwise return false </summary>
	public	static	bool			HasComponent<T>( this Transform transform ) where T : Component
	{
		return transform.GetComponent<T>() != null;
	}

	/// <summary> Return the first transform found in child hiearchy with the given name or null if not found </summary>
	public	static	bool			SearchChildWithName( this Transform transform, string childName, ref Transform child )
	{
		return Utils.Base.SearchComponent( transform.gameObject, ref child, SearchContext.CHILDREN, t => t.name == childName );
	}

	/// <summary> Can be used to retrieve a component with more detailed research details </summary>
	public	static	bool			SearchComponent<T>( this Transform transform, ref T Component, SearchContext Context, global::System.Predicate<T> Filter = null ) where T : Component
	{
		return Utils.Base.SearchComponent( transform.gameObject, ref Component, Context, Filter );
	}

	/// <summary> Can be used to retrieve a component's array with more detailed research details </summary>
	public	static	bool			SearchComponents<T>( this Transform transform, ref T[] Component, SearchContext Context, global::System.Predicate<T> Filter = null ) where T : Component
	{
		return Utils.Base.SearchComponents( transform.gameObject, ref Component, Context, Filter );
	}

	/// <summary> Search for a specific component at specific child, if found, return operation result </summary>
	public	static	bool			SearchComponentInChild<T>( this Transform t, string childName, ref T Component) where T : Component
	{
		if ( t.childCount == 0 )
			return false;

		Transform child = t.Find( childName );
		if ( child == null )
			return false;

		Component = child.GetComponent<T>();
		return Component != null;
	}


	/// <summary> Search for a specific component at specific child, if found, return operation result </summary>
	public	static	bool			SearchComponentInChild<T>( this Transform t, int childIndex, ref T Component) where T : Component
	{
		if ( t.childCount == 0 )
			return false;

		if ( t.childCount < childIndex )
			return false;

		Transform child = t.GetChild( childIndex );
		if ( child == null )
			return false;

		Component = child.GetComponent<T>();
		return Component != null;
	} 

	/// <summary> Create and fills up given array with components found paired in childrens to the given enum type </summary>
	public	static	bool			PairComponentsInChildrenIntoArray<T0, T1>( this Transform t, ref T0[] array ) where T0 :Component
	{
		if ( typeof(T1).IsEnum == false )
			return false;

		string[] names = System.Enum.GetNames( typeof(T1) );

		if ( t.childCount < names.Length )
			return false;

		array = new T0[ names.Length ];

		bool bResult = true;
		for ( int i = 0; i < names.Length; i++ )
		{
			Transform child = t.GetChild( i );
			T0 comp = null;
			bResult &= Utils.Base.SearchComponent<T0>( child.gameObject, ref comp, SearchContext.LOCAL );
			array[i] = comp;
		}

		return bResult;
	}

	/// <summary> Search for the given component only in children of given transform </summary>
	public	static	T[]				GetComponentOnlyInChildren<T>( this Transform transform, bool deepSearch = false, bool includeInactive = false  )
	{
		List<T> list = new List<T>();
		{
			for ( int i = 0; i < transform.childCount; i++ )
			{
				Transform child = transform.GetChild( i );

				if ( deepSearch == true )
				{
					T[] childComponents = child.GetComponentsInChildren<T>( includeInactive: includeInactive );
					list.AddRange( childComponents );
				}
				else
				{
					T childComponent = child.GetComponent<T>();
					if ( childComponent != null )
					{
						list.Add( childComponent );
					}
				}

			}
		}
		return list.ToArray();
	}

	/// <summary> Look for given component, if not found add it, return component reference  </summary>
	public	static	T				GetOrAddIfNotFound<T>( this Transform t ) where T : Component
	{
		T result = null;
		if ( ( result = t.GetComponent<T>() ) == null )
		{
			result = t.gameObject.AddComponent<T>();
		}
		return result;
	}


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		GAMEOBJECT

	/// <summary> Look for given component, if not found add it, return component reference  </summary>
	public	static	T				GetOrAddIfNotFound<T>( this GameObject go ) where T : Component
	{
		T result = null;
		if ( ( result = go.GetComponent<T>() ) == null )
		{
			result = go.AddComponent<T>();
		}
		return result;
	}


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		VECTOR2
	public	static	Vector2			ClampComponents( this Vector2 v, float min, float max )
	{
		v.x = Mathf.Clamp( v.x, min, max );
		v.y = Mathf.Clamp( v.y, min, max );
		return v;
	}

	public	static	Vector2			ClampComponents( this Vector2 v, Vector2 clamping )
	{
		v.x = Mathf.Clamp( v.x, -clamping.x, clamping.x );
		v.y = Mathf.Clamp( v.y, -clamping.y, clamping.x );
		return v;
	}



	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		VECTOR3
	public	static	Vector3			ClampComponents( this Vector3 v, float min, float max )
	{
		v.x = Mathf.Clamp( v.x, min, max );
		v.y = Mathf.Clamp( v.y, min, max );
		v.z = Mathf.Clamp( v.z, min, max );
		return v;
	}

	public	static	Vector3			ClampComponents( this Vector3 v, Vector3 clamping )
	{
		v.x = Mathf.Clamp( v.x, -clamping.x, clamping.x );
		v.y = Mathf.Clamp( v.y, -clamping.y, clamping.x );
		v.z = Mathf.Clamp( v.y, -clamping.z, clamping.z );
		return v;
	}
	


	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	//		QUATERNION

	/// <summary> Returna vector which rotation is the given quaternion </summary>
	public	static	Vector3			GetVector( this Quaternion q, Vector3 d )
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

	/// <summary> We need this because Quaternion.Slerp always uses the shortest arc </summary>
	public	static	Quaternion		Slerp( this Quaternion p, Quaternion q, float t)
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

	/// <summary> Return th lenght of a quaternion </summary>
	public	static	float			GetLength( this Quaternion q )
	{
		return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
	}
}


