
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathSpline : PathBase {

	[SerializeField]
	private		PathWaypoint[]		m_Nodes					= null;

	[SerializeField]
	private		bool				m_Cycle					= false;

//	private		Vector3[]			m_Positions				= null;
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

			Vector3 prevPosition = positions[0];
			Vector3 prevDirection = Vector3.zero;
			float currentStep = 0.001f;
			while ( Mathf.Abs( currentStep ) < Steps )
			{
				float interpolant = currentStep / Steps;

				// Position
				Vector3 currentPosition = Utils.Math.GetPoint( positions, interpolant );

				// Rotation
				Vector3 currentDirection = currentPosition - prevPosition;
				Quaternion currentRotation = Quaternion.identity;
				{
					Vector3 right   = Vector3.Cross( currentDirection, prevDirection );
					Vector3 forward = Vector3.Lerp( currentDirection, prevDirection, 0.5f );
					Vector3 upwards = Vector3.Cross( forward, right );
					currentRotation = Quaternion.LookRotation( forward, upwards );
				}

				waypointsList.Add( new PathWayPointOnline() { Position = currentPosition, Rotation = currentRotation } );

				// Path Length
				m_PathLength += Vector3.Distance( prevPosition, currentPosition );

				prevPosition = currentPosition;
				prevDirection = currentDirection;
				currentStep += StepLength;
			}
			m_Waypoints = waypointsList.ToArray();

//			m_Positions = m_Waypoints.Select( w => w.Position ).ToArray();
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

//	private	Vector3 p1Temp, p2Temp;
//	private float rotationSpeed = 80.0f;

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

		
		Vector3 position = t.position;
		Quaternion rotation = t.rotation;

		GetPoint( m_Waypoints, m_Interpolant, ref position, ref rotation );

		t.position = position;
		t.rotation = rotation;
		
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























	private	bool		GetPoint( PathWayPointOnline[] ws, float t, ref Vector3 position, ref Quaternion rotation )
	{
		if ( ws == null || ws.Length < 4 )
		{
			UnityEngine.Debug.Log( "GetPoint Called with points invalid array" );
			UnityEngine.Debug.DebugBreak();
			return false;
		}

		int numSections = ws.Length - 3;
		int currPt = Mathf.Min( Mathf.FloorToInt( t * ( float ) numSections ), numSections - 1 );
		float u = t * ( float ) numSections - ( float ) currPt;

		float rotationInterpolant = 0.0f;
		// Position
		{
			Vector3 p_a = ws[ currPt + 0 ];
			Vector3 p_b = ws[ currPt + 1 ];
			Vector3 p_c = ws[ currPt + 2 ];
			Vector3 p_d = ws[ currPt + 3 ];

			rotationInterpolant = ( p_b - position ).magnitude / ( p_c - p_b ).magnitude;

			position = .5f * 
			(
				( -p_a + 3f * p_b - 3f * p_c + p_d )		* ( u * u * u ) +
				( 2f * p_a - 5f * p_b + 4f * p_c - p_d )	* ( u * u ) +
				( -p_a + p_c )								* u +
				2f * p_b
			);
		}

		// Rotation
		{
//			Vector3 forward, upwards;

			// Forward
//			Vector3 d_a = ws[ currPt + 0 ].Rotation.GetForwardVector();
//			Vector3 d_b = ws[ currPt + 1 ].Rotation.GetForwardVector();
//			Vector3 d_c = ws[ currPt + 2 ].Rotation.GetForwardVector();
//			Vector3 d_d = ws[ currPt + 3 ].Rotation.GetForwardVector();
			
//			forward = Utils.Math.GetPoint( d_a, d_b, d_c, d_d, rotationInterpolant );

			// Upward
//			Vector3 u_a = ws[ currPt + 0 ].Rotation.GetUpVector();
//			Vector3 u_b = ws[ currPt + 1 ].Rotation.GetUpVector();
//			Vector3 u_c = ws[ currPt + 2 ].Rotation.GetUpVector();
//			Vector3 u_d = ws[ currPt + 3 ].Rotation.GetUpVector();

//			upwards = Utils.Math.GetPoint( u_a, u_b, u_c, u_d, 1f -	rotationInterpolant );

//			forward = Vector3.Lerp( d_b, d_c, rotationInterpolant );
//			upwards = Vector3.Lerp( u_b, u_c, rotationInterpolant );

//			Quaternion newRotation = Quaternion.LookRotation( forward, upwards );
			rotation = Quaternion.Lerp( ws[ currPt + 1 ], ws[ currPt + 2 ], rotationInterpolant );
		}

		return true;
	}



}