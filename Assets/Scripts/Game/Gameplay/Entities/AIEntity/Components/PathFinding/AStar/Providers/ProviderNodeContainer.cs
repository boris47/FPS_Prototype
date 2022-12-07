
using System.Linq;
using UnityEngine;

namespace AI.Pathfinding
{
	internal class ProviderNodeContainer : ProviderBase
	{
		[SerializeField]
		private Transform[] m_Nodes = null;

		public override Vector3[] GetNodesPosition() => m_Nodes.Select(t => t.position).ToArray();
	}
}
