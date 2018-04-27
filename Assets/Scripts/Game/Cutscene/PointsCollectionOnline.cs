
using UnityEngine;
using System.Collections.Generic;



public class PointsCollectionOnline : MonoBehaviour {
	
//	public		bool								UseNormalizedTime	= false;

	public		Entity.SimulationMovementType		EntityState			= Entity.SimulationMovementType.WALK;
	
	public		List<Vector3>						Destinations		= new List<Vector3>();

	public		List<Transform>						Targets				= new List<Transform>();

	private void Awake()
	{
		Transform positionsContainer		= transform.Find( "Positions" );
		Transform targetsPositionContainer	= transform.Find( "TargetPositions" );

		foreach( Transform t in positionsContainer )
		{
			Destinations.Add( t.position );
		}

		foreach( Transform t in targetsPositionContainer )
		{
			Targets.Add( t );
		}
	}
}