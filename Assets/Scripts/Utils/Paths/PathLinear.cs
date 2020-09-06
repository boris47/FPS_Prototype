
using System.Collections.Generic;
using UnityEngine;

public class PathLinear : PathBase {

	[SerializeField]
	private		PathWaypoint[]		m_Nodes				= null;

	private		int					m_CurrentSegment	= 0;
	

	//
	private				void	Awake()
	{
		this.ElaboratePath( 100f );
	}


	//
	protected	override	void		ElaboratePath( float Steps, float StepLength = 1.0f )
	{
		// Nodes
		this.m_Nodes = this.transform.GetComponentOnlyInChildren<PathWaypoint>();
		this.m_PathLength = 0.0f;

		// Waypoints
		{
			List<PathWayPointOnline> waypointsList = new List<PathWayPointOnline>(this.m_Nodes.Length );
			{
				System.Array.ForEach
				(this.m_Nodes,
					( PathWaypoint w ) =>
					{
						waypointsList.Add( new PathWayPointOnline( w.transform.position, w.transform.rotation ) );
					}
				);
			}
			this.m_Waypoints = waypointsList.ToArray();
		}

		// Path Length
		{
			Vector3 prevPosition = this.m_Waypoints[0];
			for ( int i = 1; i < this.m_Waypoints.Length; i++ )
			{
				Vector3 currentposition = this.m_Waypoints[i];
				this.m_PathLength += Vector3.Distance( prevPosition, currentposition );
				prevPosition = currentposition;
			}
		}
	}


	// 
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
			return false;

		// Start event
		if (this.m_Interpolant == 0.0f && this.m_CurrentSegment == 0 )
		{
			if (this.m_OnPathStart != null && this.m_OnPathStart.GetPersistentEventCount() > 0 )
			{
				this.m_OnPathStart.Invoke();
			}
		}

		// Interpolant
		this.m_Interpolant += Time.deltaTime * ( speed.HasValue ? speed.Value : this.m_Speed ) * this.m_Nodes.Length;

		// Position
		{
			Vector3 p1 = this.m_Nodes[this.m_CurrentSegment + 0 ].transform.position;
			Vector3 p2 = this.m_Nodes[this.m_CurrentSegment + 1 ].transform.position;
			subject.position = Vector3.Lerp( p1, p2, this.m_Interpolant );
		}

		// Rotation
		{
			Vector3 r1 = this.m_Nodes[this.m_CurrentSegment + 0 ].transform.forward;
			Vector3 r2 = this.m_Nodes[this.m_CurrentSegment + 1 ].transform.forward;
			Vector3 rotationLerped = Vector3.Lerp( r1, r2, this.m_Interpolant );

			// Upwards
			Vector3 finalUpwards = Vector3.zero;
			if ( upwards.HasValue == false )
			{
				Vector3 u1 = this.m_Nodes[this.m_CurrentSegment + 0 ].transform.up;
				Vector3 u2 = this.m_Nodes[this.m_CurrentSegment + 1 ].transform.up;

				Vector3 upwardsLerped = Vector3.Lerp( u1, u2, this.m_Interpolant );
				finalUpwards = upwardsLerped;
			}
			else
			{
				finalUpwards = upwards.GetValueOrDefault();
			}

			subject.rotation = Quaternion.LookRotation( rotationLerped, finalUpwards );
		}

		// Interpolant upgrade
		if (this.m_Interpolant > 1.0f )
		{
			this.m_Interpolant = 0.0f;
			this.m_Nodes[this.m_CurrentSegment ].OnReached();
			this.m_CurrentSegment ++;
			if (this.m_CurrentSegment == this.m_Nodes.Length - 1 )
			{
				this.m_IsCompleted = true;

				if (this.m_OnPathCompleted != null && this.m_OnPathCompleted.GetPersistentEventCount() > 0 )
				{
					this.m_OnPathCompleted.Invoke();
				}
			}
		}

		return true;
	}


	//
	public override void ResetPath()
	{
		base.ResetPath();
		this.m_CurrentSegment = 0;
	}


	// Called by waypoints
	public	override	void	DrawGizmos()
	{
		this.OnDrawGizmosSelected();
	}
		
	//
	private				void	OnDrawGizmosSelected()
	{
		this.ElaboratePath( Steps: 100f );

		Vector3 prevPosition = this.m_Waypoints[0];
		this.IteratePath
		(
			OnPosition: ( PathWayPointOnline w ) => {
				Gizmos.DrawLine( prevPosition, w.Position );
				prevPosition = w.Position;
			}
		);
	}

}