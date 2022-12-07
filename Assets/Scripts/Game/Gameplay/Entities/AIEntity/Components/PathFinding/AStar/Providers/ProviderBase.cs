
using UnityEngine;

namespace AI.Pathfinding
{
	internal abstract class ProviderBase : MonoBehaviour
	{
		public abstract Vector3[] GetNodesPosition();
	}
}
