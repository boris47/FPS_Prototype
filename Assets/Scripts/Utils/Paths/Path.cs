
using System.Collections.Generic;
using UnityEngine;

public abstract	class PathBase : MonoBehaviour {

	public	abstract void	IterateSpline( float Steps, float StepLength, System.Action<Vector3, Quaternion> OnPosition );

	public	abstract bool	Move( float speed, ref Vector3 position );

	public	abstract void	DrawGizmos();
}