
using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public	struct	PathWayPointOnline {

	public	Vector3		Position;
	public	Quaternion	Rotation;

	public static implicit operator Vector3		( PathWayPointOnline w )		{	return w.Position;	}
	public static implicit operator Quaternion	( PathWayPointOnline w )		{	return w.Rotation;	}

}

public abstract	class PathBase : MonoBehaviour {

	[SerializeField]
	protected		GameEvent				m_OnPathStart		= null;

	[SerializeField]
	protected		GameEvent				m_OnPathCompleted	= null;


	protected		float					m_Interpolant		= 0f;
	protected		bool					m_IsCompleted		= false;

	protected		PathWayPointOnline[]	m_Waypoints			= null;

	protected		float					m_PathLength		= 0.0f;
	public			float					PathLength
	{
		get { return m_PathLength; }
	}


	// 
	public		abstract void	IteratePath( System.Action<PathWayPointOnline> OnPosition );


	// 
	protected	abstract void	ElaboratePath( float Steps, float StepLength = 1.0f );


	// 
	public		abstract bool	Move( float speed, ref Vector3 position, ref Quaternion rotation, Vector3 upwards = new Vector3() );


	// 
	public		virtual void	ResetPath()
	{
		m_Interpolant		= 0f;
		m_IsCompleted		= false;
	}


	// 
	public		abstract void	DrawGizmos();
}