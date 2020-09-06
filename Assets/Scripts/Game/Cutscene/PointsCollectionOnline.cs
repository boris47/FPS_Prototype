
using UnityEngine;
using System.Collections.Generic;

namespace CutScene {

	[System.Serializable]
	public class CutsceneWaypointData {
		[HideInInspector]
		public	string							Name				= "CutsceneWaypointData";
		public	ESimMovementType					movementType		= ESimMovementType.WALK;
		public	Transform						point				= null;
		public	Transform						target				= null;
		[Range( 0.01f, 1f )]
		public	float							timeScaleTraget		= 1.0f;
		public	bool							zoomEnabled			= false;
		public	Waiter_Base						waiter				= null;

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
				return this.m_Waypoints[ index ];
			}
		}

		public	int	Count
		{
			get { return this.m_Waypoints.Length; }
		}

		private void OnEnable()
		{
			if (this.m_Waypoints.Length == 0 )
				return;

			for ( int i = 0; i < this.m_Waypoints.Length; i++ )
			{
				CutsceneWaypointData wayPoint = this.m_Waypoints[i];
				if ( wayPoint.movementType == ESimMovementType.STATIONARY && wayPoint.waiter == null )
				{
					print( "PointsCollectionOnline::OnEnable: Collection " + this.name + " has stationary waypoints without waiter assigned at index " + i );
#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false;
#endif
				}
			}
		}


		private void OnDrawGizmosSelected()
		{
			if (this.m_Waypoints == null || this.m_Waypoints.Length < 2 )
				return;

			Vector3 prevPosition = this.m_Waypoints[0].point.position;
			for ( int i = 1; i < this.m_Waypoints.Length; i++ )
			{
				if (this.m_Waypoints[ i ].point != null )
				{
					Vector3 currentPosition = this.m_Waypoints[ i ].point.position;

					Gizmos.DrawLine( prevPosition, currentPosition );

					prevPosition = currentPosition;
				}				
			}
		}
	}

}