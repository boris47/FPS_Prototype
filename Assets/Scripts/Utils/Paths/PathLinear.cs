
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
		{
			m_Nodes = transform.GetComponentsInChildren<PathWaypoint>();

			List<Vector3> positionsList = new List<Vector3>( m_Nodes.Length );
			{
				System.Array.ForEach( m_Nodes, ( PathWaypoint w ) => { positionsList.Add( w.transform.position ); } );
			}
			m_Positions = positionsList.ToArray();
		}

		m_PathLength = 0.0f;

		Vector3 prevPosition = m_Positions[0];
		for ( int i = 1; i < m_Positions.Length; i++ )
		{
			Vector3 currentposition = m_Positions[i];
			m_PathLength += Vector3.Distance( prevPosition, currentposition );
			prevPosition = currentposition;
		}
	}


	// 
	public	override	void	IteratePath( System.Action<Vector3, Quaternion> OnPosition )
	{
		if ( OnPosition == null )
		{
			return;
		}

		System.Array.ForEach( m_Positions, ( Vector3 position ) => {
			OnPosition( position, Quaternion.identity );
		});
	}


	//
	public	override	bool	Move( float speed, ref Vector3 position )
	{
		if ( m_IsCompleted )
			return false;

		if ( m_Interpolant == 0.0f && m_CurrentSegment == 0 )
		{
			if ( m_OnPathStart != null && m_OnPathStart.GetPersistentEventCount() > 0 )
			{
				m_OnPathStart.Invoke();
			}
		}

		Vector3 p1 = m_Nodes[ m_CurrentSegment + 0 ].transform.position;
		Vector3 p2 = m_Nodes[ m_CurrentSegment + 1 ].transform.position;

		float magnitude = ( p2 - p1 ).magnitude;
		m_Interpolant += ( Time.deltaTime ) * speed * m_Nodes.Length;

		position = Vector3.Lerp( p1, p2, m_Interpolant );

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

		Vector3 prevPosition = m_Positions[0];
		IteratePath
		(
			OnPosition: ( Vector3 position, Quaternion rotation ) => {
				Gizmos.DrawLine( prevPosition, position );
				prevPosition = position;
			}
		);
	}

}