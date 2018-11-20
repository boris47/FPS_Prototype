
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathSpline : PathBase {

	[SerializeField]
	private		PathWaypoint[]		m_Nodes				= null;

	private		Vector3[]			m_Positions			= null;

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
	}


	//
	protected	override	void			ElaboratePath( float Steps, float StepLength = 1.0f )
	{
		// Nodes
		m_Nodes = transform.GetComponentsInChildren<PathWaypoint>();

		// Spline base points
		Vector3[] positions = null;
		{
			List<Vector3> positionsList = new List<Vector3>( m_Nodes.Length + 2 );
			{
				// Fist node
				positionsList.Add ( m_Nodes.First().transform.position );

				// All middle nodes
				positionsList.AddRange( m_Nodes.Select( w => w.transform.position ).ToArray() );

				// Last node
				positionsList.Add ( m_Nodes.Last().transform.position );
				
			}
			
			positions = positionsList.ToArray();
		}
		
		// Waypoints and path length
		{
			List<PathWayPointOnline> waypointsList = new List<PathWayPointOnline>();

			Vector3 bump;

			Vector3 prevPosition = positions[0];
			float currentStep = 0.001f;
			while ( currentStep < Steps )
			{
				float interpolant = currentStep / Steps;
				Vector3 currentPosition = Utils.Math.GetPoint( positions, interpolant, out bump, out bump );

				waypointsList.Add
				(
					new PathWayPointOnline() {
						Position = currentPosition,
						Rotation = Quaternion.identity // Quaternion.LookRotation( prevPosition - currentPosition + ( Vector3.up * 0.001f ), Vector3.up )
					}

				);

				// Path Length
				m_PathLength += Vector3.Distance( prevPosition, currentPosition );

				prevPosition = currentPosition;
				currentStep += StepLength;
			}
			m_Waypoints = waypointsList.ToArray();

			m_Positions = m_Waypoints.Select( w => w.Position ).ToArray();
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

	private	Vector3 p1Temp, p2Temp;
	private float rotationSpeed = 1.0f;
	//
	public		override	bool			Move( float speed, ref Vector3 position, ref Quaternion rotation, Vector3 upwards = new Vector3() )
	{
		if ( m_IsCompleted )
			return false;

		// Start event
		if ( m_Interpolant == 0.0f )
		{
			if ( m_OnPathStart != null && m_OnPathStart.GetPersistentEventCount() > 0 )
			{
				m_OnPathStart.Invoke();
			}
		}

		// Interpolant
		m_Interpolant += ( Time.deltaTime ) * speed;

		// Position

		Vector3 p1, p2;

		position = Utils.Math.GetPoint( m_Positions, m_Interpolant , out p1, out p2 );

//		sphere1.transform.position = p1; // red
//		sphere2.transform.position = p2; // green

		p1Temp = Vector3.Lerp( p1Temp, p1, ( Time.deltaTime * rotationSpeed ) );
		p2Temp = Vector3.Lerp( p2Temp, p2, ( Time.deltaTime * rotationSpeed) );

		// Rotation
		{
			float t = ( p1Temp - position ).magnitude / ( p2Temp - p1Temp ).magnitude;
			Vector3 direction = ( p2Temp - p1Temp );
			Quaternion newRotation = Quaternion.LookRotation( direction, upwards );

			float angleNow  = Quaternion.Angle( rotation, newRotation );
			rotationSpeed = 1.0f + angleNow;

			rotation = Quaternion.Slerp( rotation, newRotation, t * rotationSpeed );
		}

		if ( m_Interpolant >= 1.0f )
		{
			m_IsCompleted = true;

			if ( m_OnPathCompleted != null && m_OnPathCompleted.GetPersistentEventCount() > 0 )
			{
				m_OnPathCompleted.Invoke();
			}
		}
		
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
		ElaboratePath( Steps: 250 );

		Vector3 prevPosition = m_Waypoints[0];
		IteratePath
		(
			OnPosition: ( PathWayPointOnline w ) => {
				Gizmos.DrawLine( prevPosition, w );
				prevPosition = w;
			}
		);
	}

}