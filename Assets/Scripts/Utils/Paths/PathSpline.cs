
using System.Collections.Generic;
using UnityEngine;

public class PathSpline : PathBase {

	private		Vector3[]			m_Nodes				= null;

	private		Vector3				m_PrevPosition		= Vector3.zero;


	// 
	private				void		Awake()
	{
		m_Nodes = FindNodes();

		Vector3 prevPosition = m_Nodes[0];

		IteratePath( 100.0f, 1.0f, 
			( Vector3 position, Quaternion rotation ) => {
				m_PathLength = ( prevPosition - position ).magnitude;
				prevPosition = position;
			}
		);

	}


	// 
	private				Vector3[]	FindNodes()
	{
		List<Vector3> vectors = new List<Vector3>();
		foreach( PathWaypoint child in transform.GetComponentOnlyInChildren<PathWaypoint>() )
		{
			vectors.Add( child.transform.position );
		}

		vectors.Insert( 0, vectors[0] );
		vectors.Insert( vectors.Count-1, vectors[vectors.Count-1] );

		return vectors.ToArray();
	}


	// Spline iteration
	public 	override	void		IteratePath( float Steps, System.Action<Vector3, Quaternion> OnPosition )
	{
		if ( OnPosition == null || Steps > m_PathLength )
		{
			return;
		}

		Vector3 prevPosition = m_Nodes[0];
		float currentStep = 0.001f;
		float stepLength = m_PathLength / Steps;
		while ( currentStep < Steps )
		{
			float interpolant = currentStep / Steps;
			Vector3 position = Utils.Math.GetPoint( m_Nodes, interpolant );
				
			OnPosition( position, Quaternion.identity );

			currentStep += stepLength;
		}
	}


	//
	public	override	bool		Move( float speed, ref Vector3 position )
	{
		if ( m_IsCompleted )
			return false;

		m_Interpolant += ( Time.deltaTime ) * speed;
		position = Utils.Math.GetPoint( m_Nodes, m_Interpolant );

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
	public	override	void		DrawGizmos()
	{
		OnDrawGizmosSelected();
	}
	

	// 
	private				void		OnDrawGizmosSelected()
	{
		m_Nodes = FindNodes();

		Vector3 prevPosition = m_Nodes[0];

		IteratePath( 100.0f, 1.0f, 
			( Vector3 position, Quaternion rotation ) => {
				m_PathLength = ( prevPosition - position ).magnitude;
				Gizmos.DrawLine( prevPosition, position );
				prevPosition = position;
			}
		);
			
	}

}