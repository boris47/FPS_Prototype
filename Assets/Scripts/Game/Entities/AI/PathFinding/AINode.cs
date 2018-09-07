




using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


namespace AI.Pathfinding {

	public interface IAINode {

	}

	[System.Serializable]
	class AINode : MonoBehaviour, IAINode, IHeapItem<AINode> {

		private static  uint                    g_ID        = 0;

		[SerializeField, ReadOnly]
		public    uint                        ID          = 0;

		[SerializeField, HideInInspector]
		public    AINode                      Parent      = null;

		///
		/// PATHFINDING		START
		///
		[SerializeField, HideInInspector]
		public    AINode                      Linker  = null;
		public    float                       gCost       = 0.0f;
		public    float                       Heuristic   = 0.0f;
		public float fCost
		{
			get {
				return gCost + Heuristic;
			}
		}
		public Vector3 Position
		{
			get {
				return transform.position;
			}
		}
		public    bool                        Visited     = false;
		///
		/// PATHFINDING		END
		///


		[SerializeField, ReadOnly]
		public		List<AINode>					Neighbours          = new List<AINode>();

		[SerializeField, ReadOnly]
		private		bool							m_IsWalkable        = true;

		[SerializeField, ReadOnly]
		public		NavMeshVolume					Volume              = null;


		public bool IsWalkable
		{
			get {
				return m_IsWalkable;
			}
			set {
				m_IsWalkable = value;
			}
		}

		private     int                         m_HeapIndex;
		int IHeapItem<AINode>.HeapIndex
		{
			get {
				return m_HeapIndex;
			}
			set {
				m_HeapIndex = value;
			}
		}

		public void SetID()
		{
			ID = g_ID;
			g_ID++;
		}

		int IComparable<AINode>.CompareTo( AINode other )
		{
			int compare = this.fCost.CompareTo( other.fCost );
			if ( compare == 0 )
			{
				compare = this.Heuristic.CompareTo( other.Heuristic );
			}
			return -compare;
		}


		bool IEquatable<AINode>.Equals( AINode other )
		{
			return this.ID == other.ID;
		}

		private void OnDrawGizmosSelected()
		{
			if ( this == null )
				return;

			if ( Neighbours != null && Neighbours.Count > 0 )
			{
				foreach ( AINode neigh in Neighbours )
				{
					if ( neigh != null )
					{
						Gizmos.DrawLine( transform.position, neigh.transform.position );
					}
				}
			}
		}
	}

}