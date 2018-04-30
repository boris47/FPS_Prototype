
using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class CutsceneWaypointData {
	public	Entity.SimulationMovementType	movementType	= Entity.SimulationMovementType.WALK;
	public	Transform						point			= null;
	public	Transform						target			= null;
}


public class PointsCollectionOnline : MonoBehaviour {
	
//	public		bool								UseNormalizedTime	= false;

	[SerializeField]
	private		List<CutsceneWaypointData>			m_Waypoints			= new List<CutsceneWaypointData>();

	// INDEXER
	public		CutsceneWaypointData	this[int index]
	{
		get { 
			return m_Waypoints[ index ];
		}
	}

	public	int	Count
	{
		get { return m_Waypoints.Count; }
	}
}