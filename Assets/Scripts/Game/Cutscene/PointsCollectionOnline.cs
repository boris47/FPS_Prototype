﻿
using UnityEngine;
using System.Collections.Generic;

namespace CutScene {

	[System.Serializable]
	public class CutsceneWaypointData {
		public	Entity.SimMovementType			movementType		= Entity.SimMovementType.WALK;
		public	Transform						point				= null;
		public	Transform						target				= null;
		[Range( 0.01f, 1f )]
		public	float							timeScaleTraget		= 1f;

		[SerializeField]
		public	GameEvent						OnWayPointReached	= null;
	}


	public class PointsCollectionOnline : MonoBehaviour {

		[SerializeField]
		public	GameEvent					OnStart			= null;

		[SerializeField]
		private		CutsceneWaypointData[]	m_Waypoints		= null;

		// INDEXER
		public		CutsceneWaypointData	this[int index]
		{
			get { 
				return m_Waypoints[ index ];
			}
		}

		public	int	Count
		{
			get { return m_Waypoints.Length; }
		}
	}

}