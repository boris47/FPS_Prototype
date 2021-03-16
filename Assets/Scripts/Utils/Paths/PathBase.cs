
using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public	struct	PathWayPointOnline {

	private	Vector3		m_Position;
	private	Quaternion	m_Rotation;

	public	Vector3		Position
	{
		get { return m_Position; }
	}

	public	Quaternion	Rotation
	{
		get { return m_Rotation; }
	}

	public PathWayPointOnline( Transform t )
	{
		m_Position = t.position;
		m_Rotation = t.rotation;
	}

	public PathWayPointOnline( Vector3 position, Quaternion rotation )
	{
		m_Position = position;
		m_Rotation = rotation;
	}

	public static implicit operator Vector3		( PathWayPointOnline w )		{	return w.Position;	}
	public static implicit operator Quaternion	( PathWayPointOnline w )		{	return w.Rotation;	}

}

public abstract	class PathBase : MonoBehaviour {

	[SerializeField]
	protected		GameEvent				m_OnPathStart		= null;

	[SerializeField]
	protected		GameEvent				m_OnPathCompleted	= null;

	[SerializeField]
	protected		float					m_Speed				= 0.02f;


	protected		float					m_Interpolant		= 0f;
	protected		bool					m_IsCompleted		= false;

	protected		PathWayPointOnline[]	m_Waypoints			= null;

	protected		float					m_PathLength		= 0.0f;
	public			float					PathLength
	{
		get { return m_PathLength; }
	}

	// 
	protected	abstract void	ElaboratePath(float Steps, float StepLength = 1.0f);


	// 
	public		abstract void	IteratePath(System.Action<PathWayPointOnline> OnPosition);


	// 
	public		abstract bool	Move(Transform subject, float? speed, Vector3? upwards);


	// 
	public		virtual void	ResetPath()
	{
		m_Interpolant		= 0f;
		m_IsCompleted		= false;
	}


	// 
	public		abstract void	DrawGizmos();
}