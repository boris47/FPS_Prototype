

using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PathSpline : PathBase {

	[SerializeField]
	private		bool					m_IsCyclic				= false;

	private		bool					m_IsStartEventCalled	= false;
	private		bool					m_IsEndEventCalled		= false;


	private void	Awake()
	{
		ElaboratePath( Steps: 250 );
	}


	//
	protected override void ElaboratePath( float Steps, float StepLength = 1.0f )
	{
		// Nodes
		PathWaypoint[] m_Nodes = transform.GetComponentsInChildren<PathWaypoint>();

		// Spline base points
		{
			List<PathWayPointOnline> waypointsList = new List<PathWayPointOnline>( m_Nodes.Length + 2 );
			{
				// Fist node
				waypointsList.Add ( new PathWayPointOnline( m_Nodes.First().transform ) );

				// All middle nodes
				waypointsList.AddRange( m_Nodes.Select( w => new PathWayPointOnline( w.transform ) ) );

				if (m_IsCyclic )
				{
					// Fist node
					waypointsList.Add ( new PathWayPointOnline( m_Nodes.First().transform ) );
					waypointsList.Add ( new PathWayPointOnline( m_Nodes.First().transform ) );
				}
				else
				{
					// Last node
					waypointsList.Add ( new PathWayPointOnline( m_Nodes.Last().transform ) );
				}
			}
			m_Waypoints = waypointsList.ToArray();
		}
	}


	// Spline iteration
	public 	override void	IteratePath( System.Action<PathWayPointOnline> OnPosition )
	{
		if ( OnPosition == null )
		{
			return;
		}

		System.Array.ForEach(m_Waypoints, ( PathWayPointOnline wayPoint ) => {
			OnPosition( wayPoint );
		});
	}


	//
	public	override	bool	Move( ref Transform subject, float? speed, Vector3? upwards )
	{
		if (m_IsCompleted )
		{
			if (m_IsCyclic == true )
			{
				ResetPath();
			}
			else
			{
				return true;
			}
		}

		// Start event
		if (m_IsStartEventCalled == false && m_Interpolant == 0.0f )
		{
			if (m_OnPathStart != null && m_OnPathStart.GetPersistentEventCount() > 0 )
			{
				m_OnPathStart.Invoke();
			}
			m_IsStartEventCalled = true;
		}

		// Interpolant
		m_Interpolant += ( Time.deltaTime ) * ( speed.HasValue ? speed.Value : m_Speed );

		// End event
		if ( Mathf.Abs(m_Interpolant) >= 1.0f && m_IsCompleted == false )
		{
			m_IsCompleted = true;

			if (m_IsEndEventCalled == false && m_OnPathCompleted != null && m_OnPathCompleted.GetPersistentEventCount() > 0 )
			{
				m_OnPathCompleted.Invoke();
			}
			m_IsEndEventCalled = true;
		}

		if ( Mathf.Abs(m_Interpolant) >= 1.0f )
		{
			m_Interpolant = 0.0f;
		}

		// Interpolation
		Vector3 position		= subject.position;
		Quaternion rotation		= subject.rotation;

		Utils.Math.GetInterpolatedWaypoint(m_Waypoints, m_Interpolant, ref position, ref rotation );

		if ( upwards.HasValue )
		{
			rotation = Quaternion.LookRotation( rotation.GetVector( Vector3.forward ), upwards.Value );
		}

		subject.position = position;
		subject.rotation = rotation;

		return false;
	}
		
	// called by childs
	public	override	void	DrawGizmos()
	{
		OnDrawGizmosSelected();
	}
		
	public bool reversed;
	private void	OnDrawGizmosSelected()
	{
		const float Steps = 50f;
		const float StepLength = 1.0f;

		ElaboratePath( Steps: Steps );

		Vector3 prevPosition = m_Waypoints[0];
		Vector3 currPosition = Vector3.zero;
		Quaternion rotation = Quaternion.identity;

		// Very first start point
		{
			Utils.Math.GetInterpolatedWaypoint(m_Waypoints, 0.0f, ref currPosition, ref rotation );

			Gizmos.DrawLine( prevPosition, currPosition );
			Gizmos.DrawRay( currPosition, rotation * ( Vector3.forward * 3f ) );

			prevPosition = currPosition;
		}
		
		// Mid points
		float currentStep = 0.001f;
		while ( currentStep < Steps )
		{
			float interpolant = Mathf.Clamp01( currentStep / Steps );

			Utils.Math.GetInterpolatedWaypoint(m_Waypoints, reversed ? -interpolant : interpolant, ref currPosition, ref rotation );

			Gizmos.DrawLine( prevPosition, currPosition );
			Gizmos.DrawRay( currPosition, rotation * ( Vector3.forward * 3f ) );

			prevPosition = currPosition;
			currentStep += StepLength; 
		}
		
		// End point
		{
			Utils.Math.GetInterpolatedWaypoint(m_Waypoints, reversed ? -1.0f:1.0f, ref currPosition, ref rotation );

			Gizmos.DrawLine( prevPosition, currPosition );
			Gizmos.DrawRay( currPosition, rotation * ( Vector3.forward * 3f ) );
		}
		
	}
	

}

