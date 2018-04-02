using UnityEngine;
using System.Collections.Generic;

public class SlowMoJumpPlate : MonoBehaviour {

	[SerializeField]
	private	bool			m_HasTimeScaleCurveOverride		= false;

	[SerializeField]
	private	AnimationCurve	m_SlowMoJumpTimeScaleCurve		= AnimationCurve.Linear( 0f, 1f, 1f, 1f );


	private		Vector3[] 	m_CurvePoints	= null;


	private void Awake()
	{
		m_CurvePoints = new Vector3[ transform.childCount + 1 ];
		/*
		var waypointsPositions = new List<Vector3>();

		waypointsPositions.Add( transform.position );
		waypointsPositions.Add( transform.position );
		foreach ( Transform t in transform )
		{
			waypointsPositions.Add( t.position );
		}
		waypointsPositions.Add( transform.GetChild( transform.childCount -1 ).position );


		m_CurvePoints = waypointsPositions.ToArray();
		*/
		m_CurvePoints[0] = transform.position;
		for ( int i = 1; i < transform.childCount+1; i++ )
		{
			m_CurvePoints[ i ] = transform.GetChild( i-1 ).position;
		}
		
	}


	private void OnTriggerEnter( Collider other )
	{
		print( other.name );
		Foots foots = other.GetComponent<Foots>();
		if ( foots == null )
			return;

		if ( foots.Onwer is Player )
		{
			if ( foots.Onwer.IsFalling )
			{
				AnimationCurve curve = ( m_HasTimeScaleCurveOverride ) ? m_SlowMoJumpTimeScaleCurve : null;

				( foots.Onwer as Player ).StartSlowMoJump( ref m_CurvePoints, curve );
			}
		}

	}

}
