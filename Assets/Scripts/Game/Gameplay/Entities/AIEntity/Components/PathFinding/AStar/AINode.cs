
using UnityEngine;

namespace AI.Pathfinding
{
	internal partial class AINode : MonoBehaviour, IHeapItem<AINode>
	{
		private	static	uint					s_ID				= 0u;

		public		AINode						Parent				{ get; set; } = null;
		public		float						gCost				{ get; set; } = 0f;
		public		float						Heuristic			{ get; set; } = 0f;
		public		float						fCost				=> gCost + Heuristic;
		public		Vector3						Position			=> transform.position;
		public		bool						Visited				{ get; set; } = false;

		[SerializeField]
		protected	bool						m_IsWalkable		= true;
		public		bool						IsWalkable			{ get => m_IsWalkable; set => m_IsWalkable = value; }

		[SerializeField, ReadOnly]
		private		AINode[]					m_Neighbors			= null;
		public		AINode[]					Neighbours			{ get => m_Neighbors; }

		private		int							m_HeapIndex;
		int							IHeapItem<AINode>.HeapIndex
		{
			get => m_HeapIndex;
			set => m_HeapIndex = value;
		}

		private		uint						m_ID				= s_ID++;
		public		uint						ID					=> m_ID;


		//////////////////////////////////////////////////////////////////////////
		int System.IComparable<AINode>.CompareTo(AINode other)
		{
			int compare = this.fCost.CompareTo(other.fCost);
			if (compare == 0)
			{
				compare = this.Heuristic.CompareTo(other.Heuristic);
			}
			return -compare;
		}

		//////////////////////////////////////////////////////////////////////////
		bool System.IEquatable<AINode>.Equals(AINode other) => m_ID == other.ID;
	}
}
