using System.Collections;
using UnityEngine;

namespace Entities.AI
{
	using Entities.AI.Components;

	public sealed class EntityLostData
	{
		public readonly Vector3 LastPosition = Vector3.zero;
		public readonly Vector3 LastDirection = Vector3.zero;
		public readonly Vector3 ViewerPosition = Vector3.zero;

		public EntityLostData(in Vector3 InLastPosition, in Vector3 InLastDirection, in Vector3 InViewerPosition)
		{
			LastPosition = InLastPosition;
			LastDirection = InLastDirection;
			ViewerPosition = InViewerPosition;
		}
	}

	[System.Serializable]
	public class BBEntry_EntityLostData : BlackboardEntryKeyValue<MemoryIdentifier>
	{

	}
}

