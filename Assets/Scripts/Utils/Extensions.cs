
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

}
