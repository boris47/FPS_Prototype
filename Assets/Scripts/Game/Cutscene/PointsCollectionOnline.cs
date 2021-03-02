
using UnityEngine;

namespace CutScene
{
	[System.Serializable]
	public class CutsceneWaypointData
	{
		[HideInInspector]
		public	string							Name				= "CutsceneWaypointData";
		public	ESimMovementType				movementType		= ESimMovementType.WALK;
		public	Transform						point				= null;
		public	Transform						target				= null;
		[Range(0.01f, 1f)]
		public	float							timeScaleTraget		= 1.0f;
		public	bool							zoomEnabled			= false;
		public	Waiter_Base						waiter				= null;

		[SerializeField]
		public	GameEvent						OnWayPointReached	= null;
	}

	public class PointsCollectionOnline : MonoBehaviour
	{
		// INDEXER
		public		CutsceneWaypointData			this[int index] => m_Waypoints[index];

		public		int								Count => m_Waypoints.Length;


		[SerializeField]
		public		GameEvent						OnStart			= null;

		[SerializeField]
		private		CutsceneWaypointData[]			m_Waypoints		= null;


		private void OnEnable()
		{
			for (int i = 0; i < m_Waypoints.Length; i++)
			{
				CutsceneWaypointData wayPoint = m_Waypoints[i];
				if (wayPoint.movementType == ESimMovementType.STATIONARY && wayPoint.waiter.IsNotNull())
				{
					print( $"PointsCollectionOnline::OnEnable: Collection {name} has stationary waypoints without waiter assigned at index {i}" );
#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false;
#endif
				}
			}
		}


		private void OnDrawGizmosSelected()
		{
			if (m_Waypoints.IsNotNull() && m_Waypoints.Length > 1)
			{
				Vector3 prevPosition = m_Waypoints[0].point.position;
				for (int i = 1; i < m_Waypoints.Length; i++)
				{
					if (m_Waypoints[i].point.IsNotNull())
					{
						Vector3 currentPosition = m_Waypoints[i].point.position;
						Gizmos.DrawLine(prevPosition, currentPosition);
						prevPosition = currentPosition;
					}				
				}
			}
		}
	}

}