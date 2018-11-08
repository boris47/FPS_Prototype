﻿
using UnityEngine;
using System.Collections.Generic;

public static class Extensions {

	public	static	T	GetComponent<T>( Transform Base ) where T : Component
	{
		if ( Base == null )
			return null;

		return Base.GetComponent<T>();
	}

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

	public	static	T[]	GetComponentOnlyInChildren<T>( this Transform transform, bool deepSearch = false ) where T : Component
	{
		var list = new System.Collections.Generic.List<T>();
		for ( int i = 0; i < transform.childCount; i++ )
		{
			Transform child = transform.GetChild( i );

			if ( deepSearch == true )
			{
				T[] childComponents = child.GetComponentsInChildren<T>( child );
				if ( childComponents.Length > 0 )
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
		return list.ToArray();
	}

	public	static	string TrimInside( this string str, params char[] trimChars )
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
}


