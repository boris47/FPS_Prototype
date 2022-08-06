
using UnityEngine;
using System.Collections.Generic;

namespace AI.Pathfinding
{
	internal class ProviderIterableVolume : ProviderBase
	{
		[SerializeField]
		private IterableVolume m_Volume = null;

		public override AINode[] PickNodes()
		{
			return new AINode[0];
		}
	}
}
