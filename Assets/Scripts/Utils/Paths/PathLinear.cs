
using System.Collections.Generic;
using UnityEngine;

public class PathLinear : PathBase {

	[SerializeField]
	private		PathWaypoint[]		m_Nodes				= null;

	private		int					m_CurrentSegment	= 0;
	

	//
	private				void	Awake()
	{
		ElaboratePath( 100f );
	}


	//
	protected	override	void		ElaboratePath( float Steps, float StepLength = 1.0f )
	{
		// Nodes
		m_Nodes = transform.GetComponentOnlyInChildren<PathWaypoint>();
		m_PathLength = 0.0f;

		// Waypoints
		{
			List<PathWayPointOnline> waypointsList = new List<PathWayPointOnline>( m_Nodes.Length );
			{
				System.Array.ForEach
				( 	m_Nodes,
					( PathWaypoint w ) =>
					{
						waypointsList.Add( new PathWayPointOnline( w.transform.position, w.transform.rotation ) );
					}
				);
			}
			m_Waypoints = waypointsList.ToArray();
		}

		// Path Length
		{
			Vector3 prevPosition = m_Waypoints[0];
			for ( int i = 1; i < m_Waypoints.Length; i++ )
			{
				Vector3 currentposition = m_Waypoints[i];
				m_PathLength += Vector3.Distance( prevPosition, currentposition );
				prevPosition = currentposition;
			}
		}
	}


	// 
	public	override	void	IteratePath( System.Action<PathWayPointOnline> OnPosition )
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
			return false;

		// Start event
		if ( m_Interpolant == 0.0f && m_CurrentSegment == 0 )
		{
			if ( m_OnPathStart != null && m_OnPathStart.GetPersistentEventCount() > 0 )
			{
				m_OnPathStart.Invoke();
			}
		}

		// Interpolant
		m_Interpolant += Time.deltaTime * speed * m_Nodes.Length;

		// Position
		{
			Vector3 p1 = m_Nodes[ m_CurrentSegment + 0 ].transform.position;
			Vector3 p2 = m_Nodes[ m_CurrentSegment + 1 ].transform.position;
			t.position = Vector3.Lerp( p1, p2, m_Interpolant );
		}

		// Rotation
		{
			Vector3 r1 = m_Nodes[ m_CurrentSegment + 0 ].transform.forward;
			Vector3 r2 = m_Nodes[ m_CurrentSegment + 1 ].transform.forward;
			Vector3 rotationLerped = Vector3.Lerp( r1, r2, m_Interpolant );

			// Upwards
			Vector3 finalUpwards = Vector3.zero;
			if ( upwards.HasValue == false )
			{
				Vector3 u1 = m_Nodes[ m_CurrentSegment + 0 ].transform.up;
				Vector3 u2 = m_Nodes[ m_CurrentSegment + 1 ].transform.up;

				Vector3 upwardsLerped = Vector3.Lerp( u1, u2, m_Interpolant );
				finalUpwards = upwardsLerped;
			}
			else
			{
				finalUpwards = upwards.GetValueOrDefault();
			}

			t.rotation = Quaternion.LookRotation( rotationLerped, finalUpwards );
		}

		// Interpolant upgrade
		if ( m_Interpolant > 1.0f )
		{
			m_Interpolant = 0.0f;
			m_Nodes[ m_CurrentSegment ].OnReached();
			m_CurrentSegment ++;
			if ( m_CurrentSegment == m_Nodes.Length - 1 )
			{
				m_IsCompleted = true;

				if ( m_OnPathCompleted != null && m_OnPathCompleted.GetPersistentEventCount() > 0 )
				{
					m_OnPathCompleted.Invoke();
				}
			}
		}

		return true;
	}


	//
	public override void ResetPath()
	{
		base.ResetPath();
		m_CurrentSegment = 0;
	}


	// Called by waypoints
	public	override	void	DrawGizmos()
	{
		OnDrawGizmosSelected();
	}
		
	//
	private				void	OnDrawGizmosSelected()
	{
		ElaboratePath( Steps: 100f );

		Vector3 prevPosition = m_Waypoints[0];
		IteratePath
		(
			OnPosition: ( PathWayPointOnline w ) => {
				Gizmos.DrawLine( prevPosition, w.Position );
				prevPosition = w.Position;
			}
		);
	}

}