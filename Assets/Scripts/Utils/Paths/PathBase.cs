
using System.Collections.Generic;
using UnityEngine;

public	class	PathWayPointOnline {

	public	Vector3	Position = Vector3.zero;

}

public abstract	class PathBase : MonoBehaviour {

	[SerializeField]
	protected		GameEvent			m_OnPathStart		= null;

	[SerializeField]
	protected		GameEvent			m_OnPathCompleted	= null;


	protected		float				m_Interpolant		= 0f;
	protected		bool				m_IsCompleted		= false;

	protected		Vector3[]			m_Positions			= null;

	protected		float				m_PathLength		= 0.0f;
	public			float				PathLength
	{
		get { return m_PathLength; }
	}


	// 
	public		abstract void	IteratePath( System.Action<Vector3, Quaternion> OnPosition );


	// 
	protected	abstract void	ElaboratePath( float Steps, float StepLength = 1.0f );


	// 
	public		abstract bool	Move( float speed, ref Vector3 position );


	// 
	public		virtual void	ResetPath()
	{
		m_Interpolant		= 0f;
		m_IsCompleted		= false;
	}


	// 
	public		abstract void	DrawGizmos();
}