
using System.Collections.Generic;
using UnityEngine;

public abstract	class PathBase : MonoBehaviour {

	[SerializeField]
	protected		GameEvent			m_OnPathCompleted	= null;

	protected		float				m_Interpolant		= 0f;
	protected		bool				m_IsCompleted		= false;

	protected		float				m_PathLength		= 0.0f;
	public			float				PathLength
	{
		get { return m_PathLength; }
	}


	// 
	public	abstract void	IteratePath( float Steps, System.Action<Vector3, Quaternion> OnPosition );


	// 
	public	abstract bool	Move( float speed, ref Vector3 position );


	// 
	public	virtual void	ResetPath()
	{
		m_Interpolant		= 0f;
		m_IsCompleted		= false;
	}


	// 
	public	abstract void	DrawGizmos();
}