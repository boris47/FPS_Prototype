﻿
using UnityEngine;
using System.Collections.Generic;

namespace CutScene {

	[System.Serializable]
	public class CutsceneWaypointData {
		[HideInInspector]
		public	string							Name				= "CutsceneWaypointData";
		public	SimMovementType					movementType		= SimMovementType.WALK;
		public	Transform						point				= null;
		public	Transform						target				= null;
		[Range( 0.01f, 1f )]
		public	float							timeScaleTraget		= 1.0f;
		public	bool							zoomEnabled			= false;
		public	Cutscene_Waiter_Base			waiter				= null;

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


		private void OnDrawGizmosSelected()
		{
			if ( m_Waypoints == null || m_Waypoints.Length < 2 )
				return;

			Vector3 prevPosition = m_Waypoints[0].point.position;
			for ( int i = 1; i < m_Waypoints.Length; i++ )
			{
				if ( m_Waypoints[ i ].point != null )
				{
					Vector3 currentPosition = m_Waypoints[ i ].point.position;

					Gizmos.DrawLine( prevPosition, currentPosition );

					prevPosition = currentPosition;
				}				
			}
		}
	}

}