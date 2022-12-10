
using System.Linq;
using UnityEngine;

namespace AI.Pathfinding
{
	internal class ProviderIterableVolume : ProviderBase
	{
		[SerializeField]
		private IterableVolume m_Volume = null;

		public override Vector3[] GetNodesPosition() => m_Volume.GetPoints();
	}
}
