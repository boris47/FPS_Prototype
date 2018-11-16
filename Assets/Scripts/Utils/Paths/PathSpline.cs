
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathSpline : PathBase {

	[SerializeField]
	private		PathWaypoint[]		m_Nodes				= null;


	// 
	private				void			Awake()
	{
		ElaboratePath( Steps: 100f );
	}



	//
	protected	override	void			ElaboratePath( float Steps, float StepLength = 1.0f )
	{
		m_Nodes = transform.GetComponentsInChildren<PathWaypoint>();

		m_PathLength = 0.0f;
		
		List<Vector3> positionsList = new List<Vector3>();

		Vector3 prevPosition = m_Nodes[0].transform.position;
		float currentStep = 0.001f;
		while ( currentStep < Steps )
		{
			float interpolant = currentStep / Steps;
			Vector3 currentPosition = Utils.Math.GetPoint( m_Positions, interpolant );

			positionsList.Add( currentPosition );
			m_PathLength += Vector3.Distance( prevPosition, currentPosition );

			prevPosition = currentPosition;
			currentStep += StepLength;
		}

		m_Positions = positionsList.ToArray();
	}


	// Spline iteration
	public 	override	void			IteratePath( System.Action<Vector3, Quaternion> OnPosition  )
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
	public	override	bool			Move( float speed, ref Vector3 position )
	{
		if ( m_IsCompleted )
			return false;

		m_Interpolant += ( Time.deltaTime ) * speed;
		position = Utils.Math.GetPoint( m_Positions, m_Interpolant );

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
		ElaboratePath( Steps: 100f );

		Vector3 prevPosition = m_Nodes[0].transform.position;

		IteratePath(
			OnPosition: ( Vector3 position, Quaternion rotation ) => {
				m_PathLength = ( prevPosition - position ).magnitude;
				Gizmos.DrawLine( prevPosition, position );
				prevPosition = position;
			}
		);
		
	}

}