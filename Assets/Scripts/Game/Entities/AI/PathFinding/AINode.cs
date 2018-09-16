




using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


namespace AI.Pathfinding {

	interface IAINode : IHeapItem<IAINode> {
		uint							ID			{ get; }
		float							GCost		{ get; set; }
		float							Heuristic	{ get; set; }
		float							FCost		{ get;		}
		AINode							Parent		{ get; set; }
		bool							Visited		{ get; set; }
		List<AINode>					Neighbours	{ get;		}

		Vector3							Position	{ get;		}
		bool							IsWalkable	{ get;		}
		NavMeshVolume					Volume		{ get; set; }

	}

	[System.Serializable]
	class AINode : MonoBehaviour, IAINode {

		private static  uint					g_ID			= 0;

		[SerializeField, ReadOnly]
		public		uint						ID				= 0;

		[SerializeField, ReadOnly]
		private     List<AINode>                m_Neighbours    = new List<AINode>();

		[SerializeField, ReadOnly]
		public      NavMeshVolume               m_Volume		= null;

		[SerializeField]
		private     bool                        m_IsWalkable    = true;





		///
		/// PATHFINDING		START
		///
		//		[SerializeField, ReadOnly]
		//		private		AINode						Linker			= null;

		// Costs
		private		float						m_GCost			= 0.0f;
		private		float						m_Heuristic		= 0.0f;
		private		float						m_FCost			{ get { return m_GCost + m_Heuristic; }	}
		
		private		AINode						m_Parent		= null;
		private		bool						m_Visited		= false;
		private		Vector3						m_Position		{ get {	return transform.position; } }

		private		int							m_HeapIndex		= -1;



		///
		/// PATHFINDING		END
		///
		
		///
		/// AI NODE INTERFACE	START
		///
					uint				IAINode.ID				{ get { return ID; } }
					float				IAINode.GCost			{ get { return m_GCost; }		set { m_GCost = value; }		}
					float				IAINode.Heuristic		{ get { return m_Heuristic; }	set { m_Heuristic = value; }	}
					float				IAINode.FCost			{ get { return m_FCost; }										}
					AINode				IAINode.Parent			{ get { return m_Parent; }		set { m_Parent = value; }		}
					bool				IAINode.Visited			{ get { return m_Visited; }		set { m_Visited = value; }		}
					List<AINode>		IAINode.Neighbours		{ get { return m_Neighbours; } }

					Vector3				IAINode.Position		{ get { return m_Position; }									}
					bool				IAINode.IsWalkable		{ get { return m_IsWalkable; }									}
					NavMeshVolume		IAINode.Volume			{ get { return m_Volume; }		set { m_Volume = value; }		}
		///
		/// AI NODE INTERFACE	END
		///

					int		IHeapItem<IAINode>.HeapIndex		{ get { return m_HeapIndex;	}	set { m_HeapIndex = value; }	}

		


		public		bool						IsWalkable		{ get {	return m_IsWalkable; }	set { m_IsWalkable = value; }	}
		public		NavMeshVolume				Volume			{ get { return m_Volume; }										}


		public void SetID()
		{
			ID = g_ID;
			g_ID++;
		}


		// Questa funzione viene chiamata durante il caricamento dello script o quando si modifica un valore nell'inspector (chiamata solo nell'editor)
		private void OnValidate()
		{
			m_HeapIndex = 0;
		}

		int IComparable<IAINode>.CompareTo( IAINode other )
		{
			int compare = this.m_FCost.CompareTo( other.FCost );
			if ( compare == 0 )
			{
				compare = this.m_Heuristic.CompareTo( other.Heuristic );
			}
			return -compare;
		}


		bool IEquatable<IAINode>.Equals( IAINode other )
		{
			return this.ID == other.ID;
		}

		public	void	ResetNode()
		{
			m_Heuristic		= 0.0f;
			m_GCost			= float.MaxValue;
			m_Parent		= null;
			m_Visited		= false;
		}

		private void OnDrawGizmosSelected()
		{
			if ( this == null )
				return;

			if ( m_Neighbours != null && m_Neighbours.Count > 0 )
			{
				foreach ( AINode neigh in m_Neighbours )
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