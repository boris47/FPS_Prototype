﻿

using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PathSpline : PathBase {

	[SerializeField]
	private		bool					m_Cycle					= false;

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

				// Last node
				waypointsList.Add ( new PathWayPointOnline( m_Nodes.Last().transform ) );
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

		System.Array.ForEach( m_Waypoints, ( PathWayPointOnline wayPoint ) => {
			OnPosition( wayPoint );
		});
	}


	//
	public	override	bool	Move( float speed, ref Transform t, Vector3? upwards )
	{
		if ( m_IsCompleted )
		{
			if ( m_Cycle == true )
			{
				ResetPath();
			}
			else
			{
				return false;
			}
		}

		// Start event
		if ( m_IsStartEventCalled == false && m_Interpolant == 0.0f )
		{
			if ( m_OnPathStart != null && m_OnPathStart.GetPersistentEventCount() > 0 )
			{
				m_OnPathStart.Invoke();
			}
			m_IsStartEventCalled = true;
		}

		// Interpolant
		m_Interpolant += ( Time.deltaTime ) * speed;

		// End event
		if ( m_IsEndEventCalled == false && m_Interpolant >= 1.0f )
		{
			m_IsCompleted = true;

			if ( m_OnPathCompleted != null && m_OnPathCompleted.GetPersistentEventCount() > 0 )
			{
				m_OnPathCompleted.Invoke();
			}
			m_IsEndEventCalled = true;
		}

		// Interpolation
		Vector3 position		= t.position;
		Quaternion rotation		= t.rotation;

		Utils.Math.GetInterpolatedWaypoint( m_Waypoints, m_Interpolant, ref position, ref rotation );

		if ( upwards.HasValue )
		{
			rotation = Quaternion.LookRotation( rotation.GetVector( Vector3.forward ), upwards.Value );
		}

		t.position = position;
		t.rotation = rotation;

		return true;
	}
		
	// called by childs
	public	override	void	DrawGizmos()
	{
		OnDrawGizmosSelected();
	}
		

	private void	OnDrawGizmosSelected()
	{
		const float Steps = 500f;
		const float StepLength = 1.0f;

		ElaboratePath( Steps: Steps );

		Quaternion rotation = Quaternion.identity;
		Vector3 currPosition = Vector3.zero;
		Vector3 prevPosition = m_Waypoints[0];
		float currentStep = 0.001f;
		while ( currentStep < Steps )
		{
			float interpolant = currentStep / Steps;

			Utils.Math.GetInterpolatedWaypoint( m_Waypoints, interpolant, ref currPosition, ref rotation );

			Gizmos.DrawLine( prevPosition, currPosition );
			Gizmos.DrawRay( currPosition, rotation * ( Vector3.forward * 3f ) );

			prevPosition = currPosition;
			currentStep += StepLength; 
		}
	}

}



/*
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathSpline : PathBase {

	[SerializeField]
	private		PathWaypoint[]		m_Nodes					= null;

	[SerializeField]
	private		bool				m_Cycle					= false;

	private		bool				m_IsStartEventCalled	= false;
	private		bool				m_IsEndEventCalled		= false;

	GameObject sphere1, sphere2;


	// 
	private				void			Awake()
	{
		ElaboratePath( Steps: 250 );
		
		sphere1 = GameObject.CreatePrimitive( PrimitiveType.Sphere );
		sphere2 = GameObject.CreatePrimitive( PrimitiveType.Sphere );

		sphere1.GetComponent<Renderer>().material.color = Color.red;
		sphere2.GetComponent<Renderer>().material.color = Color.green;

		sphere1.GetComponent<Collider>().enabled = sphere2.GetComponent<Collider>().enabled = false;

		sphere1.transform.localScale = Vector3.one * 0.1f;
		sphere2.transform.localScale = Vector3.one * 0.1f;
		
	}


	//
	protected	override	void			ElaboratePath( float Steps, float StepLength = 1.0f )
	{
		// Nodes
		m_Nodes = transform.GetComponentsInChildren<PathWaypoint>();

		// Spline base points
		PathWayPointOnline[] wayPoints = null;
		{
			List<PathWayPointOnline> waypointsList = new List<PathWayPointOnline>( m_Nodes.Length + 2 );
			{
				// Fist node
				waypointsList.Add ( new PathWayPointOnline( m_Nodes.First().transform ) );

				// All middle nodes
				waypointsList.AddRange( m_Nodes.Select( w => new PathWayPointOnline( w.transform ) ) );

				// Last node
				waypointsList.Add ( new PathWayPointOnline( m_Nodes.Last().transform ) );
			}
			wayPoints = waypointsList.ToArray();
		}

		// Waypoints and path length
		{
			List<PathWayPointOnline> waypointsList = new List<PathWayPointOnline>();

			Vector3 prevPosition		= wayPoints[0];
			Vector3 currentPosition		= wayPoints[0];
			Quaternion currentRotation	= wayPoints[0];
			float currentStep			= 0.001f;
			while ( Mathf.Abs( currentStep ) < Steps )
			{
				float interpolant = currentStep / Steps;

				Utils.Math.GetInterpolatedWaypoint( wayPoints, interpolant, ref currentPosition, ref currentRotation );

				waypointsList.Add( new PathWayPointOnline( currentPosition, currentRotation ) );

				// Path Length
				m_PathLength += Vector3.Distance( prevPosition, currentPosition );

				prevPosition = currentPosition;

				currentStep += StepLength;
			}
			m_Waypoints = waypointsList.ToArray();
		}
		
	}


	// Spline iteration
	public 		override	void			IteratePath( System.Action<PathWayPointOnline> OnPosition )
	{
		if ( OnPosition == null )
		{
			return;
		}

		System.Array.ForEach
		( 
			m_Waypoints,
			( PathWayPointOnline wayPoint ) =>
			{
				OnPosition( wayPoint );
			}
		);
	}


	//
	public		override	bool			Move( float speed, ref Transform t, Vector3? upwards )
	{
		if ( m_IsCompleted )
		{
			if ( m_Cycle == true )
			{
				ResetPath();
			}
			else
			{
				return false;
			}
		}

		// Start event
		if ( m_IsStartEventCalled == false && m_Interpolant == 0.0f )
		{
			if ( m_OnPathStart != null && m_OnPathStart.GetPersistentEventCount() > 0 )
			{
				m_OnPathStart.Invoke();
			}
			m_IsStartEventCalled = true;
		}

		// Interpolant
		m_Interpolant += ( Time.deltaTime ) * speed;

		// End event
		if ( m_IsEndEventCalled == false && m_Interpolant >= 1.0f )
		{
			m_IsCompleted = true;

			if ( m_OnPathCompleted != null && m_OnPathCompleted.GetPersistentEventCount() > 0 )
			{
				m_OnPathCompleted.Invoke();
			}
			m_IsEndEventCalled = true;
		}
		
		// get position and rotation asnd assign them
		Vector3 position = t.position;	Quaternion rotation = t.rotation;
		{
			Utils.Math.GetInterpolatedWaypoint( m_Waypoints, m_Interpolant, ref position, ref rotation );
		}
		t.position = position;	t.rotation = rotation;
		
		return true;
	}
	

	// called by childs
	public	override	void			DrawGizmos()
	{
		OnDrawGizmosSelected();
	}
	

	// 
	private				void			OnDrawGizmosSelected()
	{
		ElaboratePath( Steps: 150f );

		Vector3 prevPosition = m_Waypoints[0];
		IteratePath
		(
			OnPosition: ( PathWayPointOnline w ) => {
				Gizmos.DrawLine( prevPosition, w );
				Gizmos.DrawRay( w.Position, w.Rotation * Vector3.forward );
				prevPosition = w;
			}
		);
	}

}
*/