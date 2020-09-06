

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
		this.ElaboratePath( Steps: 250 );
	}


	//
	protected override void ElaboratePath( float Steps, float StepLength = 1.0f )
	{
		// Nodes
		PathWaypoint[] m_Nodes = this.transform.GetComponentsInChildren<PathWaypoint>();

		// Spline base points
		{
			List<PathWayPointOnline> waypointsList = new List<PathWayPointOnline>( m_Nodes.Length + 2 );
			{
				// Fist node
				waypointsList.Add ( new PathWayPointOnline( m_Nodes.First().transform ) );

				// All middle nodes
				waypointsList.AddRange( m_Nodes.Select( w => new PathWayPointOnline( w.transform ) ) );

				if (this.m_IsCyclic )
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
			this.m_Waypoints = waypointsList.ToArray();
		}
	}


	// Spline iteration
	public 	override void	IteratePath( System.Action<PathWayPointOnline> OnPosition )
	{
		if ( OnPosition == null )
		{
			return;
		}

		System.Array.ForEach(this.m_Waypoints, ( PathWayPointOnline wayPoint ) => {
			OnPosition( wayPoint );
		});
	}


	//
	public	override	bool	Move( ref Transform subject, float? speed, Vector3? upwards )
	{
		if (this.m_IsCompleted )
		{
			if (this.m_IsCyclic == true )
			{
				this.ResetPath();
			}
			else
			{
				return true;
			}
		}

		// Start event
		if (this.m_IsStartEventCalled == false && this.m_Interpolant == 0.0f )
		{
			if (this.m_OnPathStart != null && this.m_OnPathStart.GetPersistentEventCount() > 0 )
			{
				this.m_OnPathStart.Invoke();
			}
			this.m_IsStartEventCalled = true;
		}

		// Interpolant
		this.m_Interpolant += ( Time.deltaTime ) * ( speed.HasValue ? speed.Value : this.m_Speed );

		// End event
		if ( Mathf.Abs(this.m_Interpolant) >= 1.0f && this.m_IsCompleted == false )
		{
			this.m_IsCompleted = true;

			if (this.m_IsEndEventCalled == false && this.m_OnPathCompleted != null && this.m_OnPathCompleted.GetPersistentEventCount() > 0 )
			{
				this.m_OnPathCompleted.Invoke();
			}
			this.m_IsEndEventCalled = true;
		}

		if ( Mathf.Abs(this.m_Interpolant) >= 1.0f )
		{
			this.m_Interpolant = 0.0f;
		}

		// Interpolation
		Vector3 position		= subject.position;
		Quaternion rotation		= subject.rotation;

		Utils.Math.GetInterpolatedWaypoint(this.m_Waypoints, this.m_Interpolant, ref position, ref rotation );

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
		this.OnDrawGizmosSelected();
	}
		
	public bool reversed;
	private void	OnDrawGizmosSelected()
	{
		const float Steps = 50f;
		const float StepLength = 1.0f;

		this.ElaboratePath( Steps: Steps );

		Vector3 prevPosition = this.m_Waypoints[0];
		Vector3 currPosition = Vector3.zero;
		Quaternion rotation = Quaternion.identity;

		// Very first start point
		{
			Utils.Math.GetInterpolatedWaypoint(this.m_Waypoints, 0.0f, ref currPosition, ref rotation );

			Gizmos.DrawLine( prevPosition, currPosition );
			Gizmos.DrawRay( currPosition, rotation * ( Vector3.forward * 3f ) );

			prevPosition = currPosition;
		}
		
		// Mid points
		float currentStep = 0.001f;
		while ( currentStep < Steps )
		{
			float interpolant = Mathf.Clamp01( currentStep / Steps );

			Utils.Math.GetInterpolatedWaypoint(this.m_Waypoints, this.reversed ? -interpolant : interpolant, ref currPosition, ref rotation );

			Gizmos.DrawLine( prevPosition, currPosition );
			Gizmos.DrawRay( currPosition, rotation * ( Vector3.forward * 3f ) );

			prevPosition = currPosition;
			currentStep += StepLength; 
		}
		
		// End point
		{
			Utils.Math.GetInterpolatedWaypoint(this.m_Waypoints, this.reversed ? -1.0f:1.0f, ref currPosition, ref rotation );

			Gizmos.DrawLine( prevPosition, currPosition );
			Gizmos.DrawRay( currPosition, rotation * ( Vector3.forward * 3f ) );
		}
		
	}
	

}

