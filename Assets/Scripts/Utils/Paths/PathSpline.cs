
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
		Vector3[] positions = null;
		{
			m_Nodes = transform.GetComponentsInChildren<PathWaypoint>();

			List<Vector3> positionsList = new List<Vector3>( m_Nodes.Length );
			{
				positionsList.Add( m_Nodes.First().transform.position );
					System.Array.ForEach( m_Nodes, ( PathWaypoint w ) => { positionsList.Add( w.transform.position ); } );
				positionsList.Add( m_Nodes.Last().transform.position );
			}
			positions = positionsList.ToArray();
		}

		List<Vector3> allNeededPositions = new List<Vector3>();
		m_PathLength = 0.0f;

		Vector3 prevPosition = positions[0];
		float currentStep = 0.001f;
		while ( currentStep < Steps )
		{
			float interpolant = currentStep / Steps;
			Vector3 currentPosition = Utils.Math.GetPoint( positions, interpolant );

			allNeededPositions.Add( currentPosition );
			m_PathLength += Vector3.Distance( prevPosition, currentPosition );

			prevPosition = currentPosition;
			currentStep += StepLength;
		}

		m_Positions = allNeededPositions.ToArray();
	}


	// Spline iteration
	public 	override	void			IteratePath( System.Action<Vector3, Quaternion> OnPosition  )
	{
		if ( OnPosition == null )
		{
			return;
		}

		System.Array.ForEach
		(	m_Positions, ( Vector3 position ) => {
				OnPosition( position, Quaternion.identity );
			}
		);
	}


	//
	public	override	bool			Move( float speed, ref Vector3 position )
	{
		if ( m_IsCompleted )
			return false;

		if ( m_Interpolant == 0.0f )
		{
			if ( m_OnPathStart != null && m_OnPathStart.GetPersistentEventCount() > 0 )
			{
				m_OnPathStart.Invoke();
			}
		}

		m_Interpolant += ( Time.deltaTime ) * speed;

		if ( m_Interpolant >= 1.0f )
		{
			m_IsCompleted = true;

			if ( m_OnPathCompleted != null && m_OnPathCompleted.GetPersistentEventCount() > 0 )
			{
				m_OnPathCompleted.Invoke();
			}
		}

		position = Utils.Math.GetPoint( m_Positions, m_Interpolant );
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