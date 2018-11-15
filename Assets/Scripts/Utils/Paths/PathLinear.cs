
using System.Collections.Generic;
using UnityEngine;

public class PathLinear : PathBase {

	[SerializeField]
	private		PathWaypoint[]		m_Nodes			= null;

	private		int					m_CurrentSegment	= 0;
	

	//
	private				void	Awake()
	{
		m_Nodes = FindNodes();

		Vector3 prevPosition = m_Nodes[0].transform.position;

		for( int i = 1; i < m_Nodes.Length; i ++ )
		{
			Vector3 currentPostion = m_Nodes[i].transform.position;
			m_PathLength += Vector3.Distance( prevPosition, currentPostion );
			prevPosition = currentPostion;
		}

	}


	//
	private				PathWaypoint[]	FindNodes()
	{
		return transform.GetComponentsInChildren<PathWaypoint>();
	}


	// 
	public	override	void	IteratePath( float Steps, System.Action<Vector3, Quaternion> OnPosition )
	{
		if ( OnPosition == null || Steps > m_PathLength )
		{
			return;
		}

		Vector3 prevPosition = m_Nodes[0].transform.position;
		if ( Steps > 0.0f )// 100f
		{
			float stepLength = m_PathLength / Steps;

		}
		else
		{
			for( int i = 1; i < m_Nodes.Length; i ++ )
			{
				Vector3 currentPostion = m_Nodes[i].transform.position;

				OnPosition( currentPostion, Quaternion.identity );

				prevPosition = currentPostion;
			}
		}
	}


	//
	public	override	bool	Move( float speed, ref Vector3 position )
	{
		if ( m_IsCompleted )
			return false;

		float magnitude = ( m_Nodes[ m_CurrentSegment + 1 ].transform.position - m_Nodes[ m_CurrentSegment ].transform.position ).magnitude;
		m_Interpolant += ( Time.deltaTime * 1.0f / magnitude ) * speed;
		if ( m_Interpolant > 1f )
		{
			m_Interpolant = 0.0f;
			m_CurrentSegment ++;
			if ( m_CurrentSegment == m_Nodes.Length - 1 )
			{
				m_IsCompleted = true;
				return false;
			}
		}

		position = Interpolation( m_CurrentSegment, m_Interpolant );
		return true;
	}


	//
	private	Vector3 Interpolation( int segment, float interpolant )
	{
		Vector3 p1 = Vector3.zero;
		Vector3 p2 = Vector3.zero;
		Vector3 p3 = Vector3.zero;
		Vector3 p4 = Vector3.zero;
		if ( segment == 0 )
		{
			p1 = m_Nodes[ segment + 0 ].transform.position;
			p3 = m_Nodes[ segment + 1 ].transform.position;
			p4 = m_Nodes[ segment + 2 ].transform.position;
			p2 = p1;
		}
		else
		if ( segment == m_Nodes.Length - 2 )
		{
			p1 = m_Nodes[ segment - 1 ].transform.position;
			p2 = m_Nodes[ segment + 0 ].transform.position;
			p3 = m_Nodes[ segment + 1 ].transform.position;
			p4 = p3;
		}
		else
		if ( m_Interpolant >= 1.0f )
		{
			p1 = m_Nodes[ segment - 1 ].transform.position;
			p2 = m_Nodes[ segment + 0 ].transform.position;
			p3 = m_Nodes[ segment + 1 ].transform.position;
			p4 = m_Nodes[ segment + 2 ].transform.position;
		}
		return Utils.Math.GetPoint( p1, p2, p3, p4, interpolant );

	}


	//
	public override void ResetPath()
	{
		base.ResetPath();

	}


	// Called by waypoints
	public	override	void	DrawGizmos()
	{
		OnDrawGizmosSelected();
	}
		
	//
	private				void	OnDrawGizmosSelected()
	{
		
	}

}