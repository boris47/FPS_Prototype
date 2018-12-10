
using UnityEngine;
using System.Collections.Generic;

public static class Extensions {

	//		ANIMATOR
	public	static	AnimationClip	GetClipFromAnimator( this Animator animator, string name )
	{
		//favor for above foreach due to performance issues
		for ( int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++ )
		{
			AnimationClip clip = animator.runtimeAnimatorController.animationClips [i];

			if ( clip.name == name )
				return clip;
		}

		return null;
	}


	//		TRANSFORM
	public	static	bool			SearchComponent<T>( this Transform transform, ref T Component, SearchContext Context, global::System.Predicate<T> Filter = null )
	{
		return Utils.Base.SearchComponent( transform.gameObject, ref Component, Context, Filter );
	}

	public	static	bool			SearchComponents<T>( this Transform transform, ref T[] Component, SearchContext Context, global::System.Predicate<T> Filter = null )
	{
		return Utils.Base.SearchComponents( transform.gameObject, ref Component, Context, Filter );
	}

	public	static	T[]				GetComponentOnlyInChildren<T>( this Transform transform, bool deepSearch = false ) where T : Component
	{
		List<T> list = new List<T>();
		{
			for ( int i = 0; i < transform.childCount; i++ )
			{
				Transform child = transform.GetChild( i );

				if ( deepSearch == true )
				{
					T[] childComponents = child.GetComponentsInChildren<T>( child );
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


	//		STRING
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


	//		ARRAY
	public	static	T				GetByIndex<T>( this global::System.Array a, int idx, T Default = default(T) )
	{
		return ( idx > -1 && idx < a.Length ) ? (T)a.GetValue(idx) : Default;
	}


	//		QUATERNION
	public	static	Vector3			GetVector( this Quaternion q, Vector3 d )
	{
		// A quaternion doesn't have a direction by itself. It is a rotation.
		// It can be used to rotate any vector by the rotation it represents. Just multiply a Vector3 by the quaternion.
		// Ref: http://answers.unity.com/answers/525956/view.html
		return q * d;

/*		Vector3 Q = new Vector3( q.x, q.y, q.z );
		Vector3 T = 2.0f * Vector3.Cross( Q, d );
		return d + ( T * q.w ) + Vector3.Cross( Q, T );
*/
	}

	public	static	float			GetLength( this Quaternion q )
	{
		return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
	}
}


