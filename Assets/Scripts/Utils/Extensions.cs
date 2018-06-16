
using UnityEngine;

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

}
