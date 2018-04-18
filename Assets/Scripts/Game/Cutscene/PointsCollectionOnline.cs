
using UnityEngine;
using System.Collections.Generic;



public class PointsCollectionOnline : MonoBehaviour {
	
//	public		bool								UseNormalizedTime	= false;

	public		Entity.SimulationMovementType		EntityState			= Entity.SimulationMovementType.WALK;
	
	public		List<Vector3>						Positions			= new List<Vector3>();

	public		List<Vector3>						TargetPositions		= new List<Vector3>();

	private void Awake()
	{
		Transform positionsContainer		= transform.Find( "Positions" );
		Transform targetsPositionContainer	= transform.Find( "TargetPositions" );

		foreach( Transform t in positionsContainer )
		{
			Positions.Add( t.position );
		}

		foreach( Transform t in targetsPositionContainer )
		{
			TargetPositions.Add( t.position );
		}
	}
}